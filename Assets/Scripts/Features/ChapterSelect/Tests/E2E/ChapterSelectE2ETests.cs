using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UniversalPlatform.Features.ChapterSelect.Data;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.ChapterSelect.Presentation.Clock;
using UniversalPlatform.Features.ChapterSelect.UI.Clock;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.E2E
{
    /// <summary>
    /// PlayMode: vista real + ViewModel real + use cases reales (progreso en memoria).
    /// El mouse se simula llamando a los handlers de puntero de los números del reloj.
    /// </summary>
    public class ChapterSelectE2ETests
    {
        private class ProgresoFalso : ProgresoRepositoryPort
        {
            public ProgresoCapitulo ObtenerProgreso(string id) => ProgresoCapitulo.Nuevo(id);
            public void GuardarProgreso(ProgresoCapitulo p) { }
            public bool HayPartidaGuardada() => false;
            public void ReiniciarPartida() { }
        }

        private GameObject _raiz;
        private ClockNumberView[] _numeros;
        private RectTransform _horaria, _minutero;
        private string _escenaCargada;

        private static void SetPrivate(object objetivo, string campo, object valor)
        {
            var f = objetivo.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(f, $"No existe el campo {campo}");
            f.SetValue(objetivo, valor);
        }

        private static PointerEventData Puntero() =>
            new PointerEventData(null) { button = PointerEventData.InputButton.Left };

        private static RectTransform CrearRect(string nombre, Transform padre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            return (RectTransform)go.transform;
        }

        [SetUp]
        public void SetUp()
        {
            _escenaCargada = null;
            _raiz = new GameObject("ClockTest", typeof(RectTransform));

            _numeros = new ClockNumberView[12];
            for (int i = 0; i < 12; i++)
                _numeros[i] = CrearRect($"N{i + 1}", _raiz.transform).gameObject.AddComponent<ClockNumberView>();

            _horaria = CrearRect("Horaria", _raiz.transform);
            _minutero = CrearRect("Minutero", _raiz.transform);

            var vista = _raiz.AddComponent<ChapterSelectView>();
            SetPrivate(vista, "_numeros", _numeros);
            SetPrivate(vista, "_manillaHoraria", _horaria);
            SetPrivate(vista, "_manillaMinutero", _minutero);

            var capitulos = new CapitulosDataRepository(new CapitulosDefaultDataSource());
            var vm = new ChapterSelectViewModel(
                new ObtenerCapitulosUseCase(capitulos),
                new ObtenerDetalleCapituloUseCase(capitulos, new ObtenerProgresoCapituloUseCase(new ProgresoFalso())),
                new ValidarEntradaCapituloUseCase(capitulos));

            vista.Construir(vm, escena => _escenaCargada = escena);
        }

        [TearDown]
        public void TearDown()
        {
            if (_raiz != null) Object.Destroy(_raiz);
        }

        [UnityTest]
        public IEnumerator AlIniciar_LasManillasBarrenHastaLaUna()
        {
            // SmoothDamp converge de forma exponencial: el minutero recorre 360° y necesita ~2 s para asentarse.
            yield return new WaitForSeconds(2.5f);

            // Horaria en la 1 = 30° horarios = -30° de rotación Z en Unity.
            Assert.AreEqual(0f, Mathf.DeltaAngle(_horaria.localEulerAngles.z, -30f), 1f);
            // El minutero da una vuelta completa y termina en las 12.
            Assert.AreEqual(0f, Mathf.DeltaAngle(_minutero.localEulerAngles.z, 0f), 1f);
        }

        [UnityTest]
        public IEnumerator PasarElMouseSobreElSeis_MueveLaHorariaAlSeis()
        {
            yield return new WaitForSeconds(1f);

            // Del 1 al 6 el minutero da 5 vueltas (1800°): con SmoothDamp necesita ~4 s para asentarse.
            _numeros[5].OnPointerEnter(Puntero());
            yield return new WaitForSeconds(4f);

            Assert.AreEqual(0f, Mathf.DeltaAngle(_horaria.localEulerAngles.z, -180f), 1f);
            Assert.AreEqual(0f, Mathf.DeltaAngle(_minutero.localEulerAngles.z, 0f), 1f, "El minutero queda en las 12.");
        }

        [UnityTest]
        public IEnumerator PasarElMouse_IluminaElNumeroYApagaElAnterior()
        {
            _numeros[2].OnPointerEnter(Puntero());
            yield return new WaitForSeconds(0.3f);

            Assert.Greater(_numeros[2].transform.localScale.x, 1.1f, "El número seleccionado crece.");
            Assert.AreEqual(1f, _numeros[0].transform.localScale.x, 0.01f, "El anterior vuelve a su tamaño.");
        }

        [UnityTest]
        public IEnumerator ClicEnElUno_EntraAlCapituloUno()
        {
            _numeros[0].OnPointerClick(Puntero());
            yield return null;

            Assert.AreEqual("Chapter1", _escenaCargada);
        }

        [UnityTest]
        public IEnumerator ClicEnUnCapituloBloqueado_NoEntra()
        {
            _numeros[4].OnPointerClick(Puntero());
            yield return null;

            Assert.IsNull(_escenaCargada);
        }
    }
}

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.Presentation.MainMenu;
using UniversalPlatform.Features.MainMenu.UI.MainMenu;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.E2E
{
    /// <summary>
    /// PlayMode: vista real + ViewModel real + use cases reales, con un repositorio en memoria.
    /// Se simula el mouse llamando a los handlers de puntero de los botones.
    /// </summary>
    public class MainMenuE2ETests
    {
        private class RepositorioFalso : ProgresoRepositoryPort
        {
            public bool PartidaGuardada;
            public ProgresoCapitulo ObtenerProgreso(string idCapitulo) => ProgresoCapitulo.Nuevo(idCapitulo);
            public void GuardarProgreso(ProgresoCapitulo progreso) { }
            public bool HayPartidaGuardada() => PartidaGuardada;
            public void ReiniciarPartida() { PartidaGuardada = true; }
        }

        private GameObject _raiz;
        private MenuButtonView[] _botones;
        private string _escenaCargada;
        private bool _salio;

        private static void SetPrivate(object objetivo, string campo, object valor)
        {
            var f = objetivo.GetType().GetField(campo, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(f, $"No existe el campo {campo}");
            f.SetValue(objetivo, valor);
        }

        private static PointerEventData Puntero() =>
            new PointerEventData(null) { button = PointerEventData.InputButton.Left };

        [SetUp]
        public void SetUp()
        {
            _escenaCargada = null;
            _salio = false;

            _raiz = new GameObject("MenuTest");
            _botones = new MenuButtonView[4];
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject($"Btn{i}", typeof(RectTransform));
                go.transform.SetParent(_raiz.transform, false);
                _botones[i] = go.AddComponent<MenuButtonView>();
            }

            var vista = _raiz.AddComponent<MainMenuView>();
            SetPrivate(vista, "_botones", _botones);
            SetPrivate(vista, "_escenaSeleccionCapitulo", "EscenaDePrueba");

            var repo = new RepositorioFalso();
            var hayPartida = new HayPartidaGuardadaUseCase(repo);
            var vm = new MainMenuViewModel(
                new EjecutarOpcionMenuUseCase(new IniciarNuevaPartidaUseCase(repo), hayPartida),
                hayPartida);

            vista.Construir(vm, escena => _escenaCargada = escena, () => _salio = true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_raiz != null) Object.Destroy(_raiz);
        }

        [UnityTest]
        public IEnumerator PasarElMouse_IluminaYEscalaElBoton()
        {
            _botones[1].OnPointerEnter(Puntero());
            yield return new WaitForSeconds(0.3f);

            Assert.Greater(_botones[1].transform.localScale.x, 1.05f, "El botón resaltado debe crecer.");
            Assert.AreEqual(1f, _botones[0].transform.localScale.x, 0.001f, "Los demás siguen en escala normal.");
        }

        [UnityTest]
        public IEnumerator SacarElMouse_VuelveAEscalaNormal()
        {
            _botones[2].OnPointerEnter(Puntero());
            yield return new WaitForSeconds(0.3f);

            _botones[2].OnPointerExit(Puntero());
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(1f, _botones[2].transform.localScale.x, 0.01f);
        }

        [UnityTest]
        public IEnumerator ClicEnNuevaPartida_CargaLaEscenaDeSeleccionDeCapitulo()
        {
            _botones[0].OnPointerClick(Puntero());
            yield return null;

            Assert.AreEqual("EscenaDePrueba", _escenaCargada);
        }

        [UnityTest]
        public IEnumerator ClicEnSalir_PideCerrarElJuego()
        {
            _botones[3].OnPointerClick(Puntero());
            yield return null;

            Assert.IsTrue(_salio);
            Assert.IsNull(_escenaCargada);
        }

        [UnityTest]
        public IEnumerator ClicEnCargarSinGuardado_NoCambiaDeEscena()
        {
            _botones[1].OnPointerClick(Puntero());
            yield return null;

            Assert.IsNull(_escenaCargada);
        }
    }
}

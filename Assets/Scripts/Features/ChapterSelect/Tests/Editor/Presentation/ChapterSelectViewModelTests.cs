using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Data;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.ChapterSelect.Presentation.Clock;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Editor.Presentation
{
    public class ChapterSelectViewModelTests
    {
        private class ProgresoFalso : ProgresoRepositoryPort
        {
            public ProgresoCapitulo ObtenerProgreso(string id) => ProgresoCapitulo.Nuevo(id);
            public void GuardarProgreso(ProgresoCapitulo p) { }
            public bool HayPartidaGuardada() => false;
            public void ReiniciarPartida() { }
        }

        private ChapterSelectViewModel _vm;
        private List<Capitulo> _entradas;
        private int _bloqueos;
        private int _volverAlMenu;
        private int _cambiosDeEstado;

        [SetUp]
        public void SetUp()
        {
            var capitulos = new CapitulosDataRepository(new CapitulosDefaultDataSource());
            var progreso = new ObtenerProgresoCapituloUseCase(new ProgresoFalso());

            _vm = new ChapterSelectViewModel(
                new ObtenerCapitulosUseCase(capitulos),
                new ObtenerDetalleCapituloUseCase(capitulos, progreso),
                new ValidarEntradaCapituloUseCase(capitulos));

            _entradas = new List<Capitulo>();
            _bloqueos = _volverAlMenu = _cambiosDeEstado = 0;
            _vm.OnEntrarCapitulo += c => _entradas.Add(c);
            _vm.OnCapituloBloqueado += () => _bloqueos++;
            _vm.OnVolverAlMenu += () => _volverAlMenu++;
            _vm.OnStateChanged += _ => _cambiosDeEstado++;
        }

        [Test]
        public void Inicializar_SeleccionaElUnoYApuntaLasManillas()
        {
            _vm.Inicializar();

            var e = _vm.EstadoActual;
            Assert.AreEqual(1, e.NumeroSeleccionado);
            Assert.AreEqual(30f, e.AnguloHorariaObjetivo, 0.001f);
            Assert.AreEqual(360f, e.AnguloMinuteroObjetivo, 0.001f);   // una hora = una vuelta de minutero
            Assert.IsTrue(e.NumeroEnLadoDerecho);
        }

        [Test]
        public void Inicializar_SoloElCapituloUnoQuedaDesbloqueado()
        {
            _vm.Inicializar();

            var b = _vm.EstadoActual.BloqueadosPorNumero;
            Assert.AreEqual(12, b.Count);
            Assert.IsFalse(b[0]);
            for (int i = 1; i < 12; i++) Assert.IsTrue(b[i], $"El número {i + 1} debe estar bloqueado");
        }

        [Test]
        public void Inicializar_MuestraElDetalleDelCapituloConTiempoCero()
        {
            _vm.Inicializar();

            var d = _vm.EstadoActual.Detalle;
            Assert.AreEqual("Capítulo 1", d.Titulo);
            Assert.AreEqual("0 / 3", d.Llaves);
            Assert.AreEqual("00:00:00", d.Tiempo);
        }

        [Test]
        public void Mover_Adelante_AvanzaUnaHoraYElMinuteroDaOtraVuelta()
        {
            _vm.Inicializar();
            _vm.Mover(+1);

            var e = _vm.EstadoActual;
            Assert.AreEqual(2, e.NumeroSeleccionado);
            Assert.AreEqual(60f, e.AnguloHorariaObjetivo, 0.001f);
            Assert.AreEqual(720f, e.AnguloMinuteroObjetivo, 0.001f);
            Assert.AreEqual(1f, e.PasosRecorridos, 0.001f);
        }

        [Test]
        public void Mover_AtrasDesdeElUno_VaAlDoceSinDarLaVueltaLarga()
        {
            _vm.Inicializar();
            _vm.Mover(-1);

            var e = _vm.EstadoActual;
            Assert.AreEqual(12, e.NumeroSeleccionado);
            Assert.AreEqual(0f, e.AnguloHorariaObjetivo, 0.001f);
            Assert.IsFalse(e.NumeroEnLadoDerecho);
        }

        [Test]
        public void Seleccionar_ConMouse_SaltaAlNumeroPorElCaminoCorto()
        {
            _vm.Inicializar();
            _vm.Seleccionar(6);

            Assert.AreEqual(6, _vm.EstadoActual.NumeroSeleccionado);
            Assert.AreEqual(180f, _vm.EstadoActual.AnguloHorariaObjetivo, 0.001f);
            Assert.AreEqual(5f, _vm.EstadoActual.PasosRecorridos, 0.001f);
        }

        [Test]
        public void Seleccionar_ElMismoNumero_NoRepublicaElEstado()
        {
            _vm.Inicializar();
            int antes = _cambiosDeEstado;

            _vm.Seleccionar(1);

            Assert.AreEqual(antes, _cambiosDeEstado);
        }

        [Test]
        public void Seleccionar_NumeroInvalido_SeIgnora()
        {
            _vm.Inicializar();
            _vm.Seleccionar(0);
            _vm.Seleccionar(13);

            Assert.AreEqual(1, _vm.EstadoActual.NumeroSeleccionado);
        }

        [Test]
        public void Confirmar_EnCapituloDisponible_EmiteEntrarConSuEscena()
        {
            _vm.Inicializar();
            _vm.Confirmar();

            Assert.AreEqual(1, _entradas.Count);
            Assert.AreEqual("Chapter1", _entradas[0].NombreEscena);
            Assert.AreEqual(0, _bloqueos);
        }

        [Test]
        public void Confirmar_EnCapituloBloqueado_EmiteBloqueadoYNoEntra()
        {
            _vm.Inicializar();
            _vm.Seleccionar(5);
            _vm.Confirmar();

            Assert.AreEqual(0, _entradas.Count);
            Assert.AreEqual(1, _bloqueos);
        }

        [Test]
        public void Confirmar_SinInicializar_NoHaceNada()
        {
            _vm.Confirmar();

            Assert.AreEqual(0, _entradas.Count);
            Assert.AreEqual(0, _bloqueos);
        }

        [Test]
        public void ConfirmarNumero_SeleccionaYEntra()
        {
            _vm.Inicializar();
            _vm.Seleccionar(4);

            _vm.ConfirmarNumero(1);

            Assert.AreEqual(1, _vm.EstadoActual.NumeroSeleccionado);
            Assert.AreEqual(1, _entradas.Count);
        }

        [Test]
        public void Cancelar_PideVolverAlMenu()
        {
            _vm.Cancelar();
            Assert.AreEqual(1, _volverAlMenu);
        }
    }
}

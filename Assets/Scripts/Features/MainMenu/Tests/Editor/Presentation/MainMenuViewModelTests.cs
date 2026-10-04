using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.Presentation.MainMenu;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.Editor.Presentation
{
    public class MainMenuViewModelTests
    {
        private class RepositorioFalso : ProgresoRepositoryPort
        {
            public bool PartidaGuardada;
            public ProgresoCapitulo ObtenerProgreso(string idCapitulo) => ProgresoCapitulo.Nuevo(idCapitulo);
            public void GuardarProgreso(ProgresoCapitulo progreso) { }
            public bool HayPartidaGuardada() => PartidaGuardada;
            public void ReiniciarPartida() { PartidaGuardada = true; }
        }

        private RepositorioFalso _repo;
        private MainMenuViewModel _vm;
        private List<ResultadoAccionMenu> _acciones;

        [SetUp]
        public void SetUp()
        {
            _repo = new RepositorioFalso();
            var hayPartida = new HayPartidaGuardadaUseCase(_repo);
            _vm = new MainMenuViewModel(
                new EjecutarOpcionMenuUseCase(new IniciarNuevaPartidaUseCase(_repo), hayPartida),
                hayPartida);
            _acciones = new List<ResultadoAccionMenu>();
            _vm.OnAccion += a => _acciones.Add(a);
            _vm.Inicializar();
        }

        [Test]
        public void Inicializar_NoHayNadaSeleccionado()
        {
            Assert.IsFalse(_vm.EstadoActual.HaySeleccion);
            Assert.IsFalse(_vm.EstadoActual.HayPartidaGuardada);
        }

        [Test]
        public void Mover_PrimeraPulsacionAbajo_SeleccionaLaPrimera()
        {
            _vm.Mover(+1);
            Assert.AreEqual(0, _vm.EstadoActual.IndiceSeleccionado);
        }

        [Test]
        public void Mover_PrimeraPulsacionArriba_SeleccionaLaUltima()
        {
            _vm.Mover(-1);
            Assert.AreEqual(3, _vm.EstadoActual.IndiceSeleccionado);
        }

        [Test]
        public void Mover_DaLaVueltaAlLlegarAlFinal()
        {
            for (int i = 0; i < 5; i++) _vm.Mover(+1);      // 0,1,2,3,0
            Assert.AreEqual(0, _vm.EstadoActual.IndiceSeleccionado);
        }

        [Test]
        public void Hover_Y_QuitarHover_ActualizanLaSeleccion()
        {
            _vm.Hover(2);
            Assert.AreEqual(2, _vm.EstadoActual.IndiceSeleccionado);

            _vm.QuitarHover(2);
            Assert.IsFalse(_vm.EstadoActual.HaySeleccion);
        }

        [Test]
        public void QuitarHover_DeOtroBoton_NoDeseleccionaElActual()
        {
            _vm.Hover(1);
            _vm.QuitarHover(3);
            Assert.AreEqual(1, _vm.EstadoActual.IndiceSeleccionado);
        }

        [Test]
        public void Hover_ConIndiceInvalido_SeIgnora()
        {
            _vm.Hover(9);
            Assert.IsFalse(_vm.EstadoActual.HaySeleccion);
        }

        [Test]
        public void ActivarSeleccion_SinSeleccion_NoHaceNada()
        {
            _vm.ActivarSeleccion();
            Assert.AreEqual(0, _acciones.Count);
        }

        [Test]
        public void ActivarSeleccion_ConSeleccion_EjecutaLaOpcion()
        {
            _vm.Mover(+1);                 // Nueva partida
            _vm.ActivarSeleccion();

            Assert.AreEqual(1, _acciones.Count);
            Assert.AreEqual(ResultadoAccionMenu.IrASeleccionCapitulo, _acciones[0]);
        }

        [Test]
        public void Activar_NuevaPartida_MarcaQueYaHayPartidaGuardada()
        {
            _vm.Activar(0);
            Assert.IsTrue(_vm.EstadoActual.HayPartidaGuardada);
        }

        [Test]
        public void Activar_CargarSinGuardado_EmitePartidaNoEncontrada()
        {
            _vm.Activar(1);
            Assert.AreEqual(ResultadoAccionMenu.PartidaNoEncontrada, _acciones[0]);
        }

        [Test]
        public void Activar_ConIndiceInvalido_SeIgnora()
        {
            _vm.Activar(-1);
            _vm.Activar(4);
            Assert.AreEqual(0, _acciones.Count);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.Presentation.MainMenu;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.Integration
{
    /// <summary>Arma el menú igual que MainMenuInstaller, pero sobre un directorio temporal real.</summary>
    public class MainMenuIntegrationWiringTests
    {
        private string _directorio;

        [SetUp]
        public void SetUp()
        {
            _directorio = Path.Combine(Path.GetTempPath(), "mainmenu_wiring_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
        }

        private MainMenuViewModel CrearViewModel(out List<ResultadoAccionMenu> acciones)
        {
            var repo = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            var hayPartida = new HayPartidaGuardadaUseCase(repo);
            var vm = new MainMenuViewModel(
                new EjecutarOpcionMenuUseCase(new IniciarNuevaPartidaUseCase(repo), hayPartida),
                hayPartida);
            var lista = new List<ResultadoAccionMenu>();
            vm.OnAccion += a => lista.Add(a);
            acciones = lista;
            vm.Inicializar();
            return vm;
        }

        [Test]
        public void PrimeraVez_CargarPartidaAvisaQueNoHayGuardado()
        {
            var vm = CrearViewModel(out var acciones);

            vm.Activar((int)OpcionMenuPrincipal.CargarPartida);

            Assert.AreEqual(ResultadoAccionMenu.PartidaNoEncontrada, acciones[0]);
        }

        [Test]
        public void NuevaPartida_LuegoCargarPartida_FuncionaEnUnaEscenaNueva()
        {
            var vm1 = CrearViewModel(out var acciones1);
            vm1.Activar((int)OpcionMenuPrincipal.NuevaPartida);
            Assert.AreEqual(ResultadoAccionMenu.IrASeleccionCapitulo, acciones1[0]);

            // Nueva instancia de todo (como al volver al menú desde otra escena)
            var vm2 = CrearViewModel(out var acciones2);
            Assert.IsTrue(vm2.EstadoActual.HayPartidaGuardada);

            vm2.Activar((int)OpcionMenuPrincipal.CargarPartida);
            Assert.AreEqual(ResultadoAccionMenu.IrASeleccionCapitulo, acciones2[0]);
        }
    }
}

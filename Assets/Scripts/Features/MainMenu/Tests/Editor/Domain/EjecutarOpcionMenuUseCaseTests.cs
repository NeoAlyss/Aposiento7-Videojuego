using System;
using NUnit.Framework;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Tests.Editor.Domain
{
    public class EjecutarOpcionMenuUseCaseTests
    {
        private class RepositorioFalso : ProgresoRepositoryPort
        {
            public bool PartidaGuardada;
            public int Reinicios;

            public ProgresoCapitulo ObtenerProgreso(string idCapitulo) => ProgresoCapitulo.Nuevo(idCapitulo);
            public void GuardarProgreso(ProgresoCapitulo progreso) { }
            public bool HayPartidaGuardada() => PartidaGuardada;
            public void ReiniciarPartida() { Reinicios++; PartidaGuardada = true; }
        }

        private RepositorioFalso _repo;
        private EjecutarOpcionMenuUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _repo = new RepositorioFalso();
            _useCase = new EjecutarOpcionMenuUseCase(
                new IniciarNuevaPartidaUseCase(_repo),
                new HayPartidaGuardadaUseCase(_repo));
        }

        [Test]
        public void Constructor_ConDependenciasNulas_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EjecutarOpcionMenuUseCase(null, new HayPartidaGuardadaUseCase(_repo)));
            Assert.Throws<ArgumentNullException>(() =>
                new EjecutarOpcionMenuUseCase(new IniciarNuevaPartidaUseCase(_repo), null));
        }

        [Test]
        public void NuevaPartida_ReiniciaProgresoYVaASeleccionDeCapitulo()
        {
            var resultado = _useCase.Ejecutar(OpcionMenuPrincipal.NuevaPartida);

            Assert.AreEqual(ResultadoAccionMenu.IrASeleccionCapitulo, resultado);
            Assert.AreEqual(1, _repo.Reinicios);
        }

        [Test]
        public void CargarPartida_ConGuardado_VaASeleccionDeCapitulo()
        {
            _repo.PartidaGuardada = true;

            Assert.AreEqual(ResultadoAccionMenu.IrASeleccionCapitulo, _useCase.Ejecutar(OpcionMenuPrincipal.CargarPartida));
            Assert.AreEqual(0, _repo.Reinicios);
        }

        [Test]
        public void CargarPartida_SinGuardado_AvisaQueNoHayPartida()
        {
            Assert.AreEqual(ResultadoAccionMenu.PartidaNoEncontrada, _useCase.Ejecutar(OpcionMenuPrincipal.CargarPartida));
        }

        [Test]
        public void Opciones_MuestraOpciones()
        {
            Assert.AreEqual(ResultadoAccionMenu.MostrarOpciones, _useCase.Ejecutar(OpcionMenuPrincipal.Opciones));
        }

        [Test]
        public void Salir_PideSalirDelJuego()
        {
            Assert.AreEqual(ResultadoAccionMenu.SalirDelJuego, _useCase.Ejecutar(OpcionMenuPrincipal.Salir));
        }
    }
}

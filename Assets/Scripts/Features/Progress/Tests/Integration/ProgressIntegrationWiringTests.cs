using System.IO;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Integration
{
    /// <summary>Verifica que el composition root deja repositorio + JSON funcionando de punta a punta.</summary>
    public class ProgressIntegrationWiringTests
    {
        private string _directorio;

        [SetUp]
        public void SetUp()
        {
            _directorio = Path.Combine(Path.GetTempPath(), "progreso_wiring_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
        }

        [Test]
        public void ProgresoSobrevive_AUnaNuevaInstanciaDelRepositorio()
        {
            var repo1 = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            new RegistrarLlaveUseCase(repo1).Ejecutar("chapter_01", 2);
            new RegistrarTiempoJugadoUseCase(repo1).Ejecutar("chapter_01", 30f);

            // Simula cambiar de escena: nuevo repositorio sobre el mismo archivo.
            var repo2 = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            var p = new ObtenerProgresoCapituloUseCase(repo2).Ejecutar("chapter_01");

            Assert.AreEqual(2, p.Llaves);
            Assert.AreEqual(30f, p.SegundosJugados, 0.001f);
        }

        [Test]
        public void NuevaPartida_BorraElProgresoAnterior()
        {
            var repo = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            new RegistrarLlaveUseCase(repo).Ejecutar("chapter_01", 3);

            new IniciarNuevaPartidaUseCase(repo).Ejecutar();

            var otro = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            Assert.IsTrue(new HayPartidaGuardadaUseCase(otro).Ejecutar());
            Assert.AreEqual(0, new ObtenerProgresoCapituloUseCase(otro).Ejecutar("chapter_01").Llaves);
        }

        [Test]
        public void SinGuardado_HayPartidaGuardadaEsFalso()
        {
            var repo = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            Assert.IsFalse(new HayPartidaGuardadaUseCase(repo).Ejecutar());
        }
    }
}

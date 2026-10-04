using System;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Tests.Editor.Fakes;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Domain
{
    public class RegistrarTiempoJugadoUseCaseTests
    {
        [Test]
        public void Constructor_ConRepositorioNulo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrarTiempoJugadoUseCase(null));
        }

        [Test]
        public void Ejecutar_PrimerIntento_ParteDesdeCero()
        {
            var repo = new FakeProgresoRepository();

            var resultado = new RegistrarTiempoJugadoUseCase(repo).Ejecutar("chapter_01", 5f);

            Assert.AreEqual(5f, resultado.SegundosJugados);
        }

        [Test]
        public void Ejecutar_AcumulaTiempo()
        {
            var repo = new FakeProgresoRepository();
            var useCase = new RegistrarTiempoJugadoUseCase(repo);

            useCase.Ejecutar("chapter_01", 5f);
            var resultado = useCase.Ejecutar("chapter_01", 7.5f);

            Assert.AreEqual(12.5f, resultado.SegundosJugados);
        }

        [Test]
        public void Ejecutar_ConDeltaNegativo_NoCambiaElTiempo()
        {
            var repo = new FakeProgresoRepository();
            var useCase = new RegistrarTiempoJugadoUseCase(repo);
            useCase.Ejecutar("chapter_01", 5f);

            var resultado = useCase.Ejecutar("chapter_01", -3f);

            Assert.AreEqual(5f, resultado.SegundosJugados);
        }
    }
}

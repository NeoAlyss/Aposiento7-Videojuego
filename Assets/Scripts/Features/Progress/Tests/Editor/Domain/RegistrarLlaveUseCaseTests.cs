using System;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Tests.Editor.Fakes;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Domain
{
    public class RegistrarLlaveUseCaseTests
    {
        [Test]
        public void Constructor_ConRepositorioNulo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrarLlaveUseCase(null));
        }

        [Test]
        public void Ejecutar_SumaUnaLlaveYGuarda()
        {
            var repo = new FakeProgresoRepository();
            var useCase = new RegistrarLlaveUseCase(repo);

            var resultado = useCase.Ejecutar("chapter_01");

            Assert.AreEqual(1, resultado.Llaves);
            Assert.AreEqual(1, repo.ObtenerProgreso("chapter_01").Llaves);
            Assert.AreEqual(1, repo.VecesGuardado);
        }

        [Test]
        public void Ejecutar_AcumulaLlavesEntreLlamadas()
        {
            var repo = new FakeProgresoRepository();
            var useCase = new RegistrarLlaveUseCase(repo);

            useCase.Ejecutar("chapter_01", 2);
            var resultado = useCase.Ejecutar("chapter_01", 1);

            Assert.AreEqual(3, resultado.Llaves);
        }

        [Test]
        public void Ejecutar_ConservaTiempoYCompletado()
        {
            var repo = new FakeProgresoRepository();
            repo.Almacen["chapter_01"] = new ProgresoCapitulo("chapter_01", 0, 42f, true);

            var resultado = new RegistrarLlaveUseCase(repo).Ejecutar("chapter_01");

            Assert.AreEqual(42f, resultado.SegundosJugados);
            Assert.IsTrue(resultado.Completado);
        }
    }
}

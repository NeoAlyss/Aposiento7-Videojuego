using System;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Tests.Editor.Fakes;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Domain
{
    public class IniciarNuevaPartidaUseCaseTests
    {
        [Test]
        public void Constructor_ConRepositorioNulo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new IniciarNuevaPartidaUseCase(null));
        }

        [Test]
        public void Ejecutar_BorraProgresoYDejaPartidaGuardada()
        {
            var repo = new FakeProgresoRepository();
            repo.Almacen["chapter_01"] = new ProgresoCapitulo("chapter_01", 3, 99f, true);

            new IniciarNuevaPartidaUseCase(repo).Ejecutar();

            Assert.AreEqual(1, repo.VecesReiniciado);
            Assert.AreEqual(0, repo.ObtenerProgreso("chapter_01").Llaves);
            Assert.AreEqual(0f, repo.ObtenerProgreso("chapter_01").SegundosJugados);
            Assert.IsTrue(new HayPartidaGuardadaUseCase(repo).Ejecutar());
        }
    }
}

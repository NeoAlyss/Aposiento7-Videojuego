using System.Linq;
using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Data;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Editor.Data
{
    public class CapitulosDataRepositoryTests
    {
        private CapitulosDataRepository _repo;

        [SetUp]
        public void SetUp() => _repo = new CapitulosDataRepository(new CapitulosDefaultDataSource());

        [Test]
        public void ObtenerCapitulos_DevuelveLosDoceDelReloj()
        {
            var capitulos = _repo.ObtenerCapitulos();

            Assert.AreEqual(12, capitulos.Count);
            CollectionAssert.AreEqual(Enumerable.Range(1, 12).ToArray(), capitulos.Select(c => c.NumeroReloj).ToArray());
        }

        [Test]
        public void MVP_SoloElCapituloUnoEstaDisponible()
        {
            var disponibles = _repo.ObtenerCapitulos().Where(c => c.Disponible).ToArray();

            Assert.AreEqual(1, disponibles.Length);
            Assert.AreEqual("chapter_01", disponibles[0].Id);
            Assert.AreEqual("Chapter1", disponibles[0].NombreEscena);
        }

        [Test]
        public void ObtenerPorNumero_DevuelveElCapituloOSiNoExisteNull()
        {
            Assert.AreEqual("chapter_07", _repo.ObtenerPorNumero(7).Id);
            Assert.IsNull(_repo.ObtenerPorNumero(13));
            Assert.IsNull(_repo.ObtenerPorNumero(0));
        }

        [Test]
        public void LosIdsSonUnicos()
        {
            var ids = _repo.ObtenerCapitulos().Select(c => c.Id).ToArray();
            Assert.AreEqual(ids.Length, ids.Distinct().Count());
        }
    }
}

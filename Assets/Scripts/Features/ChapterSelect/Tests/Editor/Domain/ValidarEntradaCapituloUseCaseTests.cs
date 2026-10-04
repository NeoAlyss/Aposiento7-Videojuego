using System;
using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Editor.Domain
{
    public class ValidarEntradaCapituloUseCaseTests
    {
        private class RepositorioFalso : CapitulosRepositoryPort
        {
            private readonly List<Capitulo> _lista = new List<Capitulo>
            {
                new Capitulo("chapter_01", 1, "Uno", DificultadCapitulo.Facil, 3, true, "Chapter1"),
                new Capitulo("chapter_02", 2, "Dos", DificultadCapitulo.Media, 3, false, "Chapter2")
            };

            public IReadOnlyList<Capitulo> ObtenerCapitulos() => _lista;
            public Capitulo ObtenerPorNumero(int n) => _lista.Find(c => c.NumeroReloj == n);
        }

        private ValidarEntradaCapituloUseCase _useCase;

        [SetUp]
        public void SetUp() => _useCase = new ValidarEntradaCapituloUseCase(new RepositorioFalso());

        [Test]
        public void Constructor_ConRepositorioNulo_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() => new ValidarEntradaCapituloUseCase(null));
        }

        [Test]
        public void CapituloDisponible_PermiteEntrarYDevuelveElCapitulo()
        {
            var r = _useCase.Ejecutar(1);

            Assert.AreEqual(EstadoEntradaCapitulo.Permitida, r.Estado);
            Assert.AreEqual("Chapter1", r.Capitulo.NombreEscena);
        }

        [Test]
        public void CapituloNoDisponible_EstaBloqueado()
        {
            var r = _useCase.Ejecutar(2);

            Assert.AreEqual(EstadoEntradaCapitulo.Bloqueada, r.Estado);
            Assert.IsNull(r.Capitulo);
        }

        [Test]
        public void NumeroSinCapitulo_DevuelveSinCapitulo()
        {
            Assert.AreEqual(EstadoEntradaCapitulo.SinCapitulo, _useCase.Ejecutar(9).Estado);
        }
    }
}

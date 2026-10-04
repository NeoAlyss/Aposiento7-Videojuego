using System;
using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Editor.Domain
{
    public class ObtenerDetalleCapituloUseCaseTests
    {
        private class CapitulosFalso : CapitulosRepositoryPort
        {
            private readonly List<Capitulo> _lista = new List<Capitulo>
            {
                new Capitulo("chapter_01", 1, "Capítulo 1", DificultadCapitulo.Facil, 3, true, "Chapter1"),
                new Capitulo("chapter_02", 2, "Capítulo 2", DificultadCapitulo.Dificil, 3, false, "Chapter2")
            };

            public IReadOnlyList<Capitulo> ObtenerCapitulos() => _lista;
            public Capitulo ObtenerPorNumero(int n) => _lista.Find(c => c.NumeroReloj == n);
        }

        private class ProgresoFalso : ProgresoRepositoryPort
        {
            public readonly Dictionary<string, ProgresoCapitulo> Datos = new Dictionary<string, ProgresoCapitulo>();

            public ProgresoCapitulo ObtenerProgreso(string id) =>
                Datos.TryGetValue(id, out var p) ? p : ProgresoCapitulo.Nuevo(id);
            public void GuardarProgreso(ProgresoCapitulo p) => Datos[p.IdCapitulo] = p;
            public bool HayPartidaGuardada() => Datos.Count > 0;
            public void ReiniciarPartida() => Datos.Clear();
        }

        private ProgresoFalso _progreso;
        private ObtenerDetalleCapituloUseCase _useCase;

        [SetUp]
        public void SetUp()
        {
            _progreso = new ProgresoFalso();
            _useCase = new ObtenerDetalleCapituloUseCase(new CapitulosFalso(), new ObtenerProgresoCapituloUseCase(_progreso));
        }

        [Test]
        public void Constructor_ConDependenciasNulas_Lanza()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObtenerDetalleCapituloUseCase(null, new ObtenerProgresoCapituloUseCase(_progreso)));
            Assert.Throws<ArgumentNullException>(() =>
                new ObtenerDetalleCapituloUseCase(new CapitulosFalso(), null));
        }

        [Test]
        public void CapituloNuncaJugado_MuestraLlavesEnCeroYTiempoEnCero()
        {
            var d = _useCase.Ejecutar(1);

            Assert.AreEqual("Capítulo 1", d.Titulo);
            Assert.AreEqual("Fácil", d.Dificultad);
            Assert.AreEqual("0 / 3", d.Llaves);
            Assert.AreEqual("00:00:00", d.Tiempo);
            Assert.IsFalse(d.Bloqueado);
        }

        [Test]
        public void CapituloConProgreso_MuestraLlavesYTiempoGuardados()
        {
            _progreso.Datos["chapter_01"] = new ProgresoCapitulo("chapter_01", 2, 125f, false);

            var d = _useCase.Ejecutar(1);

            Assert.AreEqual("2 / 3", d.Llaves);
            Assert.AreEqual("00:02:05", d.Tiempo);
        }

        [Test]
        public void CapituloNoDisponible_SeMuestraBloqueado()
        {
            var d = _useCase.Ejecutar(2);

            Assert.IsTrue(d.Bloqueado);
            Assert.AreEqual("Bloqueado", d.Dificultad);
            Assert.AreEqual("—", d.Llaves);
            Assert.AreEqual("—", d.Tiempo);
        }

        [Test]
        public void NumeroSinCapitulo_MuestraHoraYBloqueado()
        {
            var d = _useCase.Ejecutar(9);

            Assert.AreEqual("Hora 9", d.Titulo);
            Assert.IsTrue(d.Bloqueado);
        }
    }
}

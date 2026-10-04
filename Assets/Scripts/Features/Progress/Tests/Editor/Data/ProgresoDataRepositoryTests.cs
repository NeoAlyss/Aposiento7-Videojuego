using System.Collections.Generic;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Data;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Data
{
    public class ProgresoDataRepositoryTests
    {
        private class LocalFalso : ProgresoLocalDataSourcePort
        {
            public List<ProgresoCapitulo> Contenido;          // null = no hay archivo
            public int VecesGuardado;

            public IReadOnlyList<ProgresoCapitulo> LeerTodo() => Contenido;
            public bool Existe() => Contenido != null;

            public void GuardarTodo(IReadOnlyCollection<ProgresoCapitulo> progresos)
            {
                Contenido = new List<ProgresoCapitulo>(progresos);
                VecesGuardado++;
            }
        }

        [Test]
        public void ObtenerProgreso_SinArchivo_DevuelveProgresoNuevo()
        {
            var repo = new ProgresoDataRepository(new ProgresoDefaultDataSource(), new LocalFalso());

            var p = repo.ObtenerProgreso("chapter_01");

            Assert.AreEqual(0, p.Llaves);
            Assert.AreEqual(0f, p.SegundosJugados);
        }

        [Test]
        public void ObtenerProgreso_ConArchivo_CargaLoGuardado()
        {
            var local = new LocalFalso
            {
                Contenido = new List<ProgresoCapitulo> { new ProgresoCapitulo("chapter_01", 2, 120f, false) }
            };
            var repo = new ProgresoDataRepository(new ProgresoDefaultDataSource(), local);

            var p = repo.ObtenerProgreso("chapter_01");

            Assert.AreEqual(2, p.Llaves);
            Assert.AreEqual(120f, p.SegundosJugados);
        }

        [Test]
        public void GuardarProgreso_PersisteEnLocal()
        {
            var local = new LocalFalso();
            var repo = new ProgresoDataRepository(new ProgresoDefaultDataSource(), local);

            repo.GuardarProgreso(new ProgresoCapitulo("chapter_01", 1, 10f, false));

            Assert.AreEqual(1, local.VecesGuardado);
            Assert.AreEqual(1, local.Contenido.Count);
            Assert.IsTrue(repo.HayPartidaGuardada());
        }

        [Test]
        public void ReiniciarPartida_LimpiaMemoriaYCreaArchivoVacio()
        {
            var local = new LocalFalso
            {
                Contenido = new List<ProgresoCapitulo> { new ProgresoCapitulo("chapter_01", 2, 120f, true) }
            };
            var repo = new ProgresoDataRepository(new ProgresoDefaultDataSource(), local);
            repo.ObtenerProgreso("chapter_01");

            repo.ReiniciarPartida();

            Assert.AreEqual(0, repo.ObtenerProgreso("chapter_01").Llaves);
            Assert.AreEqual(0, local.Contenido.Count);
            Assert.IsTrue(repo.HayPartidaGuardada());
        }

        [Test]
        public void HayPartidaGuardada_SinLocal_EsFalso()
        {
            var repo = new ProgresoDataRepository(new ProgresoDefaultDataSource(), null);
            Assert.IsFalse(repo.HayPartidaGuardada());
        }
    }
}

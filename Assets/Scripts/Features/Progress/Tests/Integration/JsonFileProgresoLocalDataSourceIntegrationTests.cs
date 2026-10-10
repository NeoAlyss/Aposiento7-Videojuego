using System.IO;
using NUnit.Framework;
using UniversalPlatform.Features.Progress.Datasources;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.Progress.Tests.Integration
{
    public class JsonFileProgresoLocalDataSourceIntegrationTests
    {
        private string _directorio;

        [SetUp]
        public void SetUp()
        {
            _directorio = Path.Combine(Path.GetTempPath(), "progreso_tests_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
        }

        [Test]
        public void SinArchivo_NoExisteYLeerDevuelveNull()
        {
            var ds = new JsonFileProgresoLocalDataSource(_directorio);

            Assert.IsFalse(ds.Existe());
            Assert.IsNull(ds.LeerTodo());
        }

        [Test]
        public void GuardarYLeer_HaceRoundTrip()
        {
            var ds = new JsonFileProgresoLocalDataSource(_directorio);
            var datos = new[]
            {
                new ProgresoCapitulo("chapter_01", 2, 125.5f, true),
                new ProgresoCapitulo("chapter_02", 0, 0f, false)
            };

            ds.GuardarTodo(datos);
            var leidos = new JsonFileProgresoLocalDataSource(_directorio).LeerTodo();

            Assert.IsTrue(ds.Existe());
            Assert.AreEqual(2, leidos.Count);
            Assert.AreEqual("chapter_01", leidos[0].IdCapitulo);
            Assert.AreEqual(2, leidos[0].Llaves);
            Assert.AreEqual(125.5f, leidos[0].SegundosJugados, 0.001f);
            Assert.IsTrue(leidos[0].Completado);
        }

        [Test]
        public void ArchivoCorrupto_DevuelveNullSinLanzar()
        {
            Directory.CreateDirectory(_directorio);
            File.WriteAllText(Path.Combine(_directorio, "progreso_partida.json"), "esto no es json {{{");
            var ds = new JsonFileProgresoLocalDataSource(_directorio);

            Assert.DoesNotThrow(() => ds.LeerTodo());
            Assert.IsNull(ds.LeerTodo());
        }

        [Test]
        public void GuardarTodoVacio_CreaArchivoDePartidaNueva()
        {
            var ds = new JsonFileProgresoLocalDataSource(_directorio);

            ds.GuardarTodo(new ProgresoCapitulo[0]);

            Assert.IsTrue(ds.Existe());
            Assert.AreEqual(0, ds.LeerTodo().Count);
        }
    }
}

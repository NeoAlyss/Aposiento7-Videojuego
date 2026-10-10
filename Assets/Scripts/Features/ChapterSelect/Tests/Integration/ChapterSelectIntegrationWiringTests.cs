using System.IO;
using NUnit.Framework;
using UniversalPlatform.Features.ChapterSelect.Data;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.ChapterSelect.Presentation.Clock;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.ChapterSelect.Tests.Integration
{
    /// <summary>
    /// Arma el reloj igual que ChapterSelectInstaller, con el progreso real guardado en un JSON
    /// temporal: lo que se juega en un capítulo aparece luego en los boxes del reloj.
    /// </summary>
    public class ChapterSelectIntegrationWiringTests
    {
        private string _directorio;

        [SetUp]
        public void SetUp()
        {
            _directorio = Path.Combine(Path.GetTempPath(), "chapterselect_wiring_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
        }

        private ChapterSelectViewModel CrearViewModel()
        {
            var progreso = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            var capitulos = new CapitulosDataRepository(new CapitulosDefaultDataSource());
            return new ChapterSelectViewModel(
                new ObtenerCapitulosUseCase(capitulos),
                new ObtenerDetalleCapituloUseCase(capitulos, new ObtenerProgresoCapituloUseCase(progreso)),
                new ValidarEntradaCapituloUseCase(capitulos));
        }

        [Test]
        public void PartidaNueva_ElCapituloUnoMuestraCeroLlavesYTiempoCero()
        {
            var vm = CrearViewModel();
            vm.Inicializar();

            Assert.AreEqual("0 / 3", vm.EstadoActual.Detalle.Llaves);
            Assert.AreEqual("00:00:00", vm.EstadoActual.Detalle.Tiempo);
        }

        [Test]
        public void LoJugadoEnElCapitulo_SeVeEnElRelojAlVolver()
        {
            // "Jugar" el capítulo 1: 2 llaves y 65 segundos, con el repositorio de esa escena.
            var repoJuego = ProgresoCompositionRoot.CrearRepositorio(_directorio);
            new RegistrarLlaveUseCase(repoJuego).Ejecutar("chapter_01", 2);
            new RegistrarTiempoJugadoUseCase(repoJuego).Ejecutar("chapter_01", 65f);

            // Al volver, la escena del reloj crea todo de nuevo.
            var vm = CrearViewModel();
            vm.Inicializar();

            Assert.AreEqual("2 / 3", vm.EstadoActual.Detalle.Llaves);
            Assert.AreEqual("00:01:05", vm.EstadoActual.Detalle.Tiempo);
        }

        [Test]
        public void LosCapitulosBloqueadosNoSeMuestranConProgreso()
        {
            var vm = CrearViewModel();
            vm.Inicializar();
            vm.Seleccionar(3);

            Assert.IsTrue(vm.EstadoActual.Detalle.Bloqueado);
            Assert.AreEqual("—", vm.EstadoActual.Detalle.Tiempo);
        }
    }
}

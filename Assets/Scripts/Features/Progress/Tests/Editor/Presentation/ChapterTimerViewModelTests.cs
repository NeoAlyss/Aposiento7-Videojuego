using NUnit.Framework;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Presentation.ChapterTimer;
using UniversalPlatform.Features.Progress.Tests.Editor.Fakes;

namespace UniversalPlatform.Features.Progress.Tests.Editor.Presentation
{
    public class ChapterTimerViewModelTests
    {
        private FakeProgresoRepository _repo;
        private ChapterTimerViewModel _vm;

        [SetUp]
        public void SetUp()
        {
            _repo = new FakeProgresoRepository();
            _vm = new ChapterTimerViewModel(
                new ObtenerProgresoCapituloUseCase(_repo),
                new RegistrarLlaveUseCase(_repo),
                new RegistrarTiempoJugadoUseCase(_repo),
                new CompletarCapituloUseCase(_repo),
                intervaloGuardadoSegundos: 10f);
        }

        [Test]
        public void Inicializar_PrimerIntento_MuestraTiempoCero()
        {
            _vm.Inicializar("chapter_01");

            Assert.AreEqual("00:00:00", _vm.EstadoActual.TiempoFormateado);
            Assert.AreEqual(0, _vm.EstadoActual.Llaves);
        }

        [Test]
        public void AvanzarTiempo_NoGuardaAntesDelIntervalo()
        {
            _vm.Inicializar("chapter_01");

            _vm.AvanzarTiempo(3f);

            Assert.AreEqual(0, _repo.VecesGuardado);
            Assert.AreEqual("00:00:03", _vm.EstadoActual.TiempoFormateado);
        }

        [Test]
        public void AvanzarTiempo_GuardaAlCumplirElIntervalo()
        {
            _vm.Inicializar("chapter_01");

            _vm.AvanzarTiempo(6f);
            _vm.AvanzarTiempo(6f);

            Assert.AreEqual(1, _repo.VecesGuardado);
            Assert.AreEqual(12f, _repo.ObtenerProgreso("chapter_01").SegundosJugados, 0.001f);
        }

        [Test]
        public void Finalizar_PersisteLoPendiente()
        {
            _vm.Inicializar("chapter_01");
            _vm.AvanzarTiempo(4f);

            _vm.Finalizar();

            Assert.AreEqual(4f, _repo.ObtenerProgreso("chapter_01").SegundosJugados, 0.001f);
        }

        [Test]
        public void AgregarLlave_ActualizaEstadoYNoPierdeTiempo()
        {
            _vm.Inicializar("chapter_01");
            _vm.AvanzarTiempo(5f);

            _vm.AgregarLlave();

            Assert.AreEqual(1, _vm.EstadoActual.Llaves);
            var guardado = _repo.ObtenerProgreso("chapter_01");
            Assert.AreEqual(1, guardado.Llaves);
            Assert.AreEqual(5f, guardado.SegundosJugados, 0.001f);
        }

        [Test]
        public void Completar_MarcaCompletado()
        {
            _vm.Inicializar("chapter_01");

            _vm.Completar();

            Assert.IsTrue(_vm.EstadoActual.Completado);
            Assert.IsTrue(_repo.ObtenerProgreso("chapter_01").Completado);
        }

        [Test]
        public void SinInicializar_LasOperacionesNoHacenNada()
        {
            _vm.AvanzarTiempo(5f);
            _vm.AgregarLlave();
            _vm.Completar();
            _vm.Finalizar();

            Assert.AreEqual(0, _repo.VecesGuardado);
            Assert.IsNull(_vm.EstadoActual);
        }

        [Test]
        public void Inicializar_ConProgresoPrevio_ElCronometroPartEnCero_PeroLaPartidaSigueSumando()
        {
            _repo.Almacen["chapter_01"] = new ProgresoCapitulo("chapter_01", 2, 3600f, false);

            _vm.Inicializar("chapter_01");
            Assert.AreEqual("00:00:00", _vm.EstadoActual.TiempoFormateado, "Volver a jugar el capítulo parte de 0.");
            Assert.AreEqual(2, _vm.EstadoActual.Llaves);

            _vm.AvanzarTiempo(5f);
            _vm.Finalizar();
            Assert.AreEqual("00:00:05", _vm.EstadoActual.TiempoFormateado);
            Assert.AreEqual(3605f, _repo.ObtenerProgreso("chapter_01").SegundosJugados, 0.001f,
                "El tiempo de partida guardado sigue acumulando.");
        }
    }
}

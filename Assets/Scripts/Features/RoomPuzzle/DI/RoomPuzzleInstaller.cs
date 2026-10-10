using UnityEngine;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Presentation.ChapterTimer;
using UniversalPlatform.Features.Progress.UI.ChapterTimer;
using UniversalPlatform.Features.RoomPuzzle.Data;
using UniversalPlatform.Features.RoomPuzzle.Domain;
using UniversalPlatform.Features.RoomPuzzle.Presentation.Room;
using UniversalPlatform.Features.RoomPuzzle.UI.Room;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.RoomPuzzle.DI
{
    /// <summary>
    /// Composition root de la escena de un aposento. Arma el puzle de la sala y el cronómetro del
    /// capítulo sobre el MISMO repositorio de progreso (dos repositorios sobre el mismo archivo se
    /// pisarían al guardar), y guarda llaves y capítulo completado cuando la sala se resuelve.
    /// </summary>
    public class RoomPuzzleInstaller : MonoBehaviour
    {
        [SerializeField] private RoomView _view;
        [Tooltip("Cronómetro del capítulo. Su 'Id Capitulo' decide qué sala se carga (chapter_01 = aposento azul).")]
        [SerializeField] private ChapterTimerView _cronometro;

        private void Awake()
        {
            EventSystemSetup.Asegurar();

            if (_view == null || _cronometro == null)
            {
                Debug.LogError("[RoomPuzzleInstaller] Falta asignar RoomView o ChapterTimerView.");
                return;
            }

            string idCapitulo = _cronometro.IdCapitulo;
            var sala = new ObtenerSalaUseCase(new SalasDataRepository()).Ejecutar(idCapitulo);
            if (sala == null)
            {
                Debug.LogError($"[RoomPuzzleInstaller] No hay sala definida para '{idCapitulo}' en SalasDataRepository.");
                return;
            }

            var progreso = ProgresoCompositionRoot.CrearRepositorio();
            _cronometro.Construir(new ChapterTimerViewModel(
                new ObtenerProgresoCapituloUseCase(progreso),
                new RegistrarLlaveUseCase(progreso),
                new RegistrarTiempoJugadoUseCase(progreso),
                new CompletarCapituloUseCase(progreso)));

            var guardarLlaves = new GuardarLlavesDeCapituloUseCase(progreso);
            var viewModel = new RoomPuzzleViewModel(sala);
            viewModel.OnSalaCompletada += llaves =>
            {
                guardarLlaves.Ejecutar(idCapitulo, llaves);
                _cronometro.Completar();
            };

            // El cronómetro solo corre mientras se juega: no durante la cinemática, la entrada del
            // encapuchado ni la animación final.
            _cronometro.Pausado = true;
            _view.AlCambiarJuegoActivo += activo => _cronometro.Pausado = !activo;

            _view.Construir(viewModel);
        }
    }
}

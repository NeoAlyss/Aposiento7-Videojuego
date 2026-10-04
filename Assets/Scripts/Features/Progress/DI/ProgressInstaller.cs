using UnityEngine;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Features.Progress.Presentation.ChapterTimer;
using UniversalPlatform.Features.Progress.UI.ChapterTimer;

namespace UniversalPlatform.Features.Progress.DI
{
    /// <summary>Composition root de la escena de un capítulo (cronómetro + llaves).</summary>
    public class ProgressInstaller : MonoBehaviour
    {
        [SerializeField] private ChapterTimerView _view;

        private void Awake()
        {
            var repositorio = ProgresoCompositionRoot.CrearRepositorio();

            var viewModel = new ChapterTimerViewModel(
                new ObtenerProgresoCapituloUseCase(repositorio),
                new RegistrarLlaveUseCase(repositorio),
                new RegistrarTiempoJugadoUseCase(repositorio),
                new CompletarCapituloUseCase(repositorio));

            if (_view != null)
            {
                _view.Construir(viewModel);
            }
            else
            {
                Debug.LogError("[ProgressInstaller] Falta asignar ChapterTimerView.");
            }
        }
    }
}

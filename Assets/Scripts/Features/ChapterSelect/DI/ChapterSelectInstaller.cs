using UnityEngine;
using UniversalPlatform.Features.ChapterSelect.Data;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.ChapterSelect.Presentation.Clock;
using UniversalPlatform.Features.ChapterSelect.UI.Clock;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.ChapterSelect.DI
{
    /// <summary>Composition root de la escena de selección de capítulo (el reloj).</summary>
    public class ChapterSelectInstaller : MonoBehaviour
    {
        [SerializeField] private ChapterSelectView _view;

        private void Awake()
        {
            EventSystemSetup.Asegurar();

            // Progreso (compartido con el resto del juego, mismo archivo JSON)
            var progresoRepositorio = ProgresoCompositionRoot.CrearRepositorio();
            var obtenerProgreso = new ObtenerProgresoCapituloUseCase(progresoRepositorio);

            // Catálogo de capítulos
            var capitulosRepositorio = new CapitulosDataRepository(new CapitulosDefaultDataSource());

            var viewModel = new ChapterSelectViewModel(
                new ObtenerCapitulosUseCase(capitulosRepositorio),
                new ObtenerDetalleCapituloUseCase(capitulosRepositorio, obtenerProgreso),
                new ValidarEntradaCapituloUseCase(capitulosRepositorio));

            if (_view != null)
            {
                _view.Construir(viewModel);
            }
            else
            {
                Debug.LogError("[ChapterSelectInstaller] Falta asignar ChapterSelectView.");
            }
        }
    }
}

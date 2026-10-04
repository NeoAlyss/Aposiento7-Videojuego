using UnityEngine;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.Presentation.MainMenu;
using UniversalPlatform.Features.MainMenu.UI.MainMenu;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.MainMenu.DI
{
    /// <summary>Composition root de la escena del menú principal.</summary>
    public class MainMenuInstaller : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;

        private void Awake()
        {
            EventSystemSetup.Asegurar();

            var progresoRepositorio = ProgresoCompositionRoot.CrearRepositorio();

            var iniciarNuevaPartida = new IniciarNuevaPartidaUseCase(progresoRepositorio);
            var hayPartidaGuardada = new HayPartidaGuardadaUseCase(progresoRepositorio);
            var ejecutarOpcion = new EjecutarOpcionMenuUseCase(iniciarNuevaPartida, hayPartidaGuardada);

            var viewModel = new MainMenuViewModel(ejecutarOpcion, hayPartidaGuardada);

            if (_view != null)
            {
                _view.Construir(viewModel);
            }
            else
            {
                Debug.LogError("[MainMenuInstaller] Falta asignar MainMenuView.");
            }
        }
    }
}

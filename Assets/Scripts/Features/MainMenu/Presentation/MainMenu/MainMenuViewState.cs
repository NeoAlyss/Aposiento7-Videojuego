using UniversalPlatform.Features.MainMenu.Domain;

namespace UniversalPlatform.Features.MainMenu.Presentation.MainMenu
{
    public class MainMenuViewState
    {
        public int IndiceSeleccionado { get; }
        public bool HaySeleccion { get; }
        public bool HayPartidaGuardada { get; }

        public MainMenuViewState(int indiceSeleccionado, bool hayPartidaGuardada)
        {
            IndiceSeleccionado = indiceSeleccionado;
            HaySeleccion = ReglasMenuPrincipal.EsIndiceValido(indiceSeleccionado);
            HayPartidaGuardada = hayPartidaGuardada;
        }
    }
}

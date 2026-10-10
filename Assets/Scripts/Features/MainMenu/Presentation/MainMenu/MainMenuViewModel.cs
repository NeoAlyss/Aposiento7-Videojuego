using System;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.Progress.Domain;

namespace UniversalPlatform.Features.MainMenu.Presentation.MainMenu
{
    public class MainMenuViewModel
    {
        private readonly EjecutarOpcionMenuUseCase _ejecutarOpcionUseCase;
        private readonly HayPartidaGuardadaUseCase _hayPartidaGuardadaUseCase;

        private int _indice = ReglasMenuPrincipal.SIN_SELECCION;
        private bool _hayPartida;

        public MainMenuViewState EstadoActual { get; private set; }
        public event Action<MainMenuViewState> OnStateChanged;
        public event Action<ResultadoAccionMenu> OnAccion;

        public MainMenuViewModel(
            EjecutarOpcionMenuUseCase ejecutarOpcionUseCase,
            HayPartidaGuardadaUseCase hayPartidaGuardadaUseCase)
        {
            _ejecutarOpcionUseCase = ejecutarOpcionUseCase;
            _hayPartidaGuardadaUseCase = hayPartidaGuardadaUseCase;
        }

        public void Inicializar()
        {
            _hayPartida = _hayPartidaGuardadaUseCase.Ejecutar();
            _indice = ReglasMenuPrincipal.SIN_SELECCION;
            Publicar();
        }

        /// <summary>Flechas / WASD: +1 baja, -1 sube.</summary>
        public void Mover(int delta)
        {
            _indice = ReglasMenuPrincipal.MoverSeleccion(_indice, delta);
            Publicar();
        }

        /// <summary>Mouse encima de un botón.</summary>
        public void Hover(int indice)
        {
            if (!ReglasMenuPrincipal.EsIndiceValido(indice) || indice == _indice) return;
            _indice = indice;
            Publicar();
        }

        /// <summary>El mouse sale del botón: solo deselecciona si era el seleccionado.</summary>
        public void QuitarHover(int indice)
        {
            if (indice != _indice) return;
            _indice = ReglasMenuPrincipal.SIN_SELECCION;
            Publicar();
        }

        /// <summary>Espacio / Enter sobre la opción seleccionada.</summary>
        public void ActivarSeleccion()
        {
            if (ReglasMenuPrincipal.EsIndiceValido(_indice)) Activar(_indice);
        }

        public void Activar(int indice)
        {
            if (!ReglasMenuPrincipal.EsIndiceValido(indice)) return;
            var resultado = _ejecutarOpcionUseCase.Ejecutar(ReglasMenuPrincipal.ObtenerOpcion(indice));
            _hayPartida = _hayPartidaGuardadaUseCase.Ejecutar();   // "Nueva partida" crea el guardado
            Publicar();
            OnAccion?.Invoke(resultado);
        }

        private void Publicar()
        {
            EstadoActual = new MainMenuViewState(_indice, _hayPartida);
            OnStateChanged?.Invoke(EstadoActual);
        }
    }
}

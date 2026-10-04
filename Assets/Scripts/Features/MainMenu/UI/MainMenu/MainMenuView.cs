using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.Presentation.MainMenu;
using UniversalPlatform.Features.MainMenu.UI.Placeholders;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.MainMenu.UI.MainMenu
{
    /// <summary>
    /// Vista del menú de inicio. Traduce input (mouse, flechas, WASD, Espacio/Enter) a llamadas al
    /// ViewModel y reacciona a su estado y a sus acciones. No contiene reglas de negocio.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("Botones (de arriba a abajo: Nueva, Cargar, Opciones, Salir)")]
        [SerializeField] private MenuButtonView[] _botones;

        [Header("Transición")]
        [SerializeField] private CanvasGroup _fadeOverlay;
        [SerializeField] private float _tiempoFade = 0.5f;
        [SerializeField] private string _escenaSeleccionCapitulo = "ChapterSelect";

        [Header("Paneles temporales")]
        [SerializeField] private PlaceholderEnConstruccionView _panelOpciones;
        [SerializeField] private PlaceholderEnConstruccionView _avisoSinPartida;

        private MainMenuViewModel _viewModel;
        private Action<string> _cargarEscena;
        private Action _salir;
        private bool _navegando;

        /// <param name="cargarEscena">Inyectable para tests; por defecto SceneManager.LoadScene.</param>
        /// <param name="salir">Inyectable para tests; por defecto cierra el juego.</param>
        public void Construir(MainMenuViewModel viewModel, Action<string> cargarEscena = null, Action salir = null)
        {
            _viewModel = viewModel;
            _cargarEscena = cargarEscena ?? (escena => SceneManager.LoadScene(escena));
            _salir = salir ?? SalirPorDefecto;

            for (int i = 0; i < _botones.Length; i++)
            {
                if (_botones[i] == null) continue;
                _botones[i].Construir(i, AlEntrarBoton, AlSalirBoton, AlClicBoton);
            }

            _viewModel.OnStateChanged += AplicarEstado;
            _viewModel.OnAccion += ManejarAccion;
            _viewModel.Inicializar();
        }

        private void OnDestroy()
        {
            if (_viewModel == null) return;
            _viewModel.OnStateChanged -= AplicarEstado;
            _viewModel.OnAccion -= ManejarAccion;
        }

        private bool ModalAbierto =>
            (_panelOpciones != null && _panelOpciones.EstaVisible) ||
            (_avisoSinPartida != null && _avisoSinPartida.EstaVisible);

        private bool Interactuable => _viewModel != null && !_navegando && !ModalAbierto;

        private void Update()
        {
            if (_viewModel == null || _navegando) return;

            if (ModalAbierto)
            {
                if (MenuInput.Cancelar || MenuInput.Confirmar)
                {
                    if (_panelOpciones != null) _panelOpciones.Ocultar();
                    if (_avisoSinPartida != null) _avisoSinPartida.Ocultar();
                }
                return;
            }

            if (MenuInput.Arriba || MenuInput.Izquierda) _viewModel.Mover(-1);
            else if (MenuInput.Abajo || MenuInput.Derecha) _viewModel.Mover(+1);
            else if (MenuInput.Confirmar) _viewModel.ActivarSeleccion();
        }

        // ---------- mouse (los botones llaman aquí) ----------

        private void AlEntrarBoton(int i) { if (Interactuable) _viewModel.Hover(i); }
        private void AlSalirBoton(int i) { if (_viewModel != null) _viewModel.QuitarHover(i); }
        private void AlClicBoton(int i) { if (Interactuable) _viewModel.Activar(i); }

        // ---------- reacción al ViewModel ----------

        private void AplicarEstado(MainMenuViewState estado)
        {
            for (int i = 0; i < _botones.Length; i++)
                if (_botones[i] != null) _botones[i].SetResaltado(estado.HaySeleccion && i == estado.IndiceSeleccionado);
        }

        private void ManejarAccion(ResultadoAccionMenu accion)
        {
            switch (accion)
            {
                case ResultadoAccionMenu.IrASeleccionCapitulo:
                    StartCoroutine(FadeYCargar(_escenaSeleccionCapitulo));
                    break;
                case ResultadoAccionMenu.MostrarOpciones:
                    if (_panelOpciones != null) _panelOpciones.Mostrar("Opciones\n\nEn construcción");
                    break;
                case ResultadoAccionMenu.PartidaNoEncontrada:
                    if (_avisoSinPartida != null) _avisoSinPartida.Mostrar("No hay ninguna partida guardada");
                    break;
                case ResultadoAccionMenu.SalirDelJuego:
                    _salir();
                    break;
            }
        }

        private IEnumerator FadeYCargar(string escena)
        {
            _navegando = true;
            if (_fadeOverlay != null)
            {
                _fadeOverlay.blocksRaycasts = true;
                for (float t = 0f; t < _tiempoFade; t += Time.unscaledDeltaTime)
                {
                    _fadeOverlay.alpha = t / _tiempoFade;
                    yield return null;
                }
                _fadeOverlay.alpha = 1f;
            }
            _cargarEscena(escena);
        }

        private static void SalirPorDefecto()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

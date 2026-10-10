using System;
using TMPro;
using UnityEngine;
using UniversalPlatform.Features.MainMenu.Presentation.LoadGame;
using UniversalPlatform.Features.MainMenu.UI.MainMenu;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.MainMenu.UI.LoadGame
{
    /// <summary>
    /// Submenú de partidas guardadas, encima del menú principal: una vela por ranura, un recuadro
    /// con la información de la vela seleccionada y el botón para volver. Traduce el input a
    /// llamadas al ViewModel; no contiene reglas. El objeto queda siempre activo y se muestra u
    /// oculta con su CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadGameView : MonoBehaviour
    {
        [Header("Velas (de izquierda a derecha: ranura 1, 2, 3)")]
        [SerializeField] private CandleView[] _velas;

        [Header("Textos")]
        [SerializeField] private TMP_Text _titulo;
        [SerializeField] private TMP_Text _subtitulo;

        [Header("Recuadro de información")]
        [SerializeField] private CanvasGroup _cajaInfo;
        [SerializeField] private TMP_Text _textoInfo;

        [Header("Volver")]
        [SerializeField] private MenuButtonView _botonVolver;

        [Header("Borrar partida (solo aparece con una vela encendida seleccionada)")]
        [SerializeField] private MenuButtonView _botonBorrar;
        [SerializeField] private CanvasGroup _grupoBorrar;
        [SerializeField] private TMP_Text _textoBorrar;

        [SerializeField] private float _duracionFade = 0.25f;

        private CanvasGroup _grupo;
        private LoadGameViewModel _viewModel;
        private float _alphaInfoObjetivo;
        private float _alphaBorrarObjetivo;
        private int _frameUltimoCambio = -1;

        /// <summary>El submenú está abierto (aunque todavía esté apareciendo).</summary>
        public bool EstaVisible { get; private set; }
        /// <summary>Ya tiene ViewModel: se le pueden pedir "Nueva partida" y "Cargar partida".</summary>
        public bool Lista => _viewModel != null;
        /// <summary>
        /// Frame en que se abrió o cerró por última vez. El menú principal lo usa para no procesar la
        /// misma pulsación de tecla dos veces (la que cierra el submenú no debe reabrirlo).
        /// </summary>
        public int FrameUltimoCambio => _frameUltimoCambio;

        /// <summary>Hay una partida lista para jugar: el menú principal hace el fundido y cambia de escena.</summary>
        public event Action AlEntrarAPartida;

        private CanvasGroup Grupo
        {
            get
            {
                if (_grupo == null) _grupo = GetComponent<CanvasGroup>();
                return _grupo;
            }
        }

        private void Awake()
        {
            if (_viewModel == null) AplicarAlpha(0f);
        }

        public void Construir(LoadGameViewModel viewModel)
        {
            _viewModel = viewModel;
            AplicarAlpha(0f);
            if (_cajaInfo != null) _cajaInfo.alpha = 0f;

            if (_velas != null)
                for (int i = 0; i < _velas.Length; i++)
                    if (_velas[i] != null) _velas[i].Construir(i, AlEntrarElemento, AlClicElemento);

            if (_botonVolver != null)
                _botonVolver.Construir(_velas != null ? _velas.Length : 0, AlEntrarElemento, _ => { }, AlClicElemento);
            if (_botonBorrar != null)
                _botonBorrar.Construir(_velas != null ? _velas.Length + 1 : 1, AlEntrarElemento, _ => { }, AlClicElemento);
            if (_grupoBorrar != null) { _grupoBorrar.alpha = 0f; _grupoBorrar.blocksRaycasts = false; }

            _viewModel.OnStateChanged += AplicarEstado;
            _viewModel.OnEntrarAPartida += EntrarAPartida;
            _viewModel.OnRanuraVacia += RanuraVacia;
            _viewModel.Inicializar();
        }

        private void OnDestroy()
        {
            if (_viewModel == null) return;
            _viewModel.OnStateChanged -= AplicarEstado;
            _viewModel.OnEntrarAPartida -= EntrarAPartida;
            _viewModel.OnRanuraVacia -= RanuraVacia;
        }

        // ---------- lo que pide el menú principal ----------

        public void SolicitarNuevaPartida() { if (_viewModel != null) _viewModel.SolicitarNuevaPartida(); }

        public void AbrirCarga() { if (_viewModel != null) _viewModel.AbrirCarga(); }

        // ---------- input ----------

        private void Update()
        {
            float paso = _duracionFade > 0f ? Time.unscaledDeltaTime / _duracionFade : 1f;
            AplicarAlpha(Mathf.MoveTowards(Grupo.alpha, EstaVisible ? 1f : 0f, paso));
            if (_cajaInfo != null) _cajaInfo.alpha = Mathf.MoveTowards(_cajaInfo.alpha, _alphaInfoObjetivo, paso * 2f);
            if (_grupoBorrar != null) _grupoBorrar.alpha = Mathf.MoveTowards(_grupoBorrar.alpha, _alphaBorrarObjetivo, paso * 2f);

            // La tecla que abrió el submenú no cuenta también como input dentro de él.
            if (_viewModel == null || !EstaVisible || Time.frameCount == _frameUltimoCambio) return;

            if (MenuInput.Izquierda) _viewModel.Mover(-1);
            else if (MenuInput.Derecha) _viewModel.Mover(+1);
            else if (MenuInput.Arriba) _viewModel.MoverVertical(-1);
            else if (MenuInput.Abajo) _viewModel.MoverVertical(+1);
            else if (MenuInput.Confirmar) _viewModel.Confirmar();
            else if (MenuInput.Cancelar) _viewModel.Volver();
            else if (MenuInput.Borrar) _viewModel.SolicitarBorrado();
        }

        private void AlEntrarElemento(int i) { if (_viewModel != null) _viewModel.Hover(i); }
        private void AlClicElemento(int i) { if (_viewModel != null) _viewModel.Activar(i); }

        // ---------- reacción al ViewModel ----------

        private void AplicarEstado(LoadGameViewState estado)
        {
            if (estado.Visible != EstaVisible)
            {
                EstaVisible = estado.Visible;
                _frameUltimoCambio = Time.frameCount;
                Grupo.blocksRaycasts = EstaVisible;
                Grupo.interactable = EstaVisible;
            }

            if (_titulo != null) _titulo.text = estado.Titulo;
            if (_subtitulo != null) _subtitulo.text = estado.Subtitulo;

            if (_velas != null)
            {
                for (int i = 0; i < _velas.Length; i++)
                {
                    if (_velas[i] == null) continue;
                    _velas[i].SetEncendida(i < estado.Ranuras.Count && estado.Ranuras[i] != null && estado.Ranuras[i].TienePartida);
                    // Con el foco en "Borrar partida" la vela que se borraría sigue marcada.
                    _velas[i].SetResaltada(i == estado.IndiceSeleccionado || (estado.BorrarSeleccionado && i == estado.IndiceVela));
                }
            }

            if (_botonVolver != null) _botonVolver.SetResaltado(estado.VolverSeleccionado);

            _alphaBorrarObjetivo = estado.PuedeBorrar ? 1f : 0f;
            if (_grupoBorrar != null) _grupoBorrar.blocksRaycasts = estado.PuedeBorrar;
            if (_botonBorrar != null) _botonBorrar.SetResaltado(estado.BorrarSeleccionado || estado.ConfirmandoBorrado);
            if (_textoBorrar != null) _textoBorrar.text = estado.ConfirmandoBorrado ? "Confirmar borrado" : "Borrar partida";

            // El recuadro conserva el último texto mientras se desvanece.
            _alphaInfoObjetivo = estado.HayVelaSeleccionada ? 1f : 0f;
            if (estado.HayVelaSeleccionada && _textoInfo != null) _textoInfo.text = estado.TextoInfo;
        }

        private void EntrarAPartida() => AlEntrarAPartida?.Invoke();

        private void RanuraVacia(int indice)
        {
            if (_velas != null && indice >= 0 && indice < _velas.Length && _velas[indice] != null)
                _velas[indice].Temblar();
        }

        private void AplicarAlpha(float alpha)
        {
            Grupo.alpha = alpha;
            if (!EstaVisible)
            {
                Grupo.blocksRaycasts = false;
                Grupo.interactable = false;
            }
        }
    }
}

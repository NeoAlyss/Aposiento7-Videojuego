using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UniversalPlatform.Features.ChapterSelect.Domain;
using UniversalPlatform.Features.ChapterSelect.Presentation.Clock;
using UniversalPlatform.Features.ChapterSelect.UI.Info;
using UniversalPlatform.Features.ChapterSelect.UI.Transition;
using UniversalPlatform.Shared;

namespace UniversalPlatform.Features.ChapterSelect.UI.Clock
{
    /// <summary>
    /// Vista del reloj de capítulos. Traduce input (mouse, flechas, WASD, Espacio/Enter, Escape) a
    /// llamadas al ViewModel y anima las manillas hacia los ángulos que indica el estado: la horaria
    /// apunta al capítulo y el minutero da una vuelta completa por hora recorrida (como un reloj real).
    ///
    /// Controles:  Derecha/D/Abajo/S = siguiente (sentido del reloj) · Izquierda/A/Arriba/W = anterior ·
    ///             clic izquierdo, Espacio o Enter = entrar · Escape = volver al menú.
    /// </summary>
    public class ChapterSelectView : MonoBehaviour
    {
        [Serializable]
        public class CapituloVisual
        {
            [Range(1, 12)] public int numeroReloj = 1;
            [Tooltip("Arte que se ve dentro del portal circular al entrar al capítulo.")]
            public Sprite arteDelPortal;
        }

        [Header("Números del reloj (exactamente 12, del 1 al 12)")]
        [SerializeField] private ClockNumberView[] _numeros = new ClockNumberView[12];
        [SerializeField] private bool _colocarNumerosAutomaticamente = true;
        [SerializeField] private float _radioNumeros = 380f;

        [Header("Manillas (Pivot = 0.5, 0; el dibujo apunta hacia arriba)")]
        [SerializeField] private RectTransform _manillaHoraria;
        [SerializeField] private RectTransform _manillaMinutero;
        [SerializeField] private float _tiempoSuavizado = 0.35f;

        [Header("Otros componentes")]
        [SerializeField] private ChapterInfoPanelView _panelInfo;
        [SerializeField] private ClockDoorTransitionView _transicionPuerta;
        [SerializeField] private CapituloVisual[] _visuales;

        [Header("Navegación")]
        [SerializeField] private int _numeroInicial = 1;
        [SerializeField] private string _escenaMenuPrincipal = "MainMenu";

        [Header("Eventos (sonido)")]
        [Tooltip("Cada vez que las manillas se mueven a otro número: tic-tac.")]
        [SerializeField] private UnityEvent _alMoverse;
        [SerializeField] private UnityEvent _alIntentarEntrarBloqueado;

        private ChapterSelectViewModel _viewModel;
        private Action<string> _cargarEscena;
        private int _numeroPrevio;
        private float _objetivoHoraria, _objetivoMinutero;
        private float _valorHoraria, _valorMinutero, _velHoraria, _velMinutero;
        private float _suavizado;
        private bool _ocupado;

        /// <param name="cargarEscena">Inyectable para tests; por defecto SceneManager.LoadScene.</param>
        public void Construir(ChapterSelectViewModel viewModel, Action<string> cargarEscena = null)
        {
            _viewModel = viewModel;
            _cargarEscena = cargarEscena ?? (escena => SceneManager.LoadScene(escena));
            _suavizado = _tiempoSuavizado;

            for (int i = 0; i < _numeros.Length && i < ReglasReloj.CANTIDAD_NUMEROS; i++)
            {
                var numero = _numeros[i];
                if (numero == null) continue;
                numero.Construir(i + 1, AlEntrarNumero, AlClicNumero);

                if (_colocarNumerosAutomaticamente)
                {
                    var pos = ReglasReloj.PosicionNumero(i + 1, _radioNumeros);
                    ((RectTransform)numero.transform).anchoredPosition = new Vector2(pos.X, pos.Y);
                }
            }

            _viewModel.OnStateChanged += AplicarEstado;
            _viewModel.OnEntrarCapitulo += EntrarAlCapitulo;
            _viewModel.OnCapituloBloqueado += CapituloBloqueado;
            _viewModel.OnVolverAlMenu += VolverAlMenu;
            _viewModel.Inicializar(_numeroInicial);
        }

        private void OnDestroy()
        {
            if (_viewModel == null) return;
            _viewModel.OnStateChanged -= AplicarEstado;
            _viewModel.OnEntrarCapitulo -= EntrarAlCapitulo;
            _viewModel.OnCapituloBloqueado -= CapituloBloqueado;
            _viewModel.OnVolverAlMenu -= VolverAlMenu;
        }

        private void Update()
        {
            AnimarManillas();
            if (_viewModel == null || _ocupado) return;

            if (MenuInput.Derecha || MenuInput.Abajo) _viewModel.Mover(+1);
            else if (MenuInput.Izquierda || MenuInput.Arriba) _viewModel.Mover(-1);
            else if (MenuInput.Confirmar) _viewModel.Confirmar();
            else if (MenuInput.Cancelar) _viewModel.Cancelar();
        }

        // ---------- mouse (los números llaman aquí) ----------

        private void AlEntrarNumero(int numero) { if (_viewModel != null && !_ocupado) _viewModel.Seleccionar(numero); }
        private void AlClicNumero(int numero) { if (_viewModel != null && !_ocupado) _viewModel.ConfirmarNumero(numero); }

        // ---------- reacción al ViewModel ----------

        private void AplicarEstado(ChapterSelectViewState estado)
        {
            for (int i = 0; i < _numeros.Length && i < ReglasReloj.CANTIDAD_NUMEROS; i++)
            {
                if (_numeros[i] == null) continue;
                _numeros[i].SetBloqueado(estado.BloqueadosPorNumero[i]);
                _numeros[i].SetResaltado(i + 1 == estado.NumeroSeleccionado);
            }

            _objetivoHoraria = estado.AnguloHorariaObjetivo;
            _objetivoMinutero = estado.AnguloMinuteroObjetivo;
            _suavizado = _tiempoSuavizado + 0.04f * estado.PasosRecorridos;

            if (_panelInfo != null) _panelInfo.Mostrar(estado.Detalle, estado.NumeroEnLadoDerecho);
            if (estado.NumeroSeleccionado != _numeroPrevio) _alMoverse?.Invoke();
            _numeroPrevio = estado.NumeroSeleccionado;
        }

        private void AnimarManillas()
        {
            float dt = Time.unscaledDeltaTime;
            _valorHoraria = Mathf.SmoothDamp(_valorHoraria, _objetivoHoraria, ref _velHoraria, _suavizado, Mathf.Infinity, dt);
            _valorMinutero = Mathf.SmoothDamp(_valorMinutero, _objetivoMinutero, ref _velMinutero, _suavizado, Mathf.Infinity, dt);
            if (_manillaHoraria != null) _manillaHoraria.localRotation = Quaternion.Euler(0, 0, -_valorHoraria);
            if (_manillaMinutero != null) _manillaMinutero.localRotation = Quaternion.Euler(0, 0, -_valorMinutero);
        }

        private void EntrarAlCapitulo(Capitulo capitulo)
        {
            if (_transicionPuerta == null)
            {
                _cargarEscena(capitulo.NombreEscena);
                return;
            }
            _ocupado = _transicionPuerta.Reproducir(capitulo.NombreEscena, BuscarArte(capitulo.NumeroReloj));
        }

        private void CapituloBloqueado()
        {
            _alIntentarEntrarBloqueado?.Invoke();
            if (_panelInfo != null) _panelInfo.Temblar();
        }

        private void VolverAlMenu()
        {
            if (!string.IsNullOrEmpty(_escenaMenuPrincipal)) _cargarEscena(_escenaMenuPrincipal);
        }

        private Sprite BuscarArte(int numeroReloj)
        {
            if (_visuales == null) return null;
            foreach (var v in _visuales)
                if (v != null && v.numeroReloj == numeroReloj) return v.arteDelPortal;
            return null;
        }
    }
}

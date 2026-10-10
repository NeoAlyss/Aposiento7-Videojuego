using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UniversalPlatform.Features.ChapterSelect.UI.Transition
{
    /// <summary>
    /// Transición al entrar a un capítulo: el reloj entero es una puerta redonda que se abre hacia
    /// atrás, girando en 3D sobre una bisagra en su borde izquierdo. Por el hueco aparece el interior
    /// (un portal circular con el arte del capítulo) y la vista avanza hacia él hasta que llena la
    /// pantalla, mientras la escena carga en segundo plano.
    ///
    /// Para que el giro se vea con perspectiva real, el Canvas debe estar en "Screen Space - Camera"
    /// con una cámara en perspectiva (así lo arma ChapterSelectSceneBuilder). En un Canvas Overlay
    /// funciona igual, pero la puerta solo se ve estrecharse.
    /// </summary>
    public class ClockDoorTransitionView : MonoBehaviour
    {
        [Header("Puerta (el reloj)")]
        [Tooltip("Contenedor de esfera, números y manillas. Pivot (0, 0.5): la bisagra es su borde izquierdo.")]
        [SerializeField] private RectTransform _puerta;
        [SerializeField] private CanvasGroup _grupoPuerta;
        [Tooltip("Hacia dónde y cuánto se abre. 90 = de canto; menos de 90 deja ver un filo de la puerta.")]
        [SerializeField, Range(30f, 120f)] private float _anguloApertura = 84f;

        [Header("Interior (detrás de la puerta)")]
        [Tooltip("RectTransform con Mask + Image circular, del tamaño de la esfera. Hijo: el arte del capítulo.")]
        [SerializeField] private RectTransform _portal;
        [SerializeField] private CanvasGroup _grupoPortal;
        [SerializeField] private Image _imagenPortal;
        [SerializeField] private Image _resplandorPortal;

        [Header("Zoom")]
        [Tooltip("Lo que se agranda al avanzar hacia el interior: contiene la puerta y el portal.")]
        [SerializeField] private RectTransform _escenario;

        [Header("Lo que se desvanece al empezar (boxes de información)")]
        [SerializeField] private CanvasGroup _grupoInfo;

        [Tooltip("Botón Volver: se desvanece junto con los boxes (opcional).")]
        [SerializeField] private CanvasGroup _grupoVolver;

        [Header("Fundido final")]
        [SerializeField] private CanvasGroup _fundidoFinal;

        [Header("Entrada a la escena (al llegar desde el menú o desde un capítulo)")]
        [Tooltip("El reloj aparece desde negro, acercándose hasta su tamaño normal.")]
        [SerializeField] private bool _animarEntrada = true;
        [SerializeField] private float _tiempoEntrada = 2.4f;
        [Tooltip("Tamaño del reloj al empezar a aparecer (1 = tamaño normal).")]
        [SerializeField, Range(0.2f, 1f)] private float _escalaInicialEntrada = 0.4f;

        [Header("Tiempos")]
        [SerializeField] private float _tiempoApertura = 1.5f;
        [SerializeField] private float _tiempoZoom = 1.6f;
        [Tooltip("Cuánto espera el zoom desde que la puerta empieza a abrirse. Menor = más simultáneos.")]
        [SerializeField] private float _retrasoZoom = 0.45f;

        [Header("Eventos (sonido)")]
        [SerializeField] private UnityEvent _alAbrir;
        [SerializeField] private UnityEvent _alHacerZoom;

        private bool _reproduciendo;

        /// <summary>El reloj todavía está apareciendo: la selección no acepta input.</summary>
        public bool EnEntrada { get; private set; }

        private void Start()
        {
            if (_animarEntrada && !_reproduciendo) StartCoroutine(Entrada());
        }

        /// <summary>
        /// Desde negro: el fondo aparece primero y el reloj se acerca desvaneciéndose hasta su lugar;
        /// los boxes y el botón Volver llegan al final.
        /// </summary>
        private IEnumerator Entrada()
        {
            EnEntrada = true;
            float escalaInicial = _escalaInicialEntrada;
            AplicarEntrada(0f, escalaInicial);
            if (_fundidoFinal != null) _fundidoFinal.blocksRaycasts = true;

            for (float t = 0f; t < _tiempoEntrada; t += Time.unscaledDeltaTime)
            {
                AplicarEntrada(t / Mathf.Max(0.01f, _tiempoEntrada), escalaInicial);
                yield return null;
            }
            AplicarEntrada(1f, escalaInicial);
            if (_fundidoFinal != null) _fundidoFinal.blocksRaycasts = false;
            EnEntrada = false;
        }

        private void AplicarEntrada(float x, float escalaInicial)
        {
            x = Mathf.Clamp01(x);
            // El negro se va primero (deja ver el fondo). El reloj aparece en la primera mitad y crece
            // durante TODA la entrada, frenando recién al final, para que se note que se agranda.
            if (_fundidoFinal != null) _fundidoFinal.alpha = 1f - Mathf.SmoothStep(0f, 1f, x / 0.3f);
            float acercamiento = EaseOutQuad(x);
            float escala = Mathf.Lerp(escalaInicial, 1f, acercamiento);
            if (_escenario != null) _escenario.localScale = new Vector3(escala, escala, 1f);
            if (_grupoPuerta != null) _grupoPuerta.alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.05f, 0.55f, x));
            float resto = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, x));
            if (_grupoInfo != null) _grupoInfo.alpha = resto;
            if (_grupoVolver != null) _grupoVolver.alpha = resto;
        }

        /// <returns>false si no pudo empezar (ya en curso, o escena fuera de Build Settings).</returns>
        public bool Reproducir(string nombreEscena, Sprite arteCapitulo)
        {
            if (_reproduciendo || EnEntrada) return false;
            if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                Debug.LogError($"[ClockDoorTransitionView] La escena '{nombreEscena}' no está en " +
                               "File > Build Settings > Scenes In Build.");
                return false;
            }

            _reproduciendo = true;
            StartCoroutine(Ejecutar(nombreEscena, arteCapitulo));
            return true;
        }

        private IEnumerator Ejecutar(string nombreEscena, Sprite arteCapitulo)
        {
            Preparar(arteCapitulo);

            // Carga la escena en segundo plano sin activarla todavía.
            var operacion = SceneManager.LoadSceneAsync(nombreEscena);
            operacion.allowSceneActivation = false;

            _alAbrir?.Invoke();
            float objetivoZoom = ZoomNecesario();
            float duracion = Mathf.Max(_tiempoApertura, _retrasoZoom + _tiempoZoom);
            bool zoomAvisado = false;

            for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
            {
                if (!zoomAvisado && t >= _retrasoZoom)
                {
                    zoomAvisado = true;
                    _alHacerZoom?.Invoke();
                }
                Aplicar(t, objetivoZoom);
                yield return null;
            }
            Aplicar(duracion, objetivoZoom);
            if (_fundidoFinal != null) _fundidoFinal.alpha = 1f;

            // Espera a que la escena esté lista y la activa.
            while (operacion.progress < 0.9f) yield return null;
            operacion.allowSceneActivation = true;
        }

        private void Preparar(Sprite arteCapitulo)
        {
            if (_imagenPortal != null && arteCapitulo != null)
            {
                _imagenPortal.sprite = arteCapitulo;
                _imagenPortal.color = Color.white;
            }
            if (_grupoPortal != null) _grupoPortal.alpha = 0f;
            if (_resplandorPortal != null) FijarAlpha(_resplandorPortal, 0f);
            if (_fundidoFinal != null) { _fundidoFinal.alpha = 0f; _fundidoFinal.blocksRaycasts = true; }
            if (_grupoInfo != null) _grupoInfo.blocksRaycasts = false;
            if (_grupoVolver != null) _grupoVolver.blocksRaycasts = false;
            if (_grupoPuerta != null) _grupoPuerta.blocksRaycasts = false;
        }

        /// <summary>Estado de toda la transición en el segundo <paramref name="t"/>.</summary>
        private void Aplicar(float t, float objetivoZoom)
        {
            // Apertura: la puerta gira hacia el fondo sobre su borde izquierdo.
            float apertura = EaseInOutCubic(t / Mathf.Max(0.01f, _tiempoApertura));
            if (_puerta != null) _puerta.localRotation = Quaternion.Euler(0f, -_anguloApertura * apertura, 0f);

            // El interior aparece apenas la puerta se despega, y los boxes se van enseguida.
            if (_grupoPortal != null) _grupoPortal.alpha = Mathf.Clamp01(apertura * 5f);
            if (_grupoInfo != null) _grupoInfo.alpha = 1f - Mathf.Clamp01(t / 0.25f);
            if (_grupoVolver != null) _grupoVolver.alpha = 1f - Mathf.Clamp01(t / 0.25f);

            // Zoom: avanzamos hacia el interior hasta que el círculo cubre toda la pantalla.
            float zoom = EaseInCubic((t - _retrasoZoom) / Mathf.Max(0.01f, _tiempoZoom));
            float escala = Mathf.Lerp(1f, objetivoZoom, zoom);
            if (_escenario != null) _escenario.localScale = new Vector3(escala, escala, 1f);

            if (_resplandorPortal != null) FijarAlpha(_resplandorPortal, Mathf.Clamp01(apertura * 1.5f) * (1f - zoom));
            if (_grupoPuerta != null) _grupoPuerta.alpha = 1f - Mathf.InverseLerp(0.35f, 0.85f, zoom);
            if (_fundidoFinal != null) _fundidoFinal.alpha = Mathf.InverseLerp(0.8f, 1f, zoom);
        }

        private float ZoomNecesario()
        {
            // Escala para que el círculo del portal cubra la diagonal de la pantalla.
            if (_portal == null || _escenario == null) return 3f;
            float diagonal = _escenario.rect.size.magnitude;
            float diametro = Mathf.Max(1f, _portal.rect.width);
            return Mathf.Max(1.5f, diagonal / diametro * 1.15f);
        }

        private static void FijarAlpha(Graphic g, float alpha)
        {
            var c = g.color;
            c.a = alpha;
            g.color = c;
        }

        private static float EaseInOutCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        private static float EaseOutQuad(float x)
        {
            x = Mathf.Clamp01(x);
            return 1f - (1f - x) * (1f - x);
        }

        private static float EaseOutCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return 1f - Mathf.Pow(1f - x, 3f);
        }

        private static float EaseInCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x;
        }
    }
}

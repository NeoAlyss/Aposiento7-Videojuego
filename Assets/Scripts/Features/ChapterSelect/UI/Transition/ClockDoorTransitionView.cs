using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UniversalPlatform.Features.ChapterSelect.UI.Transition
{
    /// <summary>
    /// Transición al entrar a un capítulo: el reloj se parte por la mitad y se abre como una puerta
    /// de dos hojas. Detrás aparece un portal circular (enmascarado en el centro) con la imagen del
    /// capítulo; se hace zoom hasta llenar la pantalla mientras la escena carga en segundo plano.
    ///
    /// Sin tocar tu arte: al pulsar se captura la pantalla, se corta en mitad izquierda y derecha
    /// (RawImages) y cada mitad "se abre" con scaleX 1 -> 0 anclada en su borde exterior (bisagra).
    /// Funciona con cualquier reloj que dibujes, animado o no.
    /// </summary>
    public class ClockDoorTransitionView : MonoBehaviour
    {
        [Header("Reloj real (se oculta al empezar)")]
        [SerializeField] private CanvasGroup _grupoReloj;

        [Header("Capa de transición (a pantalla completa, desactivada al inicio)")]
        [SerializeField] private GameObject _capaRaiz;
        [Tooltip("Anclas (0,0)-(0.5,1), Pivot (0, 0.5), márgenes 0.")]
        [SerializeField] private RawImage _puertaIzquierda;
        [Tooltip("Anclas (0.5,0)-(1,1), Pivot (1, 0.5), márgenes 0.")]
        [SerializeField] private RawImage _puertaDerecha;

        [Header("Portal (detrás de las puertas)")]
        [Tooltip("RectTransform con Mask + Image circular. Hijo: Image con el arte del capítulo.")]
        [SerializeField] private RectTransform _portal;
        [SerializeField] private Image _imagenPortal;
        [SerializeField] private Image _resplandorPortal;

        [Header("Fundido final")]
        [SerializeField] private CanvasGroup _fundidoFinal;

        [Header("Tiempos")]
        [SerializeField] private float _tiempoApertura = 1.1f;
        [SerializeField] private float _tiempoZoom = 1.3f;
        [Range(0.3f, 1f)] [SerializeField] private float _sombraPuertas = 0.55f;

        [Header("Eventos (sonido)")]
        [SerializeField] private UnityEvent _alAbrir;
        [SerializeField] private UnityEvent _alHacerZoom;

        private bool _reproduciendo;

        /// <returns>false si no pudo empezar (ya en curso, o escena fuera de Build Settings).</returns>
        public bool Reproducir(string nombreEscena, Sprite arteCapitulo)
        {
            if (_reproduciendo) return false;
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
            // 1) Captura al final del frame (incluye el reloj con todo su arte).
            yield return new WaitForEndOfFrame();
            Texture2D captura = null;
            try { captura = ScreenCapture.CaptureScreenshotAsTexture(); }
            catch (System.Exception e) { Debug.LogWarning("No se pudo capturar la pantalla: " + e.Message); }

            PrepararCapa(arteCapitulo, captura);
            if (_grupoReloj != null) { _grupoReloj.alpha = 0f; _grupoReloj.blocksRaycasts = false; }

            // 2) Carga la escena en segundo plano sin activarla todavía.
            var operacion = SceneManager.LoadSceneAsync(nombreEscena);
            operacion.allowSceneActivation = false;

            // 3) Las puertas se abren.
            _alAbrir?.Invoke();
            var izq = _puertaIzquierda != null ? _puertaIzquierda.rectTransform : null;
            var der = _puertaDerecha != null ? _puertaDerecha.rectTransform : null;
            for (float t = 0f; t < _tiempoApertura; t += Time.unscaledDeltaTime)
            {
                float k = EaseInOutCubic(t / _tiempoApertura);
                AplicarPuertas(k, izq, der);
                if (_resplandorPortal != null) FijarAlpha(_resplandorPortal, Mathf.Clamp01(k * 1.5f));
                yield return null;
            }
            AplicarPuertas(1f, izq, der);

            // 4) Zoom al portal hasta que el círculo cubre toda la pantalla.
            _alHacerZoom?.Invoke();
            float objetivo = ZoomNecesario();
            for (float t = 0f; t < _tiempoZoom; t += Time.unscaledDeltaTime)
            {
                float k = EaseInCubic(t / _tiempoZoom);
                float s = Mathf.Lerp(1f, objetivo, k);
                _portal.localScale = new Vector3(s, s, 1f);
                if (_resplandorPortal != null) FijarAlpha(_resplandorPortal, 1f - k);
                if (_fundidoFinal != null) _fundidoFinal.alpha = Mathf.InverseLerp(0.75f, 1f, k);
                yield return null;
            }
            _portal.localScale = new Vector3(objetivo, objetivo, 1f);
            if (_fundidoFinal != null) _fundidoFinal.alpha = 1f;

            // 5) Espera a que la escena esté lista y la activa.
            while (operacion.progress < 0.9f) yield return null;
            if (captura != null) Destroy(captura);
            operacion.allowSceneActivation = true;
        }

        private void PrepararCapa(Sprite arteCapitulo, Texture2D captura)
        {
            if (_capaRaiz != null) _capaRaiz.SetActive(true);

            ConfigurarPuerta(_puertaIzquierda, captura, new Rect(0f, 0f, 0.5f, 1f));
            ConfigurarPuerta(_puertaDerecha, captura, new Rect(0.5f, 0f, 0.5f, 1f));

            if (_portal != null) _portal.localScale = Vector3.one;
            if (_imagenPortal != null && arteCapitulo != null) _imagenPortal.sprite = arteCapitulo;
            if (_resplandorPortal != null) FijarAlpha(_resplandorPortal, 0f);
            if (_fundidoFinal != null) _fundidoFinal.alpha = 0f;
        }

        private static void ConfigurarPuerta(RawImage puerta, Texture2D captura, Rect uv)
        {
            if (puerta == null) return;
            puerta.texture = captura;
            puerta.uvRect = uv;
            puerta.color = captura != null ? Color.white : Color.black;
            puerta.raycastTarget = false;
        }

        private void AplicarPuertas(float k, RectTransform izq, RectTransform der)
        {
            // scaleX 1 -> 0 anclado en el borde exterior = puerta que se abre girando sobre su bisagra
            float sx = 1f - k;
            if (izq != null) izq.localScale = new Vector3(sx, 1f, 1f);
            if (der != null) der.localScale = new Vector3(sx, 1f, 1f);

            float sombra = Mathf.Lerp(1f, _sombraPuertas, k);
            var c = new Color(sombra, sombra, sombra, 1f);
            if (_puertaIzquierda != null && _puertaIzquierda.texture != null) _puertaIzquierda.color = c;
            if (_puertaDerecha != null && _puertaDerecha.texture != null) _puertaDerecha.color = c;
        }

        private float ZoomNecesario()
        {
            // Escala para que el círculo (diámetro = ancho del portal) cubra la diagonal de la pantalla.
            var padre = (RectTransform)_portal.parent;
            float diagonal = padre.rect.size.magnitude;
            float diametro = Mathf.Max(1f, _portal.rect.width);
            return diagonal / diametro * 1.15f;
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

        private static float EaseInCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x;
        }
    }
}

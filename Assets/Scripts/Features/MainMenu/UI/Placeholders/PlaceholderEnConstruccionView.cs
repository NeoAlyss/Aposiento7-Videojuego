using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniversalPlatform.Features.MainMenu.UI.Placeholders
{
    /// <summary>
    /// Panel temporal "En construcción" (Opciones, aviso de "no hay partida guardada", etc.).
    /// Se cierra con clic, Escape o Espacio (lo cierra quien lo muestra), o solo tras un tiempo.
    /// Requiere un CanvasGroup y un Graphic en el objeto (para recibir el clic).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PlaceholderEnConstruccionView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text _mensaje;
        [SerializeField] private string _mensajePorDefecto = "En construcción";
        [SerializeField] private float _duracionFade = 0.2f;
        [Tooltip("0 = no se cierra solo.")]
        [SerializeField] private float _cerrarSolo = 0f;

        private CanvasGroup _grupo;
        private Coroutine _rutina;

        public bool EstaVisible { get; private set; }

        private void Awake()
        {
            _grupo = GetComponent<CanvasGroup>();
            Aplicar(0f);
        }

        public void Mostrar(string mensaje = null)
        {
            if (_mensaje != null) _mensaje.text = string.IsNullOrEmpty(mensaje) ? _mensajePorDefecto : mensaje;
            EstaVisible = true;
            Animar(1f);
            if (_cerrarSolo > 0f) StartCoroutine(CerrarTrasEspera());
        }

        public void Ocultar()
        {
            EstaVisible = false;
            Animar(0f);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (EstaVisible) Ocultar();
        }

        private IEnumerator CerrarTrasEspera()
        {
            yield return new WaitForSecondsRealtime(_cerrarSolo);
            if (EstaVisible) Ocultar();
        }

        private void Animar(float destino)
        {
            if (_rutina != null) StopCoroutine(_rutina);
            _rutina = StartCoroutine(Fade(destino));
        }

        private IEnumerator Fade(float destino)
        {
            float inicio = _grupo.alpha;
            for (float t = 0f; t < _duracionFade; t += Time.unscaledDeltaTime)
            {
                _grupo.alpha = Mathf.Lerp(inicio, destino, t / _duracionFade);
                yield return null;
            }
            Aplicar(destino);
            _rutina = null;
        }

        private void Aplicar(float alpha)
        {
            _grupo.alpha = alpha;
            _grupo.blocksRaycasts = alpha > 0.5f;
            _grupo.interactable = alpha > 0.5f;
        }
    }
}

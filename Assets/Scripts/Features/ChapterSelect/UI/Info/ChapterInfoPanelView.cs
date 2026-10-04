using System.Collections;
using TMPro;
using UnityEngine;
using UniversalPlatform.Features.ChapterSelect.Domain;

namespace UniversalPlatform.Features.ChapterSelect.UI.Info
{
    /// <summary>
    /// Los "boxes" con la información del capítulo: título, dificultad, llaves y tiempo de juego.
    /// Aparecen escalonados con fade + deslizamiento y se ubican en el lado opuesto al número
    /// seleccionado. Cada box es un objeto con CanvasGroup; adentro va tu arte + un TMP_Text.
    /// </summary>
    public class ChapterInfoPanelView : MonoBehaviour
    {
        [Header("Boxes (con CanvasGroup)")]
        [SerializeField] private CanvasGroup _cajaTitulo;
        [SerializeField] private CanvasGroup _cajaDificultad;
        [SerializeField] private CanvasGroup _cajaLlaves;
        [SerializeField] private CanvasGroup _cajaTiempo;

        [Header("Textos")]
        [SerializeField] private TMP_Text _textoTitulo;
        [SerializeField] private TMP_Text _textoDificultad;
        [SerializeField] private TMP_Text _textoLlaves;
        [SerializeField] private TMP_Text _textoTiempo;

        [Header("Ubicación (objeto padre anclado al centro, colocado a la derecha)")]
        [SerializeField] private RectTransform _raizPanel;
        [SerializeField] private bool _reflejarAlLadoOpuesto = true;

        [Header("Animación")]
        [SerializeField] private float _distanciaDeslizamiento = 40f;
        [SerializeField] private float _duracionCaja = 0.22f;
        [SerializeField] private float _escalonado = 0.07f;

        private CanvasGroup[] _cajas;
        private Vector2[] _posicionesBase;
        private Vector2 _posicionRaizBase;
        private Coroutine _rutina;

        private void Awake()
        {
            _cajas = new[] { _cajaTitulo, _cajaDificultad, _cajaLlaves, _cajaTiempo };
            _posicionesBase = new Vector2[_cajas.Length];
            for (int i = 0; i < _cajas.Length; i++)
            {
                if (_cajas[i] == null) continue;
                _posicionesBase[i] = ((RectTransform)_cajas[i].transform).anchoredPosition;
                _cajas[i].alpha = 0f;
            }
            if (_raizPanel != null) _posicionRaizBase = _raizPanel.anchoredPosition;
        }

        public void Mostrar(DetalleCapitulo detalle, bool numeroEnLadoDerecho)
        {
            if (_raizPanel != null && _reflejarAlLadoOpuesto)
            {
                float lado = numeroEnLadoDerecho ? -1f : 1f;      // va al lado contrario del número
                _raizPanel.anchoredPosition = new Vector2(Mathf.Abs(_posicionRaizBase.x) * lado, _posicionRaizBase.y);
            }

            if (_textoTitulo != null) _textoTitulo.text = detalle.Titulo;
            if (_textoDificultad != null) _textoDificultad.text = detalle.Dificultad;
            if (_textoLlaves != null) _textoLlaves.text = detalle.Llaves;
            if (_textoTiempo != null) _textoTiempo.text = detalle.Tiempo;

            if (_rutina != null) StopCoroutine(_rutina);
            _rutina = StartCoroutine(Revelar());
        }

        private IEnumerator Revelar()
        {
            for (int i = 0; i < _cajas.Length; i++)
            {
                if (_cajas[i] == null) continue;
                _cajas[i].alpha = 0f;
                ((RectTransform)_cajas[i].transform).anchoredPosition = _posicionesBase[i] + Vector2.right * _distanciaDeslizamiento;
            }

            float total = _duracionCaja + _escalonado * (_cajas.Length - 1);
            for (float t = 0f; t < total; t += Time.unscaledDeltaTime)
            {
                for (int i = 0; i < _cajas.Length; i++)
                {
                    if (_cajas[i] == null) continue;
                    float k = Mathf.Clamp01((t - _escalonado * i) / _duracionCaja);
                    float e = 1f - Mathf.Pow(1f - k, 3f);          // ease-out cúbico
                    _cajas[i].alpha = e;
                    ((RectTransform)_cajas[i].transform).anchoredPosition =
                        _posicionesBase[i] + Vector2.right * (_distanciaDeslizamiento * (1f - e));
                }
                yield return null;
            }

            for (int i = 0; i < _cajas.Length; i++)
            {
                if (_cajas[i] == null) continue;
                _cajas[i].alpha = 1f;
                ((RectTransform)_cajas[i].transform).anchoredPosition = _posicionesBase[i];
            }
            _rutina = null;
        }

        /// <summary>Pequeño temblor al intentar entrar a un capítulo bloqueado.</summary>
        public void Temblar()
        {
            if (_raizPanel != null) StartCoroutine(TemblarRutina());
        }

        private IEnumerator TemblarRutina()
        {
            var inicio = _raizPanel.anchoredPosition;
            for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
            {
                float k = 1f - t / 0.25f;
                _raizPanel.anchoredPosition = inicio + Vector2.right * (Mathf.Sin(t * 70f) * 10f * k);
                yield return null;
            }
            _raizPanel.anchoredPosition = inicio;
        }
    }
}

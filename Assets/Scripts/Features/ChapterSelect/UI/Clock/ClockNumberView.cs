using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniversalPlatform.Features.ChapterSelect.UI.Clock
{
    /// <summary>
    /// Uno de los 12 números del reloj (= un capítulo). Se ilumina y hace scale cuando está
    /// seleccionado y se ve apagado si el capítulo está bloqueado. El área de hover/clic es el
    /// Graphic del propio objeto: dale un tamaño cómodo (ej. 120x120).
    /// </summary>
    public class ClockNumberView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Tooltip("Lo que se tiñe: el TMP_Text o la Image con tu numeral dibujado.")]
        [SerializeField] private Graphic[] _elementos;

        [SerializeField] private Color _colorNormal = new Color(0.85f, 0.78f, 0.62f, 0.85f);
        [SerializeField] private Color _colorIluminado = new Color(1f, 0.9f, 0.6f, 1f);
        [SerializeField] private Color _colorBloqueado = new Color(0.45f, 0.45f, 0.5f, 0.6f);
        [SerializeField] private Color _colorBloqueadoIluminado = new Color(0.65f, 0.65f, 0.72f, 0.9f);
        [SerializeField] private float _escalaIluminado = 1.25f;
        [SerializeField] private float _velocidad = 12f;

        private int _numero;
        private Action<int> _alEntrar, _alClic;
        private bool _resaltado, _bloqueado;
        private float _t;
        private Vector3 _escalaBase;
        private bool _iniciado;

        private void Awake()
        {
            Iniciar();
            Aplicar(_t);
        }

        /// <summary>
        /// El Installer puede llamar a SetBloqueado antes de que corra este Awake (Unity no garantiza
        /// el orden entre objetos). Por eso la escala original se captura la primera vez que hace
        /// falta, no solo en Awake: si no, quedaría en cero y el número desaparecería.
        /// </summary>
        private void Iniciar()
        {
            if (_iniciado) return;
            _iniciado = true;
            _escalaBase = transform.localScale;

            // Garantiza un área que reciba el mouse aunque el número sea solo un TMP pequeño.
            if (GetComponent<Graphic>() == null)
            {
                var area = gameObject.AddComponent<Image>();
                area.color = new Color(0, 0, 0, 0);
                area.raycastTarget = true;
            }
        }

        public void Construir(int numero, Action<int> alEntrar, Action<int> alClic)
        {
            _numero = numero;
            _alEntrar = alEntrar;
            _alClic = alClic;
        }

        public void SetResaltado(bool valor) => _resaltado = valor;

        public void SetBloqueado(bool valor)
        {
            _bloqueado = valor;
            Aplicar(_t);
        }

        private void Update()
        {
            float objetivo = _resaltado ? 1f : 0f;
            if (Mathf.Approximately(_t, objetivo)) return;
            _t = Mathf.MoveTowards(_t, objetivo, _velocidad * Time.unscaledDeltaTime);
            Aplicar(_t);
        }

        private void Aplicar(float t)
        {
            Iniciar();
            float e = Mathf.SmoothStep(0f, 1f, t);
            var a = _bloqueado ? _colorBloqueado : _colorNormal;
            var b = _bloqueado ? _colorBloqueadoIluminado : _colorIluminado;
            var color = Color.Lerp(a, b, e);

            if (_elementos != null)
                foreach (var g in _elementos)
                    if (g != null) g.color = color;

            transform.localScale = _escalaBase * Mathf.Lerp(1f, _escalaIluminado, e);
        }

        public void OnPointerEnter(PointerEventData e) => _alEntrar?.Invoke(_numero);

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) _alClic?.Invoke(_numero);
        }
    }
}

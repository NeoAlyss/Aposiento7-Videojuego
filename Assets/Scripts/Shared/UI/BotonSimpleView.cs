using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Botón de texto (y opcionalmente un ícono): atenuado normalmente; con el mouse encima (o
    /// seleccionado con el teclado) pasa a blanco pleno y crece un poco. El área de clic es el
    /// Graphic del propio objeto.
    /// </summary>
    public class BotonSimpleView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("Lo que se tiñe: el texto y, si hay, el ícono.")]
        [SerializeField] private Graphic[] _graficos;
        [SerializeField] private Color _colorNormal = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private Color _colorIluminado = Color.white;
        [SerializeField] private float _escalaIluminado = 1.12f;
        [SerializeField] private float _velocidad = 12f;

        private Action _alClic;
        private bool _encima, _resaltado, _iniciado;
        private float _t;
        private Vector3 _escalaBase;

        public void Construir(Action alClic) => _alClic = alClic;

        /// <summary>Seleccionado con el teclado: se ve igual que con el mouse encima.</summary>
        public void SetResaltado(bool resaltado) => _resaltado = resaltado;

        private void Iniciar()
        {
            if (_iniciado) return;
            _iniciado = true;
            _escalaBase = transform.localScale;
        }

        private void Awake()
        {
            Iniciar();
            Aplicar(0f);
        }

        private void Update()
        {
            float objetivo = _encima || _resaltado ? 1f : 0f;
            if (Mathf.Approximately(_t, objetivo)) return;
            _t = Mathf.MoveTowards(_t, objetivo, _velocidad * Time.unscaledDeltaTime);
            Aplicar(_t);
        }

        private void Aplicar(float t)
        {
            Iniciar();
            float e = Mathf.SmoothStep(0f, 1f, t);
            var color = Color.Lerp(_colorNormal, _colorIluminado, e);
            if (_graficos != null)
                foreach (var g in _graficos)
                    if (g != null) g.color = color;
            transform.localScale = _escalaBase * Mathf.Lerp(1f, _escalaIluminado, e);
        }

        public void OnPointerEnter(PointerEventData e) => _encima = true;
        public void OnPointerExit(PointerEventData e) => _encima = false;

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) _alClic?.Invoke();
        }
    }
}

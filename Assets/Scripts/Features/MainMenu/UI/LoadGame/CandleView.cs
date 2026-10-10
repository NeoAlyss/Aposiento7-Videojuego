using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniversalPlatform.Features.MainMenu.UI.LoadGame
{
    /// <summary>
    /// Una vela = una ranura de guardado. Encendida si la ranura tiene partida (la llama titila);
    /// apagada y oscurecida si está vacía. Al seleccionarla crece un poco y su halo se intensifica.
    /// El área de hover/clic es el Graphic del propio objeto.
    /// </summary>
    public class CandleView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Header("Partes")]
        [SerializeField] private Graphic _cuerpo;
        [SerializeField] private Graphic _llama;
        [SerializeField] private Graphic _halo;

        [Header("Aspecto")]
        [SerializeField] private Color _colorEncendida = Color.white;
        [SerializeField] private Color _colorApagada = new Color(0.38f, 0.38f, 0.38f, 1f);
        [SerializeField] private Color _colorHalo = new Color(1f, 0.67f, 0.31f, 1f);
        [SerializeField, Range(0f, 1f)] private float _alphaHalo = 0.3f;
        [SerializeField, Range(0f, 1f)] private float _alphaHaloResaltada = 0.6f;
        [SerializeField] private float _escalaResaltada = 1.1f;
        [SerializeField] private float _velocidad = 10f;

        [Header("Titileo de la llama")]
        [SerializeField] private float _velocidadTitileo = 3.5f;
        [SerializeField, Range(0f, 0.5f)] private float _fuerzaTitileo = 0.12f;

        private int _indice;
        private Action<int> _alEntrar, _alClic;
        private bool _encendida, _resaltada, _iniciada;
        private float _t;              // 0 = normal, 1 = resaltada
        private float _temblor;        // > 0 mientras tiembla (vela apagada que se intentó cargar)
        private float _semilla;
        private Vector3 _escalaBase, _escalaLlama;
        private Vector2 _posicionBase;
        private RectTransform _rect;

        private void Awake()
        {
            Iniciar();
        }

        private void Iniciar()
        {
            if (_iniciada) return;
            _iniciada = true;
            _rect = transform as RectTransform;
            _escalaBase = transform.localScale;
            _posicionBase = _rect != null ? _rect.anchoredPosition : Vector2.zero;
            _escalaLlama = _llama != null ? _llama.transform.localScale : Vector3.one;
            _semilla = UnityEngine.Random.value * 100f;

            // Garantiza un área que reciba el mouse.
            if (GetComponent<Graphic>() == null)
            {
                var area = gameObject.AddComponent<Image>();
                area.color = new Color(0, 0, 0, 0);
                area.raycastTarget = true;
            }
        }

        public void Construir(int indice, Action<int> alEntrar, Action<int> alClic)
        {
            _indice = indice;
            _alEntrar = alEntrar;
            _alClic = alClic;
        }

        public void SetEncendida(bool valor) => _encendida = valor;

        public void SetResaltada(bool valor) => _resaltada = valor;

        /// <summary>Pequeña sacudida: se intentó cargar una vela apagada.</summary>
        public void Temblar() => _temblor = 1f;

        private void Update()
        {
            Iniciar();
            float dt = Time.unscaledDeltaTime;
            _t = Mathf.MoveTowards(_t, _resaltada ? 1f : 0f, _velocidad * dt);
            _temblor = Mathf.MoveTowards(_temblor, 0f, 3f * dt);

            float e = Mathf.SmoothStep(0f, 1f, _t);
            float ruido = Mathf.PerlinNoise(Time.unscaledTime * _velocidadTitileo, _semilla);   // 0..1

            transform.localScale = _escalaBase * Mathf.Lerp(1f, _escalaResaltada, e);
            if (_rect != null)
                _rect.anchoredPosition = _posicionBase + new Vector2(Mathf.Sin(Time.unscaledTime * 55f) * 9f * _temblor, 0f);

            if (_cuerpo != null) _cuerpo.color = _encendida ? _colorEncendida : _colorApagada;

            if (_llama != null)
            {
                _llama.enabled = _encendida;
                float d = (ruido - 0.5f) * 2f * _fuerzaTitileo;
                _llama.transform.localScale = new Vector3(_escalaLlama.x * (1f - d * 0.5f), _escalaLlama.y * (1f + d), _escalaLlama.z);
            }

            if (_halo != null)
            {
                _halo.enabled = _encendida;
                var c = _colorHalo;
                c.a = Mathf.Lerp(_alphaHalo, _alphaHaloResaltada, e) * (0.85f + 0.3f * ruido);
                _halo.color = c;
            }
        }

        public void OnPointerEnter(PointerEventData e) => _alEntrar?.Invoke(_indice);

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) _alClic?.Invoke(_indice);
        }
    }
}

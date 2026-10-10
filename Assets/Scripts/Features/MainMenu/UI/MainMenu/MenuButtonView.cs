using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniversalPlatform.Features.MainMenu.UI.MainMenu
{
    /// <summary>
    /// Botón del menú principal. Normal: blanco atenuado. Resaltado (mouse encima o seleccionado por
    /// teclado): blanco pleno, más grande y con un resplandor en las letras.
    /// - Textos (TMP_Text): pasan del color normal al iluminado y ganan brillo (glow del propio texto).
    /// - Imágenes (tu arte): si asignas el material UI_Grayscale pasan de escala de grises a color
    ///   original; si no, se tiñen igual que los textos.
    /// No usa Selectable de Unity: MainMenuViewModel maneja la selección, por eso mouse, flechas y
    /// WASD comparten exactamente el mismo estado visual.
    /// </summary>
    public class MenuButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private static readonly int GrayscaleAmount = Shader.PropertyToID("_GrayscaleAmount");
        private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        private static readonly int GlowOffset = Shader.PropertyToID("_GlowOffset");
        private static readonly int GlowInner = Shader.PropertyToID("_GlowInner");
        private static readonly int GlowOuter = Shader.PropertyToID("_GlowOuter");
        private static readonly int GlowPower = Shader.PropertyToID("_GlowPower");

        [Header("Qué se anima")]
        [SerializeField] private Graphic[] _textos;
        [SerializeField] private Image[] _imagenes;
        [SerializeField] private Material _materialGrayscale;
        [SerializeField] private Graphic _brillo;
        [SerializeField, Range(0f, 1f)] private float _alphaMaximoBrillo = 0.6f;

        [Header("Colores y escala")]
        [Tooltip("Color sin seleccionar (el nombre viene del diseño original en gris).")]
        [SerializeField] private Color _colorGris = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private Color _colorIluminado = Color.white;
        [SerializeField] private float _escalaIluminado = 1.18f;
        [Tooltip("Resplandor de las letras al seleccionar (0 = sin resplandor). Solo textos TMP.")]
        [SerializeField, Range(0f, 1f)] private float _brilloTexto = 0.6f;
        [SerializeField] private float _velocidad = 12f;

        private int _indice;
        private Action<int> _alEntrar, _alSalir, _alClic;
        private float _t;                 // 0 = gris, 1 = iluminado
        private bool _resaltado;
        private Vector3 _escalaBase;
        private Material[] _instancias;
        private Material[] _materialesTexto;

        private void Awake()
        {
            _escalaBase = transform.localScale;

            if (_materialGrayscale != null && _imagenes != null)
            {
                _instancias = new Material[_imagenes.Length];
                for (int i = 0; i < _imagenes.Length; i++)
                {
                    if (_imagenes[i] == null) continue;
                    _instancias[i] = new Material(_materialGrayscale);
                    _imagenes[i].material = _instancias[i];
                }
            }
            PrepararBrilloDeTextos();
            Aplicar(0f);
        }

        /// <summary>
        /// Activa el glow del shader de TextMesh Pro en una copia del material de cada texto, para
        /// poder subirlo y bajarlo sin afectar a los demás textos que usan la misma fuente.
        /// </summary>
        private void PrepararBrilloDeTextos()
        {
            if (_textos == null || _brilloTexto <= 0f) return;
            _materialesTexto = new Material[_textos.Length];
            for (int i = 0; i < _textos.Length; i++)
            {
                var tmp = _textos[i] as TMP_Text;
                if (tmp == null) continue;
                var material = tmp.fontMaterial;          // instancia propia de este texto
                if (material == null || !material.HasProperty(GlowPower)) continue;
                material.EnableKeyword("GLOW_ON");
                material.SetColor(GlowColor, new Color(1f, 1f, 1f, 0.5f));
                material.SetFloat(GlowOffset, 0f);
                material.SetFloat(GlowInner, 0.05f);
                material.SetFloat(GlowOuter, 0.6f);
                material.SetFloat(GlowPower, 0f);
                tmp.UpdateMeshPadding();
                _materialesTexto[i] = material;
            }
        }

        private void OnDestroy()
        {
            if (_instancias == null) return;
            foreach (var m in _instancias)
                if (m != null) Destroy(m);
        }

        public void Construir(int indice, Action<int> alEntrar, Action<int> alSalir, Action<int> alClic)
        {
            _indice = indice;
            _alEntrar = alEntrar;
            _alSalir = alSalir;
            _alClic = alClic;
        }

        public void SetResaltado(bool valor) => _resaltado = valor;

        private void Update()
        {
            float objetivo = _resaltado ? 1f : 0f;
            if (Mathf.Approximately(_t, objetivo)) return;
            _t = Mathf.MoveTowards(_t, objetivo, _velocidad * Time.unscaledDeltaTime);
            Aplicar(_t);
        }

        private void Aplicar(float t)
        {
            float e = Mathf.SmoothStep(0f, 1f, t);
            var color = Color.Lerp(_colorGris, _colorIluminado, e);

            if (_textos != null)
                foreach (var g in _textos)
                    if (g != null) g.color = color;

            if (_imagenes != null)
            {
                for (int i = 0; i < _imagenes.Length; i++)
                {
                    if (_imagenes[i] == null) continue;
                    if (_instancias != null && _instancias[i] != null)
                    {
                        _instancias[i].SetFloat(GrayscaleAmount, 1f - e);
                        _imagenes[i].color = Color.white;
                    }
                    else
                    {
                        _imagenes[i].color = color;
                    }
                }
            }

            if (_materialesTexto != null)
                foreach (var m in _materialesTexto)
                    if (m != null) m.SetFloat(GlowPower, e * _brilloTexto);

            if (_brillo != null)
            {
                var c = _brillo.color;
                c.a = e * _alphaMaximoBrillo;
                _brillo.color = c;
            }
            transform.localScale = _escalaBase * Mathf.Lerp(1f, _escalaIluminado, e);
        }

        public void OnPointerEnter(PointerEventData e) => _alEntrar?.Invoke(_indice);
        public void OnPointerExit(PointerEventData e) => _alSalir?.Invoke(_indice);

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) _alClic?.Invoke(_indice);
        }
    }
}

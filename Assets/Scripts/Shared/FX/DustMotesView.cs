using UnityEngine;
using UnityEngine.UI;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Partículas de polvo que flotan y titilan dentro del rectángulo del objeto donde lo pongas.
    /// En un haz de luz (mismo objeto que LightBeamView) el polvo se ve solo dentro de la luz y se
    /// mece con ella; en un objeto a pantalla completa sirve de polvo ambiental.
    /// Son Images UI simples: funciona con cualquier modo de Canvas y sin materiales.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DustMotesView : MonoBehaviour
    {
        [SerializeField] private int _cantidad = 40;
        [SerializeField] private Color _color = new Color(1f, 0.92f, 0.75f, 1f);
        [SerializeField] private Vector2 _rangoTamano = new Vector2(2f, 6f);
        [SerializeField, Range(0f, 1f)] private float _alphaMaximo = 0.85f;
        [SerializeField] private float _velocidadAscenso = 6f;
        [SerializeField] private float _zigzag = 12f;
        [SerializeField] private float _velocidadTitileo = 0.8f;
        [SerializeField] private Sprite _sprite;

        private class Particula
        {
            public RectTransform rt;
            public Image img;
            public float x, y, fase, multiplicador;
        }

        private Particula[] _particulas;
        private RectTransform _area;

        private void Start()
        {
            _area = (RectTransform)transform;
            Canvas.ForceUpdateCanvases();
            var sprite = _sprite != null ? _sprite : ProceduralSprites.PuntoSuave();
            var r = _area.rect;
            _particulas = new Particula[_cantidad];

            for (int i = 0; i < _cantidad; i++)
            {
                var go = new GameObject("Mota", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_area, false);
                var tamano = Random.Range(_rangoTamano.x, _rangoTamano.y);
                var p = new Particula
                {
                    rt = (RectTransform)go.transform,
                    img = go.GetComponent<Image>(),
                    x = Random.Range(r.xMin, r.xMax),
                    y = Random.Range(r.yMin, r.yMax),
                    fase = Random.value * 100f,
                    multiplicador = Random.Range(0.5f, 1.5f)
                };
                p.img.sprite = sprite;
                p.img.raycastTarget = false;
                p.rt.sizeDelta = new Vector2(tamano, tamano);
                _particulas[i] = p;
            }
        }

        private void Update()
        {
            if (_particulas == null) return;
            var r = _area.rect;
            float dt = Time.unscaledDeltaTime;
            float t = Time.unscaledTime;

            foreach (var p in _particulas)
            {
                p.y += _velocidadAscenso * p.multiplicador * dt;
                if (p.y > r.yMax) { p.y = r.yMin; p.x = Random.Range(r.xMin, r.xMax); }

                float ox = Mathf.Sin(t * 0.5f * p.multiplicador + p.fase) * _zigzag;
                p.rt.anchoredPosition = new Vector2(p.x + ox, p.y);

                float titileo = 0.5f + 0.5f * Mathf.Sin(t * _velocidadTitileo * p.multiplicador * Mathf.PI * 2f + p.fase);
                float borde = Mathf.InverseLerp(r.yMin, r.yMin + r.height * 0.15f, p.y)
                            * Mathf.InverseLerp(r.yMax, r.yMax - r.height * 0.15f, p.y);
                p.img.color = new Color(_color.r, _color.g, _color.b, titileo * borde * _alphaMaximo);
            }
        }
    }
}

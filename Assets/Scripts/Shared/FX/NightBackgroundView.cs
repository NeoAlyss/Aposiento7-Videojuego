using UnityEngine;
using UnityEngine.UI;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Fondo de noche: azul muy oscuro al centro que degrada a negro hacia afuera.
    /// Ponlo en una Image a pantalla completa (anclas en stretch). Genera el degradé por código;
    /// si prefieres pintar tu propio fondo, no uses este componente.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class NightBackgroundView : MonoBehaviour
    {
        [SerializeField] private Color _colorCentro = new Color(0.05f, 0.09f, 0.30f, 1f);
        [SerializeField] private Color _colorBorde = Color.black;
        [SerializeField, Range(0.3f, 1.5f)] private float _radio = 0.85f;
        [SerializeField, Range(0.5f, 3f)] private float _caida = 1.4f;

        private void Awake()
        {
            const int tamano = 256;
            var tex = new Texture2D(tamano, tamano, TextureFormat.RGB24, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var px = new Color[tamano * tamano];
            for (int y = 0; y < tamano; y++)
            {
                for (int x = 0; x < tamano; x++)
                {
                    float u = (x + 0.5f) / tamano - 0.5f;
                    float v = (y + 0.5f) / tamano - 0.5f;
                    float d = Mathf.Clamp01(Mathf.Sqrt(u * u + v * v) * 2f / _radio);
                    px[y * tamano + x] = Color.Lerp(_colorCentro, _colorBorde, Mathf.Pow(d, _caida));
                }
            }
            tex.SetPixels(px);
            tex.Apply();

            var img = GetComponent<Image>();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tamano, tamano), new Vector2(0.5f, 0.5f), 100f);
            img.color = Color.white;
            img.raycastTarget = false;
        }
    }
}

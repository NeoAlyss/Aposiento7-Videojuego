using UnityEngine;
using UnityEngine.UI;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Fondo de las escenas de menú: escarlata al centro que degrada a carmesí oscuro hacia afuera
    /// (el nombre de la clase viene del fondo nocturno original). Los colores se cambian en el inspector.
    /// Ponlo en una Image a pantalla completa (anclas en stretch). Genera el degradé por código;
    /// si prefieres pintar tu propio fondo, no uses este componente.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class NightBackgroundView : MonoBehaviour
    {
        [SerializeField] private Color _colorCentro = new Color(0.86f, 0.12f, 0.06f, 1f);
        [SerializeField] private Color _colorBorde = new Color(0.35f, 0.02f, 0.03f, 1f);
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

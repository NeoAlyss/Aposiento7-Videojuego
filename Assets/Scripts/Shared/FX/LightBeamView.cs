using UnityEngine;
using UnityEngine.UI;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Haz de luz que se mece suavemente y "respira" en brillo. Usa una Image con Pivot = (0.5, 1)
    /// (nace arriba). Si no asignas sprite usa un haz procedural. Añade DustMotesView al mismo
    /// objeto para ver polvo dentro del haz.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LightBeamView : MonoBehaviour
    {
        [SerializeField] private Color _color = new Color(1f, 0.85f, 0.55f, 1f);
        [SerializeField, Range(0f, 1f)] private float _alphaMinimo = 0.10f;
        [SerializeField, Range(0f, 1f)] private float _alphaMaximo = 0.28f;
        [SerializeField] private float _velocidadRespiracion = 0.35f;
        [SerializeField] private float _anguloBalanceo = 3f;
        [SerializeField] private float _velocidadBalanceo = 0.18f;

        private Image _imagen;
        private float _anguloBase;
        private float _fase;

        private void Awake()
        {
            _imagen = GetComponent<Image>();
            if (_imagen.sprite == null) _imagen.sprite = ProceduralSprites.HazDeLuz();
            _imagen.raycastTarget = false;
            _anguloBase = transform.localEulerAngles.z;
            _fase = Random.value * 100f;
        }

        private void Update()
        {
            float t = Time.unscaledTime + _fase;
            float a = Mathf.Lerp(_alphaMinimo, _alphaMaximo,
                0.5f + 0.5f * Mathf.Sin(t * _velocidadRespiracion * Mathf.PI * 2f));
            _imagen.color = new Color(_color.r, _color.g, _color.b, a);
            transform.localRotation = Quaternion.Euler(0, 0,
                _anguloBase + Mathf.Sin(t * _velocidadBalanceo * Mathf.PI * 2f) * _anguloBalanceo);
        }
    }
}

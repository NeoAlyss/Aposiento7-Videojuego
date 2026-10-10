using System;
using UnityEngine;

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Sprites suaves generados por código (degradé radial, haz de luz, punto de polvo).
    /// Sirven de placeholder mientras no tengas tus propios assets; al asignar los tuyos
    /// en el inspector ya no se usan.
    /// </summary>
    public static class ProceduralSprites
    {
        private static Sprite _radial, _haz, _punto;

        public static Sprite BrilloRadial()
        {
            if (_radial == null) _radial = Crear(256, (u, v) =>
            {
                float d = Mathf.Clamp01(new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f);
                float a = 1f - d;
                return new Color(1, 1, 1, a * a);
            });
            return _radial;
        }

        /// <summary>Haz vertical: suave a los lados, más brillante arriba y se desvanece hacia abajo.</summary>
        public static Sprite HazDeLuz()
        {
            if (_haz == null) _haz = Crear(128, (u, v) =>
            {
                float lado = Mathf.SmoothStep(0f, 1f, 1f - Mathf.Abs(u - 0.5f) * 2f);
                float largo = Mathf.SmoothStep(0f, 1f, v);
                return new Color(1, 1, 1, lado * largo * 0.9f);
            });
            return _haz;
        }

        public static Sprite PuntoSuave()
        {
            if (_punto == null) _punto = Crear(32, (u, v) =>
            {
                float d = Mathf.Clamp01(new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f);
                return new Color(1, 1, 1, Mathf.Pow(1f - d, 2f));
            });
            return _punto;
        }

        private static Sprite Crear(int tamano, Func<float, float, Color> f)
        {
            var tex = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var px = new Color[tamano * tamano];
            for (int y = 0; y < tamano; y++)
                for (int x = 0; x < tamano; x++)
                    px[y * tamano + x] = f((x + 0.5f) / tamano, (y + 0.5f) / tamano);
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tamano, tamano), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

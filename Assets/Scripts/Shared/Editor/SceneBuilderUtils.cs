using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalPlatform.Shared.Editor
{
    /// <summary>
    /// Utilidades comunes para los SceneBuilder de cada feature: crear Canvas, Images, textos TMP y
    /// asignar campos [SerializeField] privados vía SerializedObject (el mismo mecanismo del inspector).
    /// </summary>
    public static class SceneBuilderUtils
    {
        public static readonly Color Dorado = new Color(1f, 0.86f, 0.55f, 1f);
        public static readonly Color GrisApagado = new Color(0.45f, 0.45f, 0.5f, 1f);

        public static Canvas CrearCanvas(string nombre)
        {
            var go = new GameObject(nombre, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        /// <summary>
        /// Canvas en "Screen Space - Camera" con una cámara en perspectiva. Se ve igual que uno
        /// Overlay mientras todo está plano, pero los elementos que giran en 3D (la puerta del reloj)
        /// se dibujan con perspectiva real.
        /// </summary>
        public static Canvas CrearCanvasConCamara(string nombre)
        {
            var camaraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camaraGo.tag = "MainCamera";
            camaraGo.transform.position = new Vector3(0f, 0f, -10f);
            var camara = camaraGo.GetComponent<Camera>();
            camara.orthographic = false;
            camara.fieldOfView = 50f;
            camara.nearClipPlane = 0.1f;
            camara.farClipPlane = 200f;
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = Color.black;

            var canvas = CrearCanvas(nombre);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camara;
            canvas.planeDistance = 10f;
            return canvas;
        }

        public static RectTransform CrearRect(string nombre, Transform padre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            return (RectTransform)go.transform;
        }

        public static Image CrearImage(string nombre, Transform padre, Color color, Sprite sprite = null, bool raycast = false)
        {
            var rt = CrearRect(nombre, padre);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static TextMeshProUGUI CrearTexto(string nombre, Transform padre, string texto, float tamano,
            Color color, TextAlignmentOptions alineacion = TextAlignmentOptions.Center)
        {
            var rt = CrearRect(nombre, padre);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = texto;
            tmp.fontSize = tamano;
            tmp.color = color;
            tmp.alignment = alineacion;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static void Estirar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Colocar(RectTransform rt, Vector2 posicion, Vector2 tamano)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posicion;
            rt.sizeDelta = tamano;
        }

        public static Sprite SpriteCirculo() =>
            AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        /// <summary>
        /// Carga un PNG del proyecto como Sprite (ruta completa, p. ej. "Assets/Art/Clock/x.png"). Si
        /// Unity lo importó como textura normal o como sprite múltiple, lo reimporta como sprite
        /// único. Devuelve null, avisando por consola, si el archivo no existe.
        /// </summary>
        /// <param name="pixelArt">true para pixel art: sin suavizado al agrandar (filtro Point) y sin compresión.</param>
        public static Sprite CargarSprite(string ruta, bool pixelArt = false)
        {
            var importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[SceneBuilder] No se encontró '{ruta}'; se usa arte provisional en su lugar.");
                return null;
            }

            bool cambiar = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.mipmapEnabled;
            if (pixelArt)
                cambiar |= importer.filterMode != FilterMode.Point
                    || importer.textureCompression != TextureImporterCompression.Uncompressed;

            if (cambiar)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                if (pixelArt)
                {
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                }
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite == null)
            {
                // Por si quedó como sprite múltiple: se usa el primero que tenga.
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(ruta))
                    if (asset is Sprite s) { sprite = s; break; }
            }
            if (sprite == null) Debug.LogWarning($"[SceneBuilder] '{ruta}' existe pero no se pudo cargar como Sprite.");
            return sprite;
        }

        public static void AsignarColor(Object objetivo, string campo, Color valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.colorValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- asignación de campos serializados ----------

        public static void Asignar(Object objetivo, string campo, Object valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.objectReferenceValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AsignarLista<T>(Object objetivo, string campo, IList<T> valores) where T : Object
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.arraySize = valores.Count;
            for (int i = 0; i < valores.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AsignarString(Object objetivo, string campo, string valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.stringValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AsignarFloat(Object objetivo, string campo, float valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.floatValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AsignarInt(Object objetivo, string campo, int valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[SceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.intValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- fondo + polvo (lo comparten ambas escenas) ----------

        public const string RutaFondo = "Assets/Art/Backgrounds/fondo_vitrales.png";

        /// <summary>
        /// Fondo de las escenas de menú: la imagen de los vitrales (ya viene desenfocada), oscurecida
        /// con <paramref name="brilloFondo"/> para que no compita con lo que va encima, más polvo en
        /// suspensión. Si la imagen no existe, usa el degradé generado por código con haces de luz.
        /// </summary>
        /// <param name="brilloFondo">0 = negro, 1 = la imagen tal cual. Se ajusta luego en Background > Image > Color.</param>
        public static void CrearAmbienteNocturno(Transform padre, float brilloFondo = 0.55f)
        {
            var spriteFondo = CargarSprite(RutaFondo);
            if (spriteFondo != null)
            {
                var imagen = CrearImage("Background", padre, new Color(brilloFondo, brilloFondo, brilloFondo, 1f), spriteFondo);
                Estirar(imagen.rectTransform);
                // Cubre toda la pantalla sin deformarse, sea cual sea la proporción de la ventana.
                var ajuste = imagen.gameObject.AddComponent<AspectRatioFitter>();
                ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                ajuste.aspectRatio = spriteFondo.rect.width / spriteFondo.rect.height;

                var polvo = CrearRect("PolvoAmbiental", padre);
                Estirar(polvo);
                polvo.gameObject.AddComponent<DustMotesView>();
                return;
            }

            var fondo = CrearImage("Background", padre, Color.white);
            Estirar(fondo.rectTransform);
            fondo.gameObject.AddComponent<NightBackgroundView>();

            float[] xs = { -650f, 100f, 700f };
            float[] angulos = { 14f, -6f, -16f };
            for (int i = 0; i < xs.Length; i++)
            {
                var haz = CrearImage($"Haz_{i + 1}", padre, Color.white);
                var rt = haz.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(xs[i], 80f);
                rt.sizeDelta = new Vector2(360f, 1250f);
                rt.localRotation = Quaternion.Euler(0, 0, angulos[i]);
                haz.gameObject.AddComponent<LightBeamView>();
                haz.gameObject.AddComponent<DustMotesView>();
            }

            var ambiente = CrearRect("PolvoAmbiental", padre);
            Estirar(ambiente);
            ambiente.gameObject.AddComponent<DustMotesView>();
        }
    }
}

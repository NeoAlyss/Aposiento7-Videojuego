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

        // ---------- fondo de noche + haces + polvo (lo comparten ambas escenas) ----------

        public static void CrearAmbienteNocturno(Transform padre)
        {
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

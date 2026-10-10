using UnityEditor;
using UnityEngine;
using UniversalPlatform.Features.Progress.DI;
using UniversalPlatform.Features.Progress.UI.ChapterTimer;

namespace UniversalPlatform.Features.Progress.Editor
{
    public static class ProgressSceneBuilder
    {
        [MenuItem("UniversalPlatform/Progress/Agregar cronómetro de capítulo a la escena")]
        public static void AgregarCronometro()
        {
            var raiz = new GameObject("ChapterProgress");
            var view = raiz.AddComponent<ChapterTimerView>();
            var installer = raiz.AddComponent<ProgressInstaller>();

            var so = new SerializedObject(installer);
            so.FindProperty("_view").objectReferenceValue = view;
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = raiz;
            Debug.Log("[ProgressSceneBuilder] Listo. Ajusta 'Id Capitulo' en ChapterTimerView (ej. chapter_01) y guarda la escena.");
        }
    }
}

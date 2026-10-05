using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Garantiza que exista un EventSystem con el módulo de input correcto (Input System nuevo o
    /// clásico) para que hover y clic del mouse funcionen sin importar qué sistema use el proyecto.
    /// Los Installers llaman a <see cref="Asegurar"/>; también puede ponerse como componente.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class EventSystemSetup : MonoBehaviour
    {
        private void Awake() => Asegurar();

        public static EventSystem Asegurar()
        {
#if UNITY_2023_1_OR_NEWER
            var es = FindAnyObjectByType<EventSystem>();
#else
            var es = FindObjectOfType<EventSystem>();
#endif
            if (es == null)
                es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            var clasico = es.GetComponent<StandaloneInputModule>();
            if (clasico != null) Destroy(clasico);
            if (es.GetComponent<InputSystemUIInputModule>() == null)
                es.gameObject.AddComponent<InputSystemUIInputModule>();
#else
            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();
#endif
            return es;
        }
    }
}

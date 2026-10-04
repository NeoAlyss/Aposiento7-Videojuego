using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UniversalPlatform.Shared
{
    /// <summary>
    /// Lectura de input para menús. Funciona con el Input System nuevo, con el Input Manager
    /// clásico o con ambos activados (Project Settings > Player > Active Input Handling).
    /// Flechas y WASD hacen lo mismo. Espacio / Enter confirman, Escape vuelve.
    /// </summary>
    public static class MenuInput
    {
        public static bool Arriba => Presionada(KeyCode.UpArrow, KeyCode.W);
        public static bool Abajo => Presionada(KeyCode.DownArrow, KeyCode.S);
        public static bool Izquierda => Presionada(KeyCode.LeftArrow, KeyCode.A);
        public static bool Derecha => Presionada(KeyCode.RightArrow, KeyCode.D);
        public static bool Confirmar => Presionada(KeyCode.Space, KeyCode.Return, KeyCode.KeypadEnter);
        public static bool Cancelar => Presionada(KeyCode.Escape);

        private static bool Presionada(params KeyCode[] teclas)
        {
            foreach (var tecla in teclas)
            {
#if ENABLE_INPUT_SYSTEM
                var teclado = Keyboard.current;
                if (teclado != null)
                {
                    var key = ConvertirATecla(tecla);
                    if (key != Key.None && teclado[key].wasPressedThisFrame) return true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKeyDown(tecla)) return true;
#endif
            }
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        private static Key ConvertirATecla(KeyCode tecla)
        {
            switch (tecla)
            {
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.W: return Key.W;
                case KeyCode.A: return Key.A;
                case KeyCode.S: return Key.S;
                case KeyCode.D: return Key.D;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.Escape: return Key.Escape;
                default: return Key.None;
            }
        }
#endif
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniversalPlatform.Features.MainMenu.DI;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.UI.MainMenu;
using UniversalPlatform.Features.MainMenu.UI.Placeholders;
using UniversalPlatform.Shared.Editor;

namespace UniversalPlatform.Features.MainMenu.Editor
{
    /// <summary>
    /// Arma la escena del menú principal con arte provisional (rectángulos y texto). Cuando tengas
    /// tus assets, reemplaza los sprites/colores en el inspector: los scripts no cambian.
    /// Uso: abre una escena vacía y ejecuta UniversalPlatform > MainMenu > Construir escena.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        [MenuItem("UniversalPlatform/MainMenu/Construir escena")]
        public static void Construir()
        {
            var canvas = SceneBuilderUtils.CrearCanvas("Canvas");
            var raiz = canvas.transform;

            // 1) Ambiente: fondo de noche, haces de luz y polvo
            SceneBuilderUtils.CrearAmbienteNocturno(raiz);

            // 2) Botones
            var contenedor = SceneBuilderUtils.CrearRect("Botones", raiz);
            SceneBuilderUtils.Colocar(contenedor, new Vector2(0f, -60f), new Vector2(700f, 480f));

            var opciones = new[]
            {
                OpcionMenuPrincipal.NuevaPartida, OpcionMenuPrincipal.CargarPartida,
                OpcionMenuPrincipal.Opciones, OpcionMenuPrincipal.Salir
            };
            var botones = new List<MenuButtonView>();
            for (int i = 0; i < opciones.Length; i++)
            {
                float y = 180f - i * 120f;
                botones.Add(CrearBoton(contenedor, ReglasMenuPrincipal.ObtenerNombre(opciones[i]), y));
            }

            // 3) Paneles temporales
            var panelOpciones = CrearPanelTemporal(raiz, "PanelOpciones", "Opciones\n\nEn construcción");
            var avisoSinPartida = CrearPanelTemporal(raiz, "AvisoSinPartida", "No hay ninguna partida guardada");

            // 4) Fundido a negro (siempre al final para quedar encima)
            var fade = SceneBuilderUtils.CrearImage("FadeOverlay", raiz, Color.black);
            SceneBuilderUtils.Estirar(fade.rectTransform);
            var grupoFade = fade.gameObject.AddComponent<CanvasGroup>();
            grupoFade.alpha = 0f;
            grupoFade.blocksRaycasts = false;
            grupoFade.interactable = false;

            // 5) Vista + Installer
            var vistaGo = SceneBuilderUtils.CrearRect("MainMenuView", raiz).gameObject;
            var vista = vistaGo.AddComponent<MainMenuView>();
            SceneBuilderUtils.AsignarLista(vista, "_botones", botones);
            SceneBuilderUtils.Asignar(vista, "_fadeOverlay", grupoFade);
            SceneBuilderUtils.Asignar(vista, "_panelOpciones", panelOpciones);
            SceneBuilderUtils.Asignar(vista, "_avisoSinPartida", avisoSinPartida);

            var installerGo = new GameObject("MainMenuInstaller");
            var installer = installerGo.AddComponent<MainMenuInstaller>();
            SceneBuilderUtils.Asignar(installer, "_view", vista);

            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[MainMenuSceneBuilder] Escena lista. Guárdala como 'MainMenu' y agrégala a Build Settings.");
        }

        private static MenuButtonView CrearBoton(Transform padre, string texto, float y)
        {
            var rt = SceneBuilderUtils.CrearRect($"Btn_{texto.Replace(" ", "")}", padre);
            SceneBuilderUtils.Colocar(rt, new Vector2(0f, y), new Vector2(620f, 100f));

            // Brillo detrás (alpha 0 hasta que se resalta)
            var brillo = SceneBuilderUtils.CrearImage("Brillo", rt, SceneBuilderUtils.Dorado, SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(brillo.rectTransform, Vector2.zero, new Vector2(760f, 200f));
            var c = brillo.color; c.a = 0f; brillo.color = c;

            var label = SceneBuilderUtils.CrearTexto("Texto", rt, texto, 58f, SceneBuilderUtils.GrisApagado);
            SceneBuilderUtils.Estirar(label.rectTransform);

            // Área que recibe el mouse (transparente)
            var area = rt.gameObject.AddComponent<Image>();
            area.color = new Color(0, 0, 0, 0);
            area.raycastTarget = true;

            var boton = rt.gameObject.AddComponent<MenuButtonView>();
            SceneBuilderUtils.AsignarLista(boton, "_textos", new List<Graphic> { label });
            SceneBuilderUtils.Asignar(boton, "_brillo", brillo);
            return boton;
        }

        private static PlaceholderEnConstruccionView CrearPanelTemporal(Transform padre, string nombre, string mensaje)
        {
            var fondo = SceneBuilderUtils.CrearImage(nombre, padre, new Color(0.12f, 0.01f, 0.02f, 0.92f), null, true);
            SceneBuilderUtils.Estirar(fondo.rectTransform);
            fondo.gameObject.AddComponent<CanvasGroup>();
            var vista = fondo.gameObject.AddComponent<PlaceholderEnConstruccionView>();

            var texto = SceneBuilderUtils.CrearTexto("Mensaje", fondo.transform, mensaje, 64f, SceneBuilderUtils.Dorado);
            SceneBuilderUtils.Estirar(texto.rectTransform);
            SceneBuilderUtils.Asignar(vista, "_mensaje", texto);
            return vista;
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniversalPlatform.Features.MainMenu.DI;
using UniversalPlatform.Features.MainMenu.Domain;
using UniversalPlatform.Features.MainMenu.UI.LoadGame;
using UniversalPlatform.Features.MainMenu.UI.MainMenu;
using UniversalPlatform.Features.MainMenu.UI.Placeholders;
using UniversalPlatform.Features.Progress.Domain;
using UniversalPlatform.Shared.Editor;

namespace UniversalPlatform.Features.MainMenu.Editor
{
    /// <summary>
    /// Arma la escena del menú principal: fondo de vitrales, botones de texto blanco y el submenú de
    /// partidas guardadas (una vela por ranura). El arte sale de Assets/Art; colores y tamaños se
    /// ajustan después en el inspector sin tocar los scripts.
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
            SceneBuilderUtils.CrearAmbienteNocturno(raiz, 0.55f);

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

            // 3b) Submenú de partidas guardadas (velas). Va encima de los botones y debajo del fundido.
            var vistaPartidas = CrearSubmenuPartidas(raiz);

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
            SceneBuilderUtils.Asignar(vista, "_vistaPartidas", vistaPartidas);

            var installerGo = new GameObject("MainMenuInstaller");
            var installer = installerGo.AddComponent<MainMenuInstaller>();
            SceneBuilderUtils.Asignar(installer, "_view", vista);
            SceneBuilderUtils.Asignar(installer, "_vistaPartidas", vistaPartidas);

            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[MainMenuSceneBuilder] Escena lista. Guárdala como 'MainMenu' y agrégala a Build Settings.");
        }

        /// <summary>
        /// Botón de solo texto: blanco atenuado y, al seleccionarlo, blanco pleno, más grande y con
        /// resplandor en las letras (lo anima MenuButtonView). Sin recuadro ni óvalo detrás.
        /// </summary>
        private static MenuButtonView CrearBoton(Transform padre, string texto, float y, float tamanoTexto = 58f)
        {
            var rt = SceneBuilderUtils.CrearRect($"Btn_{texto.Replace(" ", "")}", padre);
            SceneBuilderUtils.Colocar(rt, new Vector2(0f, y), new Vector2(620f, 100f));

            var label = SceneBuilderUtils.CrearTexto("Texto", rt, texto, tamanoTexto, new Color(1f, 1f, 1f, 0.55f));
            SceneBuilderUtils.Estirar(label.rectTransform);

            // Área que recibe el mouse (transparente)
            var area = rt.gameObject.AddComponent<Image>();
            area.color = new Color(0, 0, 0, 0);
            area.raycastTarget = true;

            var boton = rt.gameObject.AddComponent<MenuButtonView>();
            SceneBuilderUtils.AsignarLista(boton, "_textos", new List<Graphic> { label });
            return boton;
        }

        // ---------- submenú de partidas guardadas ----------

        private const string CarpetaVela = "Assets/Art/Candle";

        private static LoadGameView CrearSubmenuPartidas(Transform padre)
        {
            // Velo negro a pantalla completa: tapa el menú y bloquea sus botones mientras está abierto.
            var velo = SceneBuilderUtils.CrearImage("CargarPartida", padre, new Color(0f, 0f, 0f, 0.88f), null, true);
            SceneBuilderUtils.Estirar(velo.rectTransform);
            var grupo = velo.gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.blocksRaycasts = false;
            grupo.interactable = false;
            var vista = velo.gameObject.AddComponent<LoadGameView>();
            var raiz = velo.transform;

            var titulo = SceneBuilderUtils.CrearTexto("Titulo", raiz, "Cargar partida", 64f, Color.white);
            SceneBuilderUtils.Colocar(titulo.rectTransform, new Vector2(0f, 465f), new Vector2(1400f, 90f));
            var subtitulo = SceneBuilderUtils.CrearTexto("Subtitulo", raiz, "Elige una vela encendida para continuar.", 32f, new Color(1f, 1f, 1f, 0.7f));
            SceneBuilderUtils.Colocar(subtitulo.rectTransform, new Vector2(0f, 400f), new Vector2(1400f, 50f));

            // Una vela por ranura, repartidas de un lado al otro de la pantalla.
            var spriteCuerpo = SceneBuilderUtils.CargarSprite($"{CarpetaVela}/vela_cuerpo.png");
            var spriteLlama = SceneBuilderUtils.CargarSprite($"{CarpetaVela}/vela_llama.png");
            var spriteHalo = SceneBuilderUtils.CargarSprite($"{CarpetaVela}/vela_halo.png");
            var contenedor = SceneBuilderUtils.CrearRect("Velas", raiz);
            SceneBuilderUtils.Colocar(contenedor, Vector2.zero, new Vector2(10f, 10f));
            var velas = new List<CandleView>();
            const float separacion = 560f;
            for (int i = 0; i < ReglasRanuras.CANTIDAD; i++)
            {
                float x = (i - (ReglasRanuras.CANTIDAD - 1) * 0.5f) * separacion;
                velas.Add(CrearVela(contenedor, i + 1, x, spriteCuerpo, spriteLlama, spriteHalo));
            }

            // Recuadro negro con la información de la vela seleccionada (va delante de las velas).
            var caja = SceneBuilderUtils.CrearImage("CajaInfo", raiz, new Color(0f, 0f, 0f, 0.85f));
            SceneBuilderUtils.Colocar(caja.rectTransform, new Vector2(0f, -300f), new Vector2(860f, 230f));
            var grupoCaja = caja.gameObject.AddComponent<CanvasGroup>();
            grupoCaja.alpha = 0f;
            grupoCaja.blocksRaycasts = false;
            grupoCaja.interactable = false;
            var textoInfo = SceneBuilderUtils.CrearTexto("Texto", caja.transform, "", 32f, Color.white);
            SceneBuilderUtils.Estirar(textoInfo.rectTransform);
            textoInfo.rectTransform.offsetMin = new Vector2(30f, 15f);
            textoInfo.rectTransform.offsetMax = new Vector2(-30f, -15f);

            var volver = CrearBoton(raiz, "Volver al menú principal", -475f, 40f);

            SceneBuilderUtils.AsignarLista(vista, "_velas", velas);
            SceneBuilderUtils.Asignar(vista, "_titulo", titulo);
            SceneBuilderUtils.Asignar(vista, "_subtitulo", subtitulo);
            SceneBuilderUtils.Asignar(vista, "_cajaInfo", grupoCaja);
            SceneBuilderUtils.Asignar(vista, "_textoInfo", textoInfo);
            SceneBuilderUtils.Asignar(vista, "_botonVolver", volver);
            return vista;
        }

        /// <summary>
        /// Vela armada con tres imágenes: halo, cuerpo y llama. Las medidas están pensadas para los PNG
        /// de Assets/Art/Candle (cuerpo 256x640 con la mecha arriba; llama 128x256 con la base abajo).
        /// </summary>
        private static CandleView CrearVela(Transform padre, int numero, float x, Sprite cuerpo, Sprite llama, Sprite halo)
        {
            var rt = SceneBuilderUtils.CrearRect($"Vela_{numero}", padre);
            SceneBuilderUtils.Colocar(rt, new Vector2(x, -75f), new Vector2(260f, 900f));
            rt.localScale = Vector3.one * 0.8f;

            // Área que recibe el mouse (transparente)
            var area = rt.gameObject.AddComponent<Image>();
            area.color = new Color(0, 0, 0, 0);
            area.raycastTarget = true;

            var imgHalo = SceneBuilderUtils.CrearImage("Halo", rt, new Color(1f, 0.67f, 0.31f, 0f),
                halo != null ? halo : SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(imgHalo.rectTransform, new Vector2(0f, 290f), new Vector2(640f, 640f));

            // Sin PNG: un rectángulo rojo y un círculo ámbar hacen de vela provisional.
            var imgCuerpo = SceneBuilderUtils.CrearImage("Cuerpo", rt,
                cuerpo != null ? Color.white : new Color(0.75f, 0.1f, 0.1f, 1f), cuerpo);
            SceneBuilderUtils.Colocar(imgCuerpo.rectTransform, new Vector2(0f, -30f),
                cuerpo != null ? new Vector2(256f, 640f) : new Vector2(70f, 560f));

            var imgLlama = SceneBuilderUtils.CrearImage("Llama", rt,
                llama != null ? Color.white : new Color(1f, 0.75f, 0.3f, 1f),
                llama != null ? llama : SceneBuilderUtils.SpriteCirculo());
            imgLlama.rectTransform.pivot = new Vector2(0.5f, 0f);   // crece desde la mecha
            SceneBuilderUtils.Colocar(imgLlama.rectTransform, new Vector2(0f, 250f),
                llama != null ? new Vector2(128f, 256f) : new Vector2(60f, 120f));

            var vela = rt.gameObject.AddComponent<CandleView>();
            SceneBuilderUtils.Asignar(vela, "_cuerpo", imgCuerpo);
            SceneBuilderUtils.Asignar(vela, "_llama", imgLlama);
            SceneBuilderUtils.Asignar(vela, "_halo", imgHalo);
            return vela;
        }

        private static PlaceholderEnConstruccionView CrearPanelTemporal(Transform padre, string nombre, string mensaje)
        {
            var fondo = SceneBuilderUtils.CrearImage(nombre, padre, new Color(0f, 0f, 0f, 0.88f), null, true);
            SceneBuilderUtils.Estirar(fondo.rectTransform);
            fondo.gameObject.AddComponent<CanvasGroup>();
            var vista = fondo.gameObject.AddComponent<PlaceholderEnConstruccionView>();

            var texto = SceneBuilderUtils.CrearTexto("Mensaje", fondo.transform, mensaje, 64f, Color.white);
            SceneBuilderUtils.Estirar(texto.rectTransform);
            SceneBuilderUtils.Asignar(vista, "_mensaje", texto);
            return vista;
        }
    }
}

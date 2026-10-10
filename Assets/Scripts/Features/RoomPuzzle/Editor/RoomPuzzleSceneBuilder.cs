using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UniversalPlatform.Features.Progress.UI.ChapterTimer;
using UniversalPlatform.Features.RoomPuzzle.Data;
using UniversalPlatform.Features.RoomPuzzle.Domain;
using UniversalPlatform.Features.RoomPuzzle.DI;
using UniversalPlatform.Features.RoomPuzzle.UI.Room;
using UniversalPlatform.Shared;
using UniversalPlatform.Shared.Editor;

namespace UniversalPlatform.Features.RoomPuzzle.Editor
{
    /// <summary>
    /// Arma la escena de un aposento. Solo crea lo fijo (fondo, HUD, velo de la narradora y la
    /// lógica); las baldosas, los invitados, las llaves y la puerta los crea RoomView al iniciar, a
    /// partir de la definición de la sala. Uso: abre una escena vacía, ejecuta
    /// UniversalPlatform > Capítulos > Construir aposento azul y guárdala como "Chapter1".
    /// </summary>
    public static class RoomPuzzleSceneBuilder
    {
        private const string Personajes = "Assets/Art/Characters";
        private const string Objetos = "Assets/Art/Props";

        /// <summary>Lo que cambia de una sala a otra en la escena.</summary>
        private sealed class ConfigSala
        {
            public string IdCapitulo, NombreEscena, RutaFondo, RutaBaldosa, RutaCinematica;
            public string RutaPuertaCerrada = Objetos + "/puerta_cerrada.png", RutaPuertaAbierta = Objetos + "/puerta_abierta.png";
            /// <summary>La entrada es el reloj de ébano (si no, una puerta).</summary>
            public bool ConReloj;
            /// <summary>null = los colores por defecto de RoomView (los del aposento azul).</summary>
            public Color[] ColoresSuelo;      // baldosa A, baldosa B, pisada, trazo
        }

        [MenuItem("UniversalPlatform/Capítulos/Construir aposento negro (Chapter1)")]
        public static void ConstruirAposentoNegro()
        {
            Construir(new ConfigSala
            {
                IdCapitulo = "chapter_01",
                NombreEscena = "Chapter1",
                RutaFondo = "Assets/Art/Rooms/Black/sala_negra_fondo.png",
                RutaBaldosa = "Assets/Art/Rooms/Blue/baldosa.png",     // la baldosa es blanca y se tiñe
                RutaCinematica = "Assets/Video/cinematica_capitulo1.mp4",
                RutaPuertaCerrada = "Assets/Art/Rooms/Black/puerta_cerrada.png",
                RutaPuertaAbierta = "Assets/Art/Rooms/Black/puerta_abierta.png",
                ConReloj = true,
                ColoresSuelo = new[]
                {
                    new Color(0.22f, 0.07f, 0.09f, 1f), new Color(0.15f, 0.04f, 0.06f, 1f),
                    new Color(0.32f, 0.10f, 0.18f, 1f), new Color(1f, 0.18f, 0.20f, 1f)
                }
            });
        }

        [MenuItem("UniversalPlatform/Capítulos/Construir aposento azul (guardado, sin capítulo)")]
        public static void ConstruirAposentoAzul()
        {
            Construir(new ConfigSala
            {
                IdCapitulo = SalasDataRepository.ID_SALA_AZUL,
                NombreEscena = "AposentoAzul",
                RutaFondo = "Assets/Art/Rooms/Blue/sala_azul_fondo.png",
                RutaBaldosa = "Assets/Art/Rooms/Blue/baldosa.png",
            });
        }

        /// <summary>Todo el arte de las salas es pixel art: se importa sin suavizado.</summary>
        private static Sprite Pixel(string ruta) => SceneBuilderUtils.CargarSprite(ruta, true);

        private static List<Sprite> CuadrosDeCaminata(string direccion)
        {
            var cuadros = new List<Sprite>();
            for (int i = 0; i < 4; i++)
            {
                var sprite = Pixel($"{Personajes}/Encapuchado/encapuchado_{direccion}_{i}.png");
                if (sprite != null) cuadros.Add(sprite);
            }
            return cuadros;
        }

        private static void Construir(ConfigSala config)
        {
            string idCapitulo = config.IdCapitulo, rutaFondo = config.RutaFondo, rutaBaldosa = config.RutaBaldosa;
            string nombreEscena = config.NombreEscena, rutaCinematica = config.RutaCinematica;

            // Cámara: el Canvas es Overlay y no la necesita para dibujarse, pero sin ninguna cámara
            // la ventana Game avisa "No cameras rendering".
            var camaraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camaraGo.tag = "MainCamera";
            camaraGo.transform.position = new Vector3(0f, 0f, -10f);
            var camara = camaraGo.GetComponent<Camera>();
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = Color.black;

            var canvas = SceneBuilderUtils.CrearCanvas("Canvas");
            var raiz = canvas.transform;

            // ---------- Escenario: fondo + tablero (se desliza a la derecha en el final de la demo) ----------
            var escenario = SceneBuilderUtils.CrearRect("Escenario", raiz);
            SceneBuilderUtils.Estirar(escenario);

            // Mundo: fondo + tablero, del ancho de la sala. La cámara de RoomView lo desliza de lado
            // para seguir al protagonista (un fondo de 960 px de ancho = 2 pantallas).
            var spriteFondo = Pixel(rutaFondo);
            float anchoMundo = spriteFondo != null ? 1080f * spriteFondo.rect.width / spriteFondo.rect.height : 1920f;
            var mundo = SceneBuilderUtils.CrearRect("Mundo", escenario);
            SceneBuilderUtils.Colocar(mundo, Vector2.zero, new Vector2(anchoMundo, 1080f));

            var fondo = SceneBuilderUtils.CrearImage("Background", mundo,
                spriteFondo != null ? Color.white : new Color(0.04f, 0.08f, 0.22f, 1f), spriteFondo);
            SceneBuilderUtils.Estirar(fondo.rectTransform);

            // ---------- Tablero (RoomView lo llena al iniciar) ----------
            var tablero = SceneBuilderUtils.CrearRect("Tablero", mundo);
            SceneBuilderUtils.Estirar(tablero);

            // La habitación siguiente, en negro: RoomView la pone a la izquierda al cruzar la puerta.
            var salaNegra = SceneBuilderUtils.CrearImage("SalaNegra", escenario, Color.black);
            SceneBuilderUtils.Estirar(salaNegra.rectTransform);
            salaNegra.raycastTarget = false;
            salaNegra.gameObject.SetActive(false);

            var polvo = SceneBuilderUtils.CrearRect("PolvoAmbiental", raiz);
            SceneBuilderUtils.Estirar(polvo);
            polvo.gameObject.AddComponent<DustMotesView>();

            // ---------- HUD ----------
            var hud = SceneBuilderUtils.CrearRect("HUD", raiz);
            SceneBuilderUtils.Estirar(hud);
            var grupoHud = hud.gameObject.AddComponent<CanvasGroup>();
            grupoHud.blocksRaycasts = false;
            grupoHud.interactable = false;

            var nombre = SceneBuilderUtils.CrearTexto("NombreSala", hud, "Aposento", 40f, Color.white, TextAlignmentOptions.Left);
            SceneBuilderUtils.Colocar(nombre.rectTransform, new Vector2(-600f, 486f), new Vector2(640f, 60f));

            var spriteLlave = Pixel($"{Objetos}/llave.png");
            var iconos = new List<Image>();
            for (int i = 0; i < 3; i++)
            {
                var icono = SceneBuilderUtils.CrearImage($"Llave_{i + 1}", hud, new Color(1f, 1f, 1f, 0.22f), spriteLlave);
                SceneBuilderUtils.Colocar(icono.rectTransform, new Vector2(-884f + i * 74f, 418f), new Vector2(68f, 68f));
                iconos.Add(icono);
            }

            var aviso = SceneBuilderUtils.CrearTexto("Aviso", hud, "", 26f, new Color(1f, 1f, 1f, 0.75f));
            SceneBuilderUtils.Colocar(aviso.rectTransform, new Vector2(0f, -508f), new Vector2(1700f, 44f));

            // Cronómetro del capítulo (solo corre mientras se juega), con su ícono pixel art.
            var spriteCronometro = Pixel($"{Objetos}/cronometro.png");
            if (spriteCronometro != null)
            {
                var iconoTiempo = SceneBuilderUtils.CrearImage("IconoCronometro", hud, Color.white, spriteCronometro);
                SceneBuilderUtils.Colocar(iconoTiempo.rectTransform, new Vector2(690f, 486f), new Vector2(64f, 70f));
                iconoTiempo.preserveAspect = true;
            }
            var tiempo = SceneBuilderUtils.CrearTexto("Tiempo", hud, "00:00:00", 40f, Color.white, TextAlignmentOptions.Left);
            SceneBuilderUtils.Colocar(tiempo.rectTransform, new Vector2(840f, 486f), new Vector2(220f, 60f));

            // ---------- Velo de la narradora (encima de todo) ----------
            var velo = SceneBuilderUtils.CrearImage("VeloNarradora", raiz, Color.black, null, true);
            SceneBuilderUtils.Estirar(velo.rectTransform);
            var grupoVelo = velo.gameObject.AddComponent<CanvasGroup>();
            grupoVelo.alpha = 1f;
            var clicVelo = velo.gameObject.AddComponent<ClicView>();
            var narradora = SceneBuilderUtils.CrearTexto("Texto", velo.transform, "", 46f, Color.white);
            SceneBuilderUtils.Colocar(narradora.rectTransform, Vector2.zero, new Vector2(1400f, 400f));
            var continuar = SceneBuilderUtils.CrearTexto("Continuar", velo.transform, "Espacio o clic para continuar", 24f, new Color(1f, 1f, 1f, 0.45f));
            SceneBuilderUtils.Colocar(continuar.rectTransform, new Vector2(0f, -440f), new Vector2(900f, 40f));

            // ---------- Final de la demo (aparece sobre la habitación negra) ----------
            var final = SceneBuilderUtils.CrearImage("FinalDemo", raiz, new Color(0f, 0f, 0f, 0f), null, true);
            SceneBuilderUtils.Estirar(final.rectTransform);
            var grupoFinal = final.gameObject.AddComponent<CanvasGroup>();
            grupoFinal.alpha = 0f;
            grupoFinal.blocksRaycasts = false;
            var clicFinal = final.gameObject.AddComponent<ClicView>();
            var gracias = SceneBuilderUtils.CrearTexto("Gracias", final.transform, "Gracias por jugar nuestra demo", 80f, Color.white);
            SceneBuilderUtils.Colocar(gracias.rectTransform, new Vector2(0f, 60f), new Vector2(1700f, 120f));
            var creditos = SceneBuilderUtils.CrearTexto("Creditos", final.transform, "Alondra González   ·   Andrea Díaz", 32f,
                new Color(1f, 1f, 1f, 0.8f));
            SceneBuilderUtils.Colocar(creditos.rectTransform, new Vector2(0f, -40f), new Vector2(1200f, 50f));
            var volverMenu = SceneBuilderUtils.CrearTexto("Volver", final.transform, "Espacio o clic para volver al menú", 22f,
                new Color(1f, 1f, 1f, 0.4f));
            SceneBuilderUtils.Colocar(volverMenu.rectTransform, new Vector2(0f, -440f), new Vector2(900f, 40f));

            // ---------- Cinemática (encima de todo; se ve antes de los textos y del puzle) ----------
            CinematicaView cinematica = null;
            var clip = string.IsNullOrEmpty(rutaCinematica) ? null : AssetDatabase.LoadAssetAtPath<VideoClip>(rutaCinematica);
            if (clip != null)
            {
                var telon = SceneBuilderUtils.CrearImage("Cinematica", raiz, Color.black, null, true);
                SceneBuilderUtils.Estirar(telon.rectTransform);
                telon.gameObject.AddComponent<CanvasGroup>();
                var pantalla = SceneBuilderUtils.CrearRect("Video", telon.transform).gameObject.AddComponent<RawImage>();
                pantalla.raycastTarget = false;
                SceneBuilderUtils.Estirar(pantalla.rectTransform);
                var ajuste = pantalla.gameObject.AddComponent<AspectRatioFitter>();
                ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                ajuste.aspectRatio = clip.height > 0 ? (float)clip.width / clip.height : 4f / 3f;
                var ayuda = SceneBuilderUtils.CrearTexto("Ayuda", telon.transform, "Espacio: siguiente   ·   Esc: saltar", 24f,
                    new Color(1f, 1f, 1f, 0.45f), TextAlignmentOptions.Right);
                SceneBuilderUtils.Colocar(ayuda.rectTransform, new Vector2(620f, -505f), new Vector2(620f, 40f));

                cinematica = telon.gameObject.AddComponent<CinematicaView>();
                SceneBuilderUtils.Asignar(cinematica, "_clip", clip);
                SceneBuilderUtils.Asignar(cinematica, "_pantalla", pantalla);
                SceneBuilderUtils.Asignar(cinematica, "_ajuste", ajuste);
            }
            else if (!string.IsNullOrEmpty(rutaCinematica))
            {
                Debug.LogWarning($"[RoomPuzzleSceneBuilder] No se encontró el video '{rutaCinematica}'; la escena queda sin cinemática.");
            }

            // ---------- Vista ----------
            var vistaGo = SceneBuilderUtils.CrearRect("RoomView", raiz).gameObject;
            var vista = vistaGo.AddComponent<RoomView>();
            SceneBuilderUtils.Asignar(vista, "_tablero", tablero);
            SceneBuilderUtils.Asignar(vista, "_spriteBaldosa", Pixel(rutaBaldosa));
            SceneBuilderUtils.Asignar(vista, "_spriteSombra", Pixel($"{Objetos}/sombra.png"));

            // Protagonista: 4 cuadros de caminata por dirección.
            var sur = CuadrosDeCaminata("sur");
            SceneBuilderUtils.AsignarLista(vista, "_caminarSur", sur);
            SceneBuilderUtils.AsignarLista(vista, "_caminarNorte", CuadrosDeCaminata("norte"));
            SceneBuilderUtils.AsignarLista(vista, "_caminarEste", CuadrosDeCaminata("este"));
            SceneBuilderUtils.AsignarLista(vista, "_caminarOeste", CuadrosDeCaminata("oeste"));
            SceneBuilderUtils.Asignar(vista, "_spriteProtagonista", sur.Count > 0 ? sur[0] : null);

            // Invitados: 3 máscaras x 5 colores; el índice es el "aspecto" de SalasDataRepository.
            var invitados = new List<Sprite>();
            for (int i = 0; i < 15; i++)
            {
                var sprite = Pixel($"{Personajes}/Invitados/invitado_{i:00}.png");
                if (sprite == null) break;
                invitados.Add(sprite);
            }
            SceneBuilderUtils.AsignarLista(vista, "_spritesInvitados", invitados);
            SceneBuilderUtils.Asignar(vista, "_spriteLlave", spriteLlave);
            SceneBuilderUtils.Asignar(vista, "_spritePuertaCerrada", Pixel(config.RutaPuertaCerrada));
            SceneBuilderUtils.Asignar(vista, "_spritePuertaAbierta", Pixel(config.RutaPuertaAbierta));

            // Cámara.
            SceneBuilderUtils.Asignar(vista, "_mundo", mundo);
            SceneBuilderUtils.AsignarFloat(vista, "_anchoMundo", anchoMundo);

            // Reloj de ébano en la entrada: 4 cuadros de péndulo + abierto (40 x 92 px, a 3x).
            if (config.ConReloj)
            {
                var cuadros = new List<Sprite>();
                for (int i = 0; i < 4; i++)
                {
                    var c = Pixel($"{Objetos}/reloj_ebano_{i}.png");
                    if (c != null) cuadros.Add(c);
                }
                SceneBuilderUtils.AsignarLista(vista, "_cuadrosEntrada", cuadros);
                SceneBuilderUtils.Asignar(vista, "_spriteEntradaAbierta", Pixel($"{Objetos}/reloj_ebano_abierto.png"));
                AsignarVector2(vista, "_tamanoEntrada", new Vector2(120f, 276f));
            }

            // Muebles (pixel art a 3x, como los personajes).
            AsignarMuebles(vista, new[]
            {
                new ArteMueble(TipoMueble.Libreria, "libreria", new Vector2(288f, 210f)),
                new ArteMueble(TipoMueble.Sillon, "sillon", new Vector2(288f, 138f)),
                new ArteMueble(TipoMueble.Planta, "planta", new Vector2(96f, 162f)),
                new ArteMueble(TipoMueble.Mesita, "mesita", new Vector2(102f, 114f)),
                new ArteMueble(TipoMueble.Candelabro, "candelabro", new Vector2(84f, 204f)),
            });

            if (config.ColoresSuelo != null && config.ColoresSuelo.Length == 4)
            {
                SceneBuilderUtils.AsignarColor(vista, "_colorBaldosaA", config.ColoresSuelo[0]);
                SceneBuilderUtils.AsignarColor(vista, "_colorBaldosaB", config.ColoresSuelo[1]);
                SceneBuilderUtils.AsignarColor(vista, "_colorBaldosaPisada", config.ColoresSuelo[2]);
                SceneBuilderUtils.AsignarColor(vista, "_colorTrazo", config.ColoresSuelo[3]);
            }
            SceneBuilderUtils.Asignar(vista, "_spriteGlobo", Pixel($"{Objetos}/globo.png"));
            SceneBuilderUtils.Asignar(vista, "_textoNombre", nombre);
            SceneBuilderUtils.AsignarLista(vista, "_iconosLlaves", iconos);
            SceneBuilderUtils.Asignar(vista, "_textoAviso", aviso);
            SceneBuilderUtils.Asignar(vista, "_velo", grupoVelo);
            SceneBuilderUtils.Asignar(vista, "_textoNarradora", narradora);
            SceneBuilderUtils.Asignar(vista, "_clicVelo", clicVelo);
            if (cinematica != null) SceneBuilderUtils.Asignar(vista, "_cinematica", cinematica);
            SceneBuilderUtils.Asignar(vista, "_escenario", escenario);
            SceneBuilderUtils.Asignar(vista, "_salaNegra", salaNegra.rectTransform);
            SceneBuilderUtils.Asignar(vista, "_hud", grupoHud);
            SceneBuilderUtils.Asignar(vista, "_finalDemo", grupoFinal);
            SceneBuilderUtils.Asignar(vista, "_clicFinal", clicFinal);

            // ---------- Lógica: cronómetro del capítulo + installer ----------
            var logica = new GameObject("RoomLogic");
            var cronometro = logica.AddComponent<ChapterTimerView>();
            SceneBuilderUtils.AsignarString(cronometro, "_idCapitulo", idCapitulo);
            SceneBuilderUtils.Asignar(cronometro, "_textoTiempo", tiempo);
            var installer = logica.AddComponent<RoomPuzzleInstaller>();
            SceneBuilderUtils.Asignar(installer, "_view", vista);
            SceneBuilderUtils.Asignar(installer, "_cronometro", cronometro);

            Selection.activeGameObject = canvas.gameObject;
            Debug.Log($"[RoomPuzzleSceneBuilder] Escena lista. Guárdala como '{nombreEscena}' (File > Save As, reemplazando la que hay en Assets/Scenes).");
        }

        private static void AsignarVector2(Object objetivo, string campo, Vector2 valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[RoomPuzzleSceneBuilder] No existe el campo '{campo}'."); return; }
            prop.vector2Value = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Llena RoomView._spritesMuebles con el dibujo y el tamaño de cada tipo de mueble.</summary>
        private struct ArteMueble
        {
            public readonly TipoMueble Tipo; public readonly string Archivo; public readonly Vector2 Tamano;
            public ArteMueble(TipoMueble tipo, string archivo, Vector2 tamano) { Tipo = tipo; Archivo = archivo; Tamano = tamano; }
        }

        private static void AsignarMuebles(RoomView vista, ArteMueble[] muebles)
        {
            var so = new SerializedObject(vista);
            var lista = so.FindProperty("_spritesMuebles");
            lista.arraySize = muebles.Length;
            for (int i = 0; i < muebles.Length; i++)
            {
                var elemento = lista.GetArrayElementAtIndex(i);
                elemento.FindPropertyRelative("tipo").enumValueIndex = (int)muebles[i].Tipo;
                elemento.FindPropertyRelative("sprite").objectReferenceValue = Pixel($"{Objetos}/{muebles[i].Archivo}.png");
                elemento.FindPropertyRelative("tamano").vector2Value = muebles[i].Tamano;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

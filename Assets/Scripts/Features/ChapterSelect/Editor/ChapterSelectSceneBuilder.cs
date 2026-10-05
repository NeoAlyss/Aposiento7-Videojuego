using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniversalPlatform.Features.ChapterSelect.DI;
using UniversalPlatform.Features.ChapterSelect.UI.Clock;
using UniversalPlatform.Features.ChapterSelect.UI.Info;
using UniversalPlatform.Features.ChapterSelect.UI.Transition;
using UniversalPlatform.Shared.Editor;

namespace UniversalPlatform.Features.ChapterSelect.Editor
{
    /// <summary>
    /// Arma la escena del reloj de capítulos. Usa el arte de <c>Assets/Art/Clock</c> (esfera,
    /// numerales romanos y manillas); si falta algún PNG, cae al arte provisional (círculo,
    /// rectángulos y texto) y avisa por consola. Uso: abre una escena vacía y ejecuta
    /// UniversalPlatform > ChapterSelect > Construir escena.
    /// </summary>
    public static class ChapterSelectSceneBuilder
    {
        private static readonly Color Crema = new Color(0.93f, 0.87f, 0.72f, 1f);
        private static readonly Color Oscuro = new Color(0.12f, 0.08f, 0.06f, 1f);

        // Arte del reloj. Los PNG están dibujados sobre un lienzo de 1024 px con el eje en el centro,
        // así que esfera y manillas comparten tamaño y quedan alineadas sin ajustes.
        private const string CarpetaArte = "Assets/Art/Clock";
        private const float TamanoReloj = 980f;
        private const float Escala = TamanoReloj / 1024f;
        private const float RadioNumerales = 405f * Escala;   // centro de la banda de números del dibujo

        // El reloj no lleva relleno: líneas y manillas blancas sobre la noche, con un halo suave.
        // Los PNG son blancos, así que estos colores se pueden cambiar en el inspector (Image > Color).
        private static readonly Color Linea = Color.white;
        private static readonly Color Brillo = new Color(0.75f, 0.85f, 1f, 0.8f);

        // Colores de los numerales.
        private static readonly Color NumeroNormal = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color NumeroIluminado = new Color(1f, 0.86f, 0.55f, 1f);
        private static readonly Color NumeroBloqueado = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color NumeroBloqueadoIluminado = new Color(1f, 1f, 1f, 0.55f);

        [MenuItem("UniversalPlatform/ChapterSelect/Construir escena")]
        public static void Construir()
        {
            var canvas = SceneBuilderUtils.CrearCanvas("Canvas");
            var raiz = canvas.transform;

            SceneBuilderUtils.CrearAmbienteNocturno(raiz);

            // ---------- Reloj ----------
            var reloj = SceneBuilderUtils.CrearRect("ClockGroup", raiz);
            SceneBuilderUtils.Estirar(reloj);
            var grupoReloj = reloj.gameObject.AddComponent<CanvasGroup>();

            var tamanoReloj = new Vector2(TamanoReloj, TamanoReloj);
            var spriteLineas = CargarSprite("esfera_reloj");
            if (spriteLineas != null)
            {
                CrearCapa("ClockFaceGlow", reloj, CargarSprite("esfera_reloj_brillo"), Brillo, tamanoReloj);
                CrearCapa("ClockFace", reloj, spriteLineas, Linea, tamanoReloj);
            }
            else
            {
                var esfera = SceneBuilderUtils.CrearImage("ClockFace", reloj, Crema, SceneBuilderUtils.SpriteCirculo());
                SceneBuilderUtils.Colocar(esfera.rectTransform, Vector2.zero, tamanoReloj);
            }

            var numerosRaiz = SceneBuilderUtils.CrearRect("Numbers", reloj);
            SceneBuilderUtils.Colocar(numerosRaiz, Vector2.zero, new Vector2(10f, 10f));
            var numeros = new List<ClockNumberView>();
            for (int n = 1; n <= 12; n++)
            {
                var rt = SceneBuilderUtils.CrearRect($"Numero_{n}", numerosRaiz);
                SceneBuilderUtils.Colocar(rt, Vector2.zero, new Vector2(120f, 120f));
                var vistaNumero = rt.gameObject.AddComponent<ClockNumberView>();

                Graphic grafico;
                var spriteNumero = CargarSprite($"numero_{n:00}");
                if (spriteNumero != null)
                {
                    // Numeral romano dibujado: va girado siguiendo la esfera, como en el diseño.
                    var numeral = SceneBuilderUtils.CrearImage("Numeral", rt, NumeroNormal, spriteNumero);
                    SceneBuilderUtils.Colocar(numeral.rectTransform, Vector2.zero,
                        new Vector2(160f, 100f) * Escala);
                    rt.localRotation = Quaternion.Euler(0f, 0f, -30f * n);
                    AsignarColor(vistaNumero, "_colorNormal", NumeroNormal);
                    AsignarColor(vistaNumero, "_colorIluminado", NumeroIluminado);
                    AsignarColor(vistaNumero, "_colorBloqueado", NumeroBloqueado);
                    AsignarColor(vistaNumero, "_colorBloqueadoIluminado", NumeroBloqueadoIluminado);
                    grafico = numeral;
                }
                else
                {
                    var texto = SceneBuilderUtils.CrearTexto("Texto", rt, n.ToString(), 78f, Oscuro);
                    SceneBuilderUtils.Estirar(texto.rectTransform);
                    grafico = texto;
                }

                SceneBuilderUtils.AsignarLista(vistaNumero, "_elementos", new List<Graphic> { grafico });
                numeros.Add(vistaNumero);
            }

            var spriteHoraria = CargarSprite("manilla_horario");
            var spriteMinutero = CargarSprite("manilla_minutero");
            bool manillasDibujadas = spriteHoraria != null && spriteMinutero != null;

            RectTransform horaria, minutero;
            if (manillasDibujadas)
            {
                horaria = CrearManillaDibujada(reloj, "ManillaHoraria", spriteHoraria, CargarSprite("manilla_horario_brillo"));
                minutero = CrearManillaDibujada(reloj, "ManillaMinutero", spriteMinutero, CargarSprite("manilla_minutero_brillo"));
            }
            else
            {
                horaria = CrearManilla(reloj, "ManillaHoraria", new Vector2(26f, 300f));
                minutero = CrearManilla(reloj, "ManillaMinutero", new Vector2(16f, 420f));
                var centro = SceneBuilderUtils.CrearImage("Centro", reloj, Oscuro, SceneBuilderUtils.SpriteCirculo());
                SceneBuilderUtils.Colocar(centro.rectTransform, Vector2.zero, new Vector2(60f, 60f));
            }

            // ---------- Boxes de información ----------
            var panelInfo = CrearPanelInfo(reloj);

            // ---------- Capa de la puerta (desactivada al inicio, encima del reloj) ----------
            var capa = SceneBuilderUtils.CrearRect("DoorLayer", raiz);
            SceneBuilderUtils.Estirar(capa);

            var resplandor = SceneBuilderUtils.CrearImage("PortalGlow", capa, SceneBuilderUtils.Dorado, SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(resplandor.rectTransform, Vector2.zero, new Vector2(1400f, 1400f));
            var cr = resplandor.color; cr.a = 0f; resplandor.color = cr;

            var portal = SceneBuilderUtils.CrearImage("Portal", capa, Color.white, SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(portal.rectTransform, Vector2.zero, new Vector2(700f, 700f));
            portal.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var arte = SceneBuilderUtils.CrearImage("Arte", portal.transform, new Color(0.05f, 0.09f, 0.30f, 1f));
            SceneBuilderUtils.Estirar(arte.rectTransform);

            var puertaIzq = CrearPuerta(capa, "PuertaIzquierda", true);
            var puertaDer = CrearPuerta(capa, "PuertaDerecha", false);

            var fundido = SceneBuilderUtils.CrearImage("FundidoFinal", capa, Color.black);
            SceneBuilderUtils.Estirar(fundido.rectTransform);
            var grupoFundido = fundido.gameObject.AddComponent<CanvasGroup>();
            grupoFundido.alpha = 0f;
            grupoFundido.blocksRaycasts = false;
            grupoFundido.interactable = false;

            capa.gameObject.SetActive(false);

            // ---------- Lógica ----------
            var logica = new GameObject("ChapterSelectLogic");
            var transicion = logica.AddComponent<ClockDoorTransitionView>();
            SceneBuilderUtils.Asignar(transicion, "_grupoReloj", grupoReloj);
            SceneBuilderUtils.Asignar(transicion, "_capaRaiz", capa.gameObject);
            SceneBuilderUtils.Asignar(transicion, "_puertaIzquierda", puertaIzq);
            SceneBuilderUtils.Asignar(transicion, "_puertaDerecha", puertaDer);
            SceneBuilderUtils.Asignar(transicion, "_portal", portal.rectTransform);
            SceneBuilderUtils.Asignar(transicion, "_imagenPortal", arte);
            SceneBuilderUtils.Asignar(transicion, "_resplandorPortal", resplandor);
            SceneBuilderUtils.Asignar(transicion, "_fundidoFinal", grupoFundido);

            var vistaGo = SceneBuilderUtils.CrearRect("ChapterSelectView", raiz).gameObject;
            var vista = vistaGo.AddComponent<ChapterSelectView>();
            SceneBuilderUtils.AsignarLista(vista, "_numeros", numeros);
            if (spriteLineas != null) SceneBuilderUtils.AsignarFloat(vista, "_radioNumeros", RadioNumerales);
            SceneBuilderUtils.Asignar(vista, "_manillaHoraria", horaria);
            SceneBuilderUtils.Asignar(vista, "_manillaMinutero", minutero);
            SceneBuilderUtils.Asignar(vista, "_panelInfo", panelInfo);
            SceneBuilderUtils.Asignar(vista, "_transicionPuerta", transicion);

            var installer = logica.AddComponent<ChapterSelectInstaller>();
            SceneBuilderUtils.Asignar(installer, "_view", vista);

            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[ChapterSelectSceneBuilder] Escena lista. Guárdala como 'ChapterSelect' y agrégala a Build Settings junto con 'MainMenu' y 'Chapter1'.");
        }

        private static RectTransform CrearManilla(Transform padre, string nombre, Vector2 tamano)
        {
            var img = SceneBuilderUtils.CrearImage(nombre, padre, Oscuro);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);            // gira desde la base
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = tamano;
            return rt;
        }

        /// <summary>
        /// Manilla con arte propio: el dibujo ocupa el mismo lienzo que la esfera y gira sobre su
        /// centro (trae contrapeso bajo el eje), por eso el pivot va en (0.5, 0.5). El objeto que
        /// gira es un contenedor con dos hijos: el halo ("Brillo") y el trazo ("Dibujo").
        /// </summary>
        private static RectTransform CrearManillaDibujada(Transform padre, string nombre, Sprite sprite, Sprite spriteBrillo)
        {
            var tamano = new Vector2(TamanoReloj, TamanoReloj);
            var rt = SceneBuilderUtils.CrearRect(nombre, padre);
            SceneBuilderUtils.Colocar(rt, Vector2.zero, tamano);
            rt.pivot = new Vector2(0.5f, 0.5f);
            CrearCapa("Brillo", rt, spriteBrillo, Brillo, tamano);
            CrearCapa("Dibujo", rt, sprite, Linea, tamano);
            return rt;
        }

        /// <summary>Imagen centrada del tamaño indicado; no crea nada si el sprite no existe.</summary>
        private static void CrearCapa(string nombre, Transform padre, Sprite sprite, Color color, Vector2 tamano)
        {
            if (sprite == null) return;
            var img = SceneBuilderUtils.CrearImage(nombre, padre, color, sprite);
            SceneBuilderUtils.Colocar(img.rectTransform, Vector2.zero, tamano);
        }

        /// <summary>
        /// Carga un PNG de <see cref="CarpetaArte"/> como Sprite. Si Unity lo importó como textura
        /// normal (o como sprite múltiple), lo reimporta como sprite único. Devuelve null si no existe.
        /// </summary>
        private static Sprite CargarSprite(string nombre)
        {
            string ruta = $"{CarpetaArte}/{nombre}.png";
            var importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[ChapterSelectSceneBuilder] No se encontró '{ruta}'; se usa arte provisional en su lugar.");
                return null;
            }

            if (importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || importer.mipmapEnabled)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }

        private static void AsignarColor(Object objetivo, string campo, Color valor)
        {
            var so = new SerializedObject(objetivo);
            var prop = so.FindProperty(campo);
            if (prop == null) { Debug.LogError($"[ChapterSelectSceneBuilder] No existe el campo '{campo}' en {objetivo.GetType().Name}"); return; }
            prop.colorValue = valor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RawImage CrearPuerta(Transform padre, string nombre, bool izquierda)
        {
            var rt = SceneBuilderUtils.CrearRect(nombre, padre);
            rt.anchorMin = izquierda ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
            rt.anchorMax = izquierda ? new Vector2(0.5f, 1f) : new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = izquierda ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);   // bisagra en el borde exterior
            var raw = rt.gameObject.AddComponent<RawImage>();
            raw.color = Color.black;
            raw.raycastTarget = false;
            return raw;
        }

        private static ChapterInfoPanelView CrearPanelInfo(Transform padre)
        {
            var panel = SceneBuilderUtils.CrearRect("InfoPanel", padre);
            SceneBuilderUtils.Colocar(panel, new Vector2(640f, 0f), new Vector2(460f, 520f));
            var vista = panel.gameObject.AddComponent<ChapterInfoPanelView>();

            string[] campos = { "Titulo", "Dificultad", "Llaves", "Tiempo" };
            float[] ys = { 195f, 65f, -65f, -195f };
            var cajas = new CanvasGroup[4];
            var textos = new TextMeshProUGUI[4];
            for (int i = 0; i < 4; i++)
            {
                var caja = SceneBuilderUtils.CrearImage($"Caja_{campos[i]}", panel, new Color(0.03f, 0.05f, 0.16f, 0.78f));
                SceneBuilderUtils.Colocar(caja.rectTransform, new Vector2(0f, ys[i]), new Vector2(460f, 110f));
                cajas[i] = caja.gameObject.AddComponent<CanvasGroup>();
                textos[i] = SceneBuilderUtils.CrearTexto("Texto", caja.transform, campos[i], 44f, SceneBuilderUtils.Dorado);
                SceneBuilderUtils.Estirar(textos[i].rectTransform);
            }

            SceneBuilderUtils.Asignar(vista, "_cajaTitulo", cajas[0]);
            SceneBuilderUtils.Asignar(vista, "_cajaDificultad", cajas[1]);
            SceneBuilderUtils.Asignar(vista, "_cajaLlaves", cajas[2]);
            SceneBuilderUtils.Asignar(vista, "_cajaTiempo", cajas[3]);
            SceneBuilderUtils.Asignar(vista, "_textoTitulo", textos[0]);
            SceneBuilderUtils.Asignar(vista, "_textoDificultad", textos[1]);
            SceneBuilderUtils.Asignar(vista, "_textoLlaves", textos[2]);
            SceneBuilderUtils.Asignar(vista, "_textoTiempo", textos[3]);
            SceneBuilderUtils.Asignar(vista, "_raizPanel", panel);
            return vista;
        }
    }
}

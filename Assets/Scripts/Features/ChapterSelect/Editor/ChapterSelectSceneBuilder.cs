using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniversalPlatform.Features.ChapterSelect.DI;
using UniversalPlatform.Features.ChapterSelect.UI.Clock;
using UniversalPlatform.Features.ChapterSelect.UI.Info;
using UniversalPlatform.Features.ChapterSelect.UI.Transition;
using UniversalPlatform.Shared;
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

        // El reloj no lleva relleno: líneas y manillas blancas sobre el fondo oscuro, con un halo suave.
        // Los PNG son blancos, así que estos colores se pueden cambiar en el inspector (Image > Color).
        private static readonly Color Linea = Color.white;
        private static readonly Color Brillo = new Color(1f, 0.95f, 0.9f, 0.8f);

        // Colores de los numerales.
        // El seleccionado va en blanco pleno (y ClockNumberView lo agranda con _escalaIluminado);
        // el resto queda con algo de transparencia para que el seleccionado destaque.
        private static readonly Color NumeroNormal = new Color(1f, 1f, 1f, 0.65f);
        private static readonly Color NumeroIluminado = Color.white;
        private static readonly Color NumeroBloqueado = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color NumeroBloqueadoIluminado = new Color(1f, 1f, 1f, 0.55f);

        [MenuItem("UniversalPlatform/ChapterSelect/Construir escena")]
        public static void Construir()
        {
            // Canvas con cámara en perspectiva: hace falta para que la puerta del reloj gire en 3D.
            var canvas = SceneBuilderUtils.CrearCanvasConCamara("Canvas");
            var raiz = canvas.transform;

            // Fondo más oscuro que en el menú: detrás van las líneas finas del reloj.
            SceneBuilderUtils.CrearAmbienteNocturno(raiz, 0.42f);

            var tamanoReloj = new Vector2(TamanoReloj, TamanoReloj);

            // ---------- Escenario: lo que se agranda al entrar (interior + puerta) ----------
            var escenario = SceneBuilderUtils.CrearRect("Escenario", raiz);
            SceneBuilderUtils.Estirar(escenario);

            // Interior del capítulo: queda detrás del reloj y solo aparece cuando la puerta se abre.
            var spriteHalo = SceneBuilderUtils.CargarSprite("Assets/Art/Candle/vela_halo.png");
            var spriteDisco = CargarSprite("portal_disco");

            var resplandor = SceneBuilderUtils.CrearImage("PortalGlow", escenario, SceneBuilderUtils.Dorado,
                spriteHalo != null ? spriteHalo : SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(resplandor.rectTransform, Vector2.zero, tamanoReloj * 1.7f);
            var cr = resplandor.color; cr.a = 0f; resplandor.color = cr;

            var portal = SceneBuilderUtils.CrearImage("Portal", escenario, Color.white,
                spriteDisco != null ? spriteDisco : SceneBuilderUtils.SpriteCirculo());
            SceneBuilderUtils.Colocar(portal.rectTransform, Vector2.zero, tamanoReloj * 0.96f);   // justo dentro del anillo exterior
            portal.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var grupoPortal = portal.gameObject.AddComponent<CanvasGroup>();
            grupoPortal.alpha = 0f;
            grupoPortal.blocksRaycasts = false;
            grupoPortal.interactable = false;
            // "Arte" es lo que se ve dentro: por defecto, penumbra con una luz cálida al fondo. Asigna
            // el arte de cada capítulo en ChapterSelectView > Visuales y lo reemplaza al entrar.
            var arte = SceneBuilderUtils.CrearImage("Arte", portal.transform, new Color(0.10f, 0.02f, 0.02f, 1f));
            SceneBuilderUtils.Estirar(arte.rectTransform);
            if (spriteHalo != null)
            {
                var luz = SceneBuilderUtils.CrearImage("Luz", portal.transform, new Color(1f, 0.78f, 0.45f, 0.85f), spriteHalo);
                SceneBuilderUtils.Colocar(luz.rectTransform, Vector2.zero, tamanoReloj * 1.1f);
            }

            // ---------- Reloj = puerta: gira sobre su borde izquierdo (pivot en la bisagra) ----------
            var reloj = SceneBuilderUtils.CrearRect("ClockGroup", escenario);
            reloj.pivot = new Vector2(0f, 0.5f);
            SceneBuilderUtils.Colocar(reloj, new Vector2(-TamanoReloj * 0.5f, 0f), tamanoReloj);
            var grupoReloj = reloj.gameObject.AddComponent<CanvasGroup>();

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

            // ---------- Boxes de información (fuera de la puerta: no giran con ella) ----------
            var panelInfo = CrearPanelInfo(raiz);
            var grupoInfo = panelInfo.gameObject.AddComponent<CanvasGroup>();

            // Volver: debajo del fundido, para que aparezca y desaparezca con el resto.
            var botonVolver = CrearBotonVolver(raiz);
            var grupoVolver = botonVolver.gameObject.AddComponent<CanvasGroup>();

            // ---------- Fundido final (encima de todo) ----------
            var fundido = SceneBuilderUtils.CrearImage("FundidoFinal", raiz, Color.black);
            SceneBuilderUtils.Estirar(fundido.rectTransform);
            var grupoFundido = fundido.gameObject.AddComponent<CanvasGroup>();
            grupoFundido.alpha = 0f;
            grupoFundido.blocksRaycasts = false;
            grupoFundido.interactable = false;

            // ---------- Lógica ----------
            var logica = new GameObject("ChapterSelectLogic");
            var transicion = logica.AddComponent<ClockDoorTransitionView>();
            SceneBuilderUtils.Asignar(transicion, "_puerta", reloj);
            SceneBuilderUtils.Asignar(transicion, "_grupoPuerta", grupoReloj);
            SceneBuilderUtils.Asignar(transicion, "_portal", portal.rectTransform);
            SceneBuilderUtils.Asignar(transicion, "_grupoPortal", grupoPortal);
            SceneBuilderUtils.Asignar(transicion, "_imagenPortal", arte);
            SceneBuilderUtils.Asignar(transicion, "_resplandorPortal", resplandor);
            SceneBuilderUtils.Asignar(transicion, "_escenario", escenario);
            SceneBuilderUtils.Asignar(transicion, "_grupoInfo", grupoInfo);
            SceneBuilderUtils.Asignar(transicion, "_grupoVolver", grupoVolver);
            SceneBuilderUtils.AsignarFloat(transicion, "_tiempoEntrada", 2.4f);
            SceneBuilderUtils.AsignarFloat(transicion, "_escalaInicialEntrada", 0.4f);
            SceneBuilderUtils.Asignar(transicion, "_fundidoFinal", grupoFundido);

            var vistaGo = SceneBuilderUtils.CrearRect("ChapterSelectView", raiz).gameObject;
            var vista = vistaGo.AddComponent<ChapterSelectView>();
            SceneBuilderUtils.AsignarLista(vista, "_numeros", numeros);
            if (spriteLineas != null) SceneBuilderUtils.AsignarFloat(vista, "_radioNumeros", RadioNumerales);
            SceneBuilderUtils.Asignar(vista, "_manillaHoraria", horaria);
            SceneBuilderUtils.Asignar(vista, "_manillaMinutero", minutero);
            SceneBuilderUtils.Asignar(vista, "_panelInfo", panelInfo);
            SceneBuilderUtils.Asignar(vista, "_transicionPuerta", transicion);
            SceneBuilderUtils.Asignar(vista, "_botonVolver", botonVolver);

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

        private static Sprite CargarSprite(string nombre) =>
            SceneBuilderUtils.CargarSprite($"{CarpetaArte}/{nombre}.png");

        private static void AsignarColor(Object objetivo, string campo, Color valor) =>
            SceneBuilderUtils.AsignarColor(objetivo, campo, valor);

        /// <summary>Botón "Volver" con la flecha de las manillas, abajo a la izquierda.</summary>
        private static BotonSimpleView CrearBotonVolver(Transform padre)
        {
            var rt = SceneBuilderUtils.CrearRect("BotonVolver", padre);
            SceneBuilderUtils.Colocar(rt, new Vector2(-760f, -470f), new Vector2(300f, 90f));
            var area = rt.gameObject.AddComponent<Image>();
            area.color = new Color(0, 0, 0, 0);
            area.raycastTarget = true;

            var graficos = new List<Graphic>();
            var flecha = SceneBuilderUtils.CargarSprite("Assets/Art/UI/flecha_volver.png");
            if (flecha != null)
            {
                var icono = SceneBuilderUtils.CrearImage("Flecha", rt, Color.white, flecha);
                SceneBuilderUtils.Colocar(icono.rectTransform, new Vector2(-80f, 0f), new Vector2(108f, 60f));
                graficos.Add(icono);
            }
            var texto = SceneBuilderUtils.CrearTexto("Texto", rt, "Volver", 40f, Color.white);
            SceneBuilderUtils.Colocar(texto.rectTransform, new Vector2(50f, 0f), new Vector2(180f, 70f));
            graficos.Add(texto);

            var boton = rt.gameObject.AddComponent<BotonSimpleView>();
            SceneBuilderUtils.AsignarLista(boton, "_graficos", graficos);
            return boton;
        }

        private static ChapterInfoPanelView CrearPanelInfo(Transform padre)
        {
            var panel = SceneBuilderUtils.CrearRect("InfoPanel", padre);
            SceneBuilderUtils.Colocar(panel, new Vector2(740f, 0f), new Vector2(420f, 520f));   // fuera del anillo del reloj
            var vista = panel.gameObject.AddComponent<ChapterInfoPanelView>();

            string[] campos = { "Titulo", "Dificultad", "Llaves", "Tiempo" };
            float[] ys = { 195f, 65f, -65f, -195f };
            var cajas = new CanvasGroup[4];
            var textos = new TextMeshProUGUI[4];
            for (int i = 0; i < 4; i++)
            {
                var caja = SceneBuilderUtils.CrearImage($"Caja_{campos[i]}", panel, new Color(0f, 0f, 0f, 0.8f));
                SceneBuilderUtils.Colocar(caja.rectTransform, new Vector2(0f, ys[i]), new Vector2(420f, 110f));
                cajas[i] = caja.gameObject.AddComponent<CanvasGroup>();
                textos[i] = SceneBuilderUtils.CrearTexto("Texto", caja.transform, campos[i], 44f, Color.white);
                SceneBuilderUtils.Estirar(textos[i].rectTransform);

                // Cajas de llaves y de tiempo: un ícono pixel art a la izquierda del texto.
                string rutaIcono = i == 2 ? "Assets/Art/Props/llave.png" : i == 3 ? "Assets/Art/Props/cronometro.png" : null;
                if (rutaIcono != null)
                {
                    var spriteIcono = SceneBuilderUtils.CargarSprite(rutaIcono, true);
                    if (spriteIcono != null)
                    {
                        bool esTiempo = i == 3;      // el tiempo es más largo que "0/3": ícono más a la izquierda
                        var icono = SceneBuilderUtils.CrearImage(esTiempo ? "IconoCronometro" : "IconoLlave", caja.transform, Color.white, spriteIcono);
                        SceneBuilderUtils.Colocar(icono.rectTransform, new Vector2(esTiempo ? -125f : -55f, 0f), new Vector2(60f, 60f));
                        icono.preserveAspect = true;
                        textos[i].rectTransform.offsetMin = new Vector2(esTiempo ? 60f : 80f, 0f);
                    }
                }
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

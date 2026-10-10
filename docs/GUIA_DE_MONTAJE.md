# Menú de inicio + selección de capítulo con reloj (Unity)

Código organizado con la misma arquitectura del ejemplo del profesor (Domain, Data, Datasources, Presentation, UI, DI, Editor, Tests), con **composición manual** (sin Zenject/VContainer): cada `Installer` es un `MonoBehaviour` que arma todo en `Awake()` y llama a `view.Construir(viewModel)`.

## Cómo instalarlo

1. Copia la carpeta `Assets/Scripts` de este paquete dentro del `Assets` de tu proyecto Unity (el nombre `ClockGame` de la raíz es solo un placeholder; usa el de tu proyecto).
2. Ten instalados los paquetes **TextMeshPro** (Unity pedirá importar "TMP Essentials" la primera vez) y **Test Framework** (viene por defecto). Compatible con Input System nuevo o Input Manager clásico.
3. Si el Input System nuevo no está instalado, Unity mostrará un aviso sobre la referencia `Unity.InputSystem` del asmdef de `Shared`. Es solo un warning; no afecta.
4. Crea las escenas con los menús del editor (abajo) y agrégalas en *File > Build Settings*: `MainMenu`, `ChapterSelect`, `Chapter1`.

## Árbol

```
Assets/Scripts/
├── Shared/                                   (UniversalPlatform.Shared)
│   ├── Input/MenuInput.cs                    flechas + WASD + Espacio/Enter/Escape, ambos sistemas de input
│   ├── UI/EventSystemSetup.cs, ProceduralSprites.cs, Shaders/UI_Grayscale.shader
│   ├── FX/NightBackgroundView.cs, LightBeamView.cs, DustMotesView.cs
│   └── Editor/SceneBuilderUtils.cs (+ asmdef)
└── Features/
    ├── Progress/        llaves y tiempo por capítulo (persistencia JSON)
    ├── MainMenu/        Nueva partida, Cargar partida, Opciones, Salir
    └── ChapterSelect/   reloj de 12 capítulos, boxes de info, puerta con zoom
        ├── Domain/          Capitulo, ReglasReloj, ports y use cases (sin dependencias de Unity)
        ├── Data/            CapitulosDefaultDataSource(+Port), CapitulosDataRepository
        ├── Presentation/Clock/   ChapterSelectViewModel, ChapterSelectViewState
        ├── UI/Clock|Info|Transition/   ChapterSelectView, ClockNumberView, ChapterInfoPanelView, ClockDoorTransitionView
        ├── DI/              ChapterSelectInstaller
        ├── Editor/          ChapterSelectSceneBuilder (+ asmdef)
        ├── Tests/Editor|Integration|E2E/   (+ asmdefs)
        └── UniversalPlatform.Features.ChapterSelect.asmdef
```

`MainMenu` y `Progress` siguen el mismo esquema por dentro. Diferencias con el ejemplo del profesor: `ChapterSelect` no tiene carpeta `Datasources` porque su catálogo de capítulos vive en código (solo `Progress` persiste en JSON), y `MainMenu` no tiene `Data` porque no guarda nada propio. `Progress` tiene además `DI/ProgresoCompositionRoot.cs`, que arma el repositorio para que las tres escenas lean y escriban el mismo archivo.

**Dependencias entre capas:** `UI → Presentation → Domain ← Data ← Datasources`, y `DI` conoce todo. La lógica del reloj (ángulos, navegación, camino más corto, minutero) está en `ReglasReloj`, sin Unity, por eso se puede testear en Edit Mode.

## Escena MainMenu

1. Abre una escena vacía y ejecuta **UniversalPlatform > MainMenu > Construir escena**. Genera el Canvas, el fondo de vitrales con polvo, los 4 botones, el submenú de partidas guardadas (velas), paneles temporales, fundido y el `MainMenuInstaller`.
2. Guarda como `MainMenu`.

**Cambiar al arte propio:** en cada `Btn_*` (componente `MenuButtonView`) arrastra tus sprites a *Imagenes* (y/o deja los textos TMP en *Textos*).
- Para que tus sprites pasen de gris a color original (en vez de teñirse), crea un Material con el shader `UniversalPlatform/UI/Grayscale` y arrástralo a *Material Grayscale*.
- *Brillo* es una imagen opcional de resplandor detrás del botón; *Escala Iluminado* = 1.1.

**Controles:** mouse (hover + clic izquierdo), ↑↓←→ y WASD para moverse, Espacio o Enter para activar, Escape cierra los paneles.
**Flujo:** Nueva partida borra el progreso y va a `ChapterSelect`; Cargar partida va allí solo si hay guardado (si no, muestra un aviso); Opciones abre un panel "En construcción"; Salir cierra el juego.

### Fondo, botones y partidas guardadas (velas)

- **Fondo:** `Assets/Art/Backgrounds/fondo_vitrales.png` (vitrales luminosos, ya desenfocados). Lo usan las dos escenas de menú; qué tan oscuro se ve se ajusta en `Background` > *Image > Color* (más gris = más oscuro). La imagen y la vela se regeneran con `python3 ArteFuente/generar_arte.py`.
- **Botones:** solo texto. Sin seleccionar van en blanco atenuado; seleccionados, en blanco pleno, más grandes y con resplandor en las letras (`MenuButtonView`: *Color Gris*, *Color Iluminado*, *Escala Iluminado*, *Brillo Texto*).
- **Partidas guardadas:** hay 3 ranuras (`ReglasRanuras.CANTIDAD`), una vela por ranura dentro del objeto `CargarPartida`. Vela encendida = hay partida; apagada = ranura vacía.
  - *Nueva partida* usa la primera vela libre y entra directo. Si las tres están ocupadas, abre las velas para elegir cuál reemplazar.
  - *Cargar partida* abre las velas. Al seleccionar una encendida aparece el recuadro negro con capítulo, porcentaje, tiempo de partida y fecha del último guardado; clic o Enter la carga. *Volver* (o Escape) cierra.
  - El porcentaje es fijo por ahora: `ReglasRanuras.PORCENTAJE_PROVISIONAL` (10). Cuando existan los capítulos, se reemplaza `ReglasRanuras.CalcularPorcentaje`.
  - Cada ranura es un archivo en `Application.persistentDataPath`: `progreso_partida.json` (ranura 1), `progreso_partida_2.json` y `progreso_partida_3.json`. Para probar con las velas apagadas, borra esos archivos.

## Escena ChapterSelect

1. Escena vacía > **UniversalPlatform > ChapterSelect > Construir escena**. Genera la esfera, los 12 números, las manillas, los 4 boxes, el interior del portal (oculto hasta entrar), la cámara y el `ChapterSelectInstaller`.
2. Guarda como `ChapterSelect`.

**Arte del reloj:** el constructor usa los PNG de `Assets/Art/Clock` (esfera, 12 numerales romanos y las dos manillas; los SVG originales están en `ArteFuente/Reloj`). Las manillas están dibujadas con el eje en el centro del lienzo, así que usan **Pivot = (0.5, 0.5)** y el mismo tamaño que la esfera. El reloj no lleva relleno y va sobre el mismo fondo de vitrales, más oscurecido: las líneas y las manillas son blancas y cada una tiene detrás una capa de halo (`ClockFaceGlow` y el hijo `Brillo` de cada manilla) cuyo color e intensidad se cambian en *Image > Color*. Todos los PNG son blancos para poder teñirlos; los colores de los numerales (normal, iluminado, bloqueado) se ajustan en cada `Numero_N` > `ClockNumberView`. Si falta algún PNG, el constructor usa el arte provisional y avisa por consola.

**Cambiar al arte propio (si reemplazas los PNG a mano):**
- *ClockFace*: tu esfera, lo más grande posible. Los números y manillas deben quedar centrados sobre ella.
- *Manillas*: sprites dibujados **apuntando hacia arriba** y con el **Pivot en la base** (Pivot = 0.5, 0). La horaria apunta al capítulo y el minutero da una vuelta completa por cada hora recorrida, terminando siempre en las 12.
- *Números*: cada `Numero_N` tiene un TMP (o tu numeral como Image). `ChapterSelectView` los coloca en círculo con *Radio Numeros* (ajústalo a tu dibujo o desmarca *Colocar Numeros Automaticamente*). Mantén el tamaño ~120x120 para que el hover sea cómodo.
- *InfoPanel*: los 4 boxes (título, dificultad, llaves, tiempo). Se refleja al lado contrario del número seleccionado, por eso `InfoPanel` debe estar anclado al centro y colocado a la derecha.
- *Escenario > Portal*: círculo con `Mask`, detrás del reloj; en `ChapterSelectView > Visuales` asigna el arte de cada capítulo (*Numero Reloj* + *Arte Del Portal*) y se verá dentro del círculo al entrar.

**La puerta:** al entrar, el reloj completo (`ClockGroup`) se abre hacia atrás como una puerta redonda, girando en 3D sobre su borde izquierdo, mientras la vista avanza hacia el interior (`Portal`) hasta que llena la pantalla y carga la escena. Para que el giro tenga perspectiva, esta escena usa un Canvas en *Screen Space - Camera* con una cámara en perspectiva (la crea el constructor). Ángulo y tiempos se ajustan en `ChapterSelectLogic` > `ClockDoorTransitionView`; lo que se ve dentro es `Portal > Arte` (o el arte que asignes por capítulo en `ChapterSelectView > Visuales`).

**Entrada a la escena:** al llegar (desde Nueva/Cargar partida o al volver de un capítulo) la pantalla sale del negro y el reloj aparece desvaneciéndose mientras crece desde el 40% hasta su tamaño (unos 2,4 s); los boxes y el botón Volver llegan al final. No se acepta input hasta que termina. Duración y tamaño inicial en `ClockDoorTransitionView > Entrada a la escena`; desmarca *Animar Entrada* para quitarla.

**Controles:** el mouse sobre un número lo selecciona (las manillas se mueven); clic izquierdo o Espacio/Enter entra. → D avanzan en sentido del reloj, ← ↑ A W retroceden; ↓ S baja al botón **Volver** (Enter vuelve al menú; ↑ ← → regresan al reloj). Escape vuelve al menú. Solo el capítulo 1 está disponible en el MVP; los demás se ven apagados y el panel tiembla al intentar entrar.

## Partidas guardadas: borrar

En la pantalla de velas, con una vela encendida seleccionada aparece **Borrar partida** abajo a la derecha (o se usa la tecla **Supr**). La primera vez pide confirmación en el recuadro; la segunda borra el archivo de esa ranura y la vela se apaga. Moverse a otra vela cancela la confirmación.

Con teclado: ← → recorren las velas y los botones; ↓ desde una vela encendida baja a **Borrar partida** (desde una apagada, a **Volver**); en los botones, ← → alternan entre Volver y Borrar, y ↑ vuelve a la vela. Enter sobre Borrar pide la confirmación y, la segunda vez, borra. El botón lleva una X con el mismo dibujo de puntas y volutas que la flecha de Volver (`Assets/Art/UI/x_borrar.png`; el SVG está en `ArteFuente/Reloj/x_borrar.svg`).

## Selección de capítulo: volver

Abajo a la izquierda hay un botón **Volver** (con la flecha de las manillas) que lleva al menú principal, igual que Escape.

## Escena Chapter1: el aposento negro

1. Escena vacía > **UniversalPlatform > Capítulos > Construir aposento negro (Chapter1)**. Crea el fondo, el HUD, el velo de la narradora, el cronómetro del capítulo y el `RoomPuzzleInstaller`.
2. Guarda como `Chapter1` (reemplazando la que hay).

El capítulo 1 es el **aposento negro**: terciopelo negro y vitrales escarlata, la séptima sala del relato, donde está el reloj de ébano. La sala mide **20 × 5 baldosas**; la cámara sigue al encapuchado de lado (`RoomView > Cámara`) y el fondo panorámico mide 2 pantallas de ancho.

El **aposento azul** queda guardado (id `aposento_azul`) pero ningún capítulo lo usa. Para verlo: escena vacía > **Construir aposento azul (guardado, sin capítulo)**.

**Cinemática:** al entrar se ve primero `Assets/Video/cinematica_capitulo1.mp4` (copia sin audio y reducida a 1446x1080 del video original). **Espacio**, Enter o clic saltan a la siguiente línea de diálogo; **Escape** la salta entera. Los puntos donde empieza cada línea están en `Cinematica > CinematicaView > Cortes` (10.8 s y 21.4 s); si cambias el video, ajústalos ahí. Para quitarla, borra el objeto `Cinematica` de la escena.

**Cómo se juega:** el suelo es el puzle. El reloj de ébano da tres campanadas, se abre y de él sale el encapuchado rojo (la máscara del relato), que avanza de baldosa en baldosa dejando un trazo rojo. No puede cruzar su propio trazo ni atravesar invitados o muebles, pero **sí devolverse por el mismo camino**: pisar la baldosa de la que viene recoge el trazo un paso (y si ahí tomó una llave, la llave vuelve a su lugar). Debe recoger las 3 llaves y llegar a la puerta del oeste: sin las tres, la puerta no cede. Al pasar junto a un invitado, este suelta una frase de fiesta (no le habla a él: no lo ven).

**Entrada:** al terminar (o saltar) la cinemática, el reloj de ébano da tres campanadas, se abre y el encapuchado camina hasta su primera baldosa; la portezuela se cierra tras él. Recién ahí se puede jugar.

**Cronómetro:** arriba a la derecha, con su ícono. Solo corre mientras se juega (no durante la cinemática, la entrada ni el final) y **cada vez que entras al capítulo parte de 0**. En la partida guardada se sigue sumando todo lo jugado en el capítulo: ese total es el que muestran la caja de tiempo del reloj y el "tiempo de partida" de las velas.

**Final de la demo:** al resolver la sala se abre la puerta de la izquierda, el encapuchado la cruza, el escenario se desliza a la derecha hacia una habitación negra y aparece "Gracias por jugar nuestra demo" con los créditos. Espacio, Enter, Escape o clic (o 15 s sin tocar nada) vuelven al menú principal. Si borras el objeto `FinalDemo` de la escena, al resolver se vuelve directo al reloj.

**Controles:** flechas o WASD (o clic en una baldosa vecina) para moverse; volver a la baldosa anterior retrocede. **Z** también deshace un paso, **R** reinicia la sala, **Escape** vuelve a la selección de capítulos. En los textos de la narradora, Espacio, Enter o clic para continuar.

**Al completar:** se guardan las llaves (la mejor marca, no se suman al repetir), el tiempo y el capítulo como completado, y se vuelve al reloj.

**Dónde está cada cosa** (`Assets/Scripts/Features/RoomPuzzle`):
- La sala se define en `Data/SalasDataRepository.cs` como un **mapa de texto**: una línea por fila (la primera es el fondo) y una letra por baldosa: `.` libre, `E` entrada (reloj), `X` salida (puerta), `L` llave, `I` invitado, `B` librería y `S` sillón (dos letras seguidas = un mueble de 2 baldosas), `P` planta, `M` mesita, `C` candelabro. Debajo van los invitados en orden de lectura, cada uno con su máscara (gato, zorro, halcón), su color (original, plata, carmesí, jade, violeta) y sus frases. Un test comprueba que la sala tenga solución.
- Las reglas del puzle están en `Domain/PartidaSala.cs`, sin Unity, con sus tests en `Tests/Editor`.
- `UI/Room/RoomView.cs` dibuja todo al iniciar a partir de la definición. Tamaños, colores y sprites se cambian en el inspector, en el objeto `RoomView`.

**Arte:** pixel art en `Assets/Art/Characters`, `Assets/Art/Props` y `Assets/Art/Rooms/Black` (y `Rooms/Blue`), importado con filtro *Point*. Es provisional: el aposento negro, el reloj de ébano, los muebles y los invitados con máscaras de colores se regeneran con `python3 ArteFuente/generar_sala_negra.py`; el protagonista y el aposento azul, con `python3 ArteFuente/generar_sala_azul.py`. El protagonista tiene 4 cuadros de caminata por dirección (`encapuchado_<sur|norte|este|oeste>_<0-3>.png`); los invitados son la misma figura con capucha negra y máscaras de gato, zorro y halcón. Para usar arte propio, reemplaza los PNG con el mismo nombre y tamaño.

## Habilitar más capítulos

En `ChapterSelect/Data/CapitulosDefaultDataSource.cs` cambia `disponible: n == 1` por la condición que necesites y crea la escena `ChapterN` correspondiente. El número total de llaves y la dificultad de cada uno se definen ahí mismo.

## Tests

Abre *Window > General > Test Runner*.
- **EditMode** (`Tests/Editor` e `Tests/Integration`): reglas del reloj, use cases, repositorios, ViewModels, y JSON real en un directorio temporal.
- **PlayMode** (`Tests/E2E`): vista + ViewModel + use cases reales; el mouse se simula con los handlers de puntero.

## Supuestos y límites

- Los namespaces siguen el patrón `UniversalPlatform.Features.<Feature>` del ejemplo; renombra si tu proyecto usa otro.
- Los SceneBuilders usan arte provisional (rectángulos, círculo integrado de Unity y texto TMP); el arte definitivo lo pones tú, sin cambiar código.
- **No pude compilar ni ejecutar nada dentro de Unity desde aquí.** Verifiqué que los namespaces, referencias entre asmdefs y llaves cuadren, y revisé a mano la lógica de los tests. Si algo no compila o un test falla, pégame el error y lo ajusto.

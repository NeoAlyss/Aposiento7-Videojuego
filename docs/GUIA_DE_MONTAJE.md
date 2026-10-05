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

1. Escena vacía > **UniversalPlatform > ChapterSelect > Construir escena**. Genera la esfera, los 12 números, las manillas, los 4 boxes, la capa de la puerta (desactivada) y el `ChapterSelectInstaller`.
2. Guarda como `ChapterSelect`.

**Arte del reloj:** el constructor usa los PNG de `Assets/Art/Clock` (esfera, 12 numerales romanos y las dos manillas; los SVG originales están en `ArteFuente/Reloj`). Las manillas están dibujadas con el eje en el centro del lienzo, así que usan **Pivot = (0.5, 0.5)** y el mismo tamaño que la esfera. El reloj no lleva relleno y va sobre el mismo fondo de vitrales, más oscurecido: las líneas y las manillas son blancas y cada una tiene detrás una capa de halo (`ClockFaceGlow` y el hijo `Brillo` de cada manilla) cuyo color e intensidad se cambian en *Image > Color*. Todos los PNG son blancos para poder teñirlos; los colores de los numerales (normal, iluminado, bloqueado) se ajustan en cada `Numero_N` > `ClockNumberView`. Si falta algún PNG, el constructor usa el arte provisional y avisa por consola.

**Cambiar al arte propio (si reemplazas los PNG a mano):**
- *ClockFace*: tu esfera, lo más grande posible. Los números y manillas deben quedar centrados sobre ella.
- *Manillas*: sprites dibujados **apuntando hacia arriba** y con el **Pivot en la base** (Pivot = 0.5, 0). La horaria apunta al capítulo y el minutero da una vuelta completa por cada hora recorrida, terminando siempre en las 12.
- *Números*: cada `Numero_N` tiene un TMP (o tu numeral como Image). `ChapterSelectView` los coloca en círculo con *Radio Numeros* (ajústalo a tu dibujo o desmarca *Colocar Numeros Automaticamente*). Mantén el tamaño ~120x120 para que el hover sea cómodo.
- *InfoPanel*: los 4 boxes (título, dificultad, llaves, tiempo). Se refleja al lado contrario del número seleccionado, por eso `InfoPanel` debe estar anclado al centro y colocado a la derecha.
- *DoorLayer > Portal*: círculo con `Mask`; en `ChapterSelectView > Visuales` asigna el arte de cada capítulo (*Numero Reloj* + *Arte Del Portal*) y se verá dentro del círculo al entrar.

**La puerta:** al seleccionar, se captura la pantalla, se corta en dos mitades y cada una gira sobre su bisagra exterior (scaleX 1 → 0). Por la abertura aparece el portal circular, se hace zoom y la escena se carga en segundo plano. Funciona con cualquier reloj que dibujes.

**Controles:** el mouse sobre un número lo selecciona (las manillas se mueven); clic izquierdo o Espacio/Enter entra. → ↓ D S avanzan en sentido del reloj, ← ↑ A W retroceden. Escape vuelve al menú. Solo el capítulo 1 está disponible en el MVP; los demás se ven apagados y el panel tiembla al intentar entrar.

## Escena Chapter1 (y las siguientes)

Ejecuta **UniversalPlatform > Progress > Agregar cronómetro de capítulo a la escena** y deja *Id Capitulo* = `chapter_01`. Desde tu lógica de juego:

```csharp
[SerializeField] private ChapterTimerView _progreso;   // UniversalPlatform.Features.Progress.UI.ChapterTimer

_progreso.AgregarLlave();   // al recoger una llave
_progreso.Completar();      // al terminar el capítulo
```

El tiempo cuenta solo mientras `Time.timeScale > 0` y se guarda cada 10 s, al pausar y al salir de la escena. El primer intento parte en `00:00:00` y `0 / 3` llaves.

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

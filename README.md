# Aposiento7 — Videojuego

Demo de un juego de puzles en Unity basado en *La máscara de la Muerte Roja* de Edgar Allan Poe.
Una máscara de capucha roja sale del reloj de ébano a medianoche y recorre los aposentos del
príncipe Próspero, cruzando cada sala por un suelo de baldosas que es, en sí mismo, el puzle.

**Demo actual:** menú de inicio con 3 partidas guardadas (velas), reloj de selección de capítulos
y el **capítulo 1 · Aposento negro** completo, con cinemática, puzle de 20 × 5 baldosas y final de la demo.

Hecho por **Alondra González** y **Andrea Díaz**.

## Cómo abrirlo

1. Instala **Unity 6000.5.10f1** desde Unity Hub (otra versión de Unity 6 también debería funcionar).
2. Descarga el repositorio (**Code → Download ZIP** y descomprímelo, o `git clone`).
3. En Unity Hub: **Add → Add project from disk** y elige la carpeta del proyecto.
   La primera vez Unity tarda unos minutos en importar todo.
4. Abre la escena `Assets/Scenes/MainMenu` y aprieta **Play**.

Las escenas ya vienen armadas. Si se cambia el código de una escena, se reconstruye desde el
menú **UniversalPlatform** del editor (detalles en [`docs/GUIA_DE_MONTAJE.md`](docs/GUIA_DE_MONTAJE.md)).

## Cómo se juega

- **Menú:** flechas o WASD para moverse, Enter o Espacio para elegir, Escape para volver. También funciona con el mouse.
- **Reloj de capítulos:** ← → mueven las manillas, Enter entra al capítulo, ↓ baja al botón Volver.
- **Capítulo 1:** avanza de baldosa en baldosa dejando un trazo; no puedes cruzarlo, pero sí devolverte
  pisando la baldosa de la que vienes. Junta las **3 llaves** esquivando invitados y muebles, y llega a
  la puerta. **R** reinicia la sala y **Escape** vuelve a la selección de capítulos.

## Arquitectura

Cada parte del juego es una *feature* con su propio ensamblado y capas separadas
(Domain → Data → Datasources → Presentation → UI → DI). Las reglas del juego no dependen de
Unity y tienen tests (Window → General → Test Runner). La composición es manual: cada escena tiene
un `Installer` que arma las dependencias y se las entrega a la vista.

```
Assets/Scripts/
├── Shared/            input, transiciones, cinemática, efectos y utilidades de editor
└── Features/
    ├── Progress/       partidas guardadas, llaves y tiempo por capítulo (JSON)
    ├── MainMenu/       menú de inicio y velas de partidas guardadas
    ├── ChapterSelect/  reloj de selección de capítulo
    └── RoomPuzzle/     los aposentos: el puzle del suelo (capítulo 1)
ArteFuente/            scripts que generan el arte provisional (pixel art)
docs/                  guía de montaje de las escenas
```

## Ramas

- `main` — versión estable (la que se descarga).
- `develop` — integración de lo terminado.
- `feature/*` — una rama por trabajo; entra a `develop` por Pull Request.

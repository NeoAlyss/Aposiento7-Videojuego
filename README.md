# Aposiento7 — Videojuego

MVP de un juego en Unity. Por ahora el alcance cubre el **menú de inicio** y la
**selección de capítulo** (hasta el capítulo 1).

## Arquitectura

El código sigue capas (Domain, Data, Datasources, Presentation, UI, DI) con
composición manual: cada `Installer` es un `MonoBehaviour` que arma las
dependencias en `Awake()` y se las pasa a la vista. El detalle completo de
cómo montar las escenas en Unity está en [`docs/GUIA_DE_MONTAJE.md`](docs/GUIA_DE_MONTAJE.md)
(se agrega en la rama `feature/shared-core`).

```
Assets/Scripts/
├── Shared/                   input, EventSystem, fondo nocturno, shader
└── Features/
    ├── Progress/              llaves y tiempo por capítulo (persistencia JSON)
    ├── MainMenu/               menú de inicio
    └── ChapterSelect/          reloj de selección de capítulo
```

## Ramas

- `main` — versión estable.
- `develop` — integración de las features ya terminadas.
- `feature/*` — una rama por parte del proyecto, cada una se integra a
  `develop` mediante Pull Request.

Orden de dependencias entre features (importa al revisar/fusionar los PR):

1. `feature/shared-core` — no depende de nada.
2. `feature/progress-system` — depende de `Shared`.
3. `feature/main-menu` y `feature/chapter-select` — dependen de `Shared` y de `Progress`.

## Requisitos

- Unity (con el paquete TextMeshPro importado).
- Input System nuevo o Input Manager clásico (el proyecto es compatible con ambos).

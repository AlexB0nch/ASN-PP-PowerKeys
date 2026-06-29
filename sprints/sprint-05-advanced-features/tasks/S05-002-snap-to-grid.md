# S05-002 — Snap-to-grid 0.1 cm

> Передача builder'у: `/builder выполни S05-002`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S05-002` |
| **Спринт** | `sprint-05-advanced-features` |
| **Компонент** | Core + Api + AddIn + Tests |
| **Статус** | In Progress |
| **Issue** | #33 |

## Цель

Реализовать **Consulting Mode snap-to-grid 0.1 cm**: после геометрических layout-команд
выравнивать позицию и размеры фигур к сетке **0.1 cm**, если пользователь включил опцию в Settings.
Математика — в **Core** (тестируемо без PowerPoint); применение — post-process в layout pipeline
перед `applyShapeBounds`.

## Контекст

- README / Consulting Mode: «Автоматическое выравнивание по сетке 0.1 cm».
- `ShapeBounds` в **points** (Office.js parity). 1 cm = 72/2.54 ≈ **28.346456692913385** pt;
  шаг сетки 0.1 cm ≈ **2.8346456692913385** pt.
- S05-001 Done: `UserSettings.profile` + presets; расширяем тот же DTO.
- `LayoutEngine.Apply` → Api `POST /api/layout/apply` → AddIn `applyShapeBounds`.
- **Anti-scope:** snap-to-nearest-object при drag — Office.js не даёт hook; не реализовывать.

## Scope

### 1. Core — `GridSnap` (новый)

Файл: `src/PptPowerKeys.Core/Layout/GridSnap.cs`

- `public const double GridStepCm = 0.1;`
- `public const double PointsPerCm = 72.0 / 2.54;` (или `GridStepPoints` как derived constant)
- `public static double SnapValue(double points, double gridStepPoints = default)` — округление к ближайшему шагу.
- `public static ShapeBounds Snap(ShapeBounds shape, double gridStepPoints = default)` — snap `Left`, `Top`, `Width`, `Height`
  (все четыре координаты/размера; минимальный размер не ниже `LayoutOptions.MinSize` после snap).
- `public static IReadOnlyList<ShapeBounds> SnapAll(IEnumerable<ShapeBounds> shapes, ...)` — map по списку.
- Юнит-тесты `GridSnapTests.cs`:
  - 0.0 → 0.0; значение на сетке не меняется
  - 2.85 pt (≈0.1 cm) snap к 2.8346…
  - mid-point rounding (0.05 cm от узла → ближайший узел)
  - `Snap(shape)` сохраняет `Id`, меняет геометрию
  - width/height не уходят ниже 1 pt при агрессивном snap

### 2. Core — post-process в layout pipeline

Расширить `LayoutOptions`:

```csharp
public bool SnapToGrid { get; init; } = false;
public double GridStepCm { get; init; } = GridSnap.GridStepCm;
```

В `LayoutEngine.Apply`: после вычисления результата (только если `result.Changed` и `options.SnapToGrid`),
прогнать `result.Shapes` через `GridSnap.SnapAll`. Если snap изменил координаты — оставить `Changed = true`.

**Не** snap'ить no-op результаты (`NoChange`).

### 3. Core — `UserSettings`

`UserSettings.cs`:

- `public bool SnapToGrid { get; set; } = false;`
- `CreateDefaults()` — `SnapToGrid = false`
- JSON round-trip (существующие файлы без поля → default false благодаря default value)

Тест: `UserSettings_RoundTripsThroughJson` / новый тест с `snapToGrid: true`.

### 4. Api

- `LayoutApiRequest` / `LayoutOptions` — поле `snapToGrid` (bool, optional) пробрасывается в Core.
  AddIn передаёт флаг из загруженных settings при каждом `applyLayout`.
- Альтернатива (не предпочтительна): читать settings на сервере по `X-User-Id` — **не делать**,
  чтобы layout endpoint оставался stateless относительно user store.
- Integration test: `ApplyLayout_AlignLeft_WithSnapToGrid_SnapsResult` — shapes с off-grid left,
  `options: { snapToGrid: true }` → left кратен шагу 0.1 cm.

### 5. AddIn

**types.ts** — `UserSettings.snapToGrid?: boolean`; `LayoutOptions` или поле в applyLayout body.

**api.ts** — `applyLayout(command, shapes, anchorIndex?, snapToGrid?)` передаёт `options: { snapToGrid }`.

**SettingsPanel.tsx** — Checkbox «Snap to grid (0.1 cm)» в Consulting / General секции;
сохраняется через `saveSettings`; Reset сбрасывает в false.

**runCommand.ts** — загружать `snapToGrid` из settings при ServerLayout:
  - Вариант A: кэш settings в App и передавать в `runCommand(descriptor, settingsActions, layoutOptions)`.
  - Вариант B: `runServerLayout` вызывает `api.getSettings()` (лишний round-trip).
  - **Предпочтение:** App держит `userSettings` state (reload при Settings save/reset), передаёт `snapToGrid` в `runCommand`.

**applyShapeBounds** — **не** дублировать snap-математику на клиенте; shapes уже приходят snapped с API.

### 6. Copy-and-align path

`runCopyAndAlign` тоже вызывает `api.applyLayout` — передать тот же `snapToGrid` флаг.

## Анти-scope

- Snap-to-nearest-object при drag/move
- Toggle PowerPoint view grid (`ToggleGrid` — HostScript None)
- Новые CommandIds
- Snap на HostScript-only командах (Duplicate*, PasteObjectPosition) — **вне scope** S05-002
- `VstoLegacy*`

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/Layout/GridSnap.cs` (новый)
- `src/PptPowerKeys.Core/Layout/LayoutOptions.cs`
- `src/PptPowerKeys.Core/Layout/LayoutEngine.cs`
- `src/PptPowerKeys.Core/Settings/UserSettings.cs`
- `src/PptPowerKeys.Api/Contracts/ApiContracts.cs` (если нужно)
- `src/PptPowerKeys.Tests/GridSnapTests.cs` (новый)
- `src/PptPowerKeys.Tests/LayoutEngineTests.cs` (snap integration)
- `src/PptPowerKeys.Tests/ApiIntegrationTests.cs`
- `src/PptPowerKeys.Tests/SettingsAndCatalogTests.cs`
- `src/PptPowerKeys.AddIn/src/services/types.ts`, `api.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`, `runCommand.ts`

## Критерии приёмки (Definition of Done)

1. [ ] `GridSnap` unit-тесты: шаг 0.1 cm, rounding, ShapeBounds snap.
2. [ ] `LayoutEngine` с `SnapToGrid=true` post-process после align/resize/distribute.
3. [ ] `UserSettings.snapToGrid` persist round-trip (GET/PUT/reset).
4. [ ] Settings UI: checkbox Snap to grid 0.1 cm; Save/Reset работают.
5. [ ] AddIn передаёт `snapToGrid` в `applyLayout`; ServerLayout команды дают snapped geometry.
6. [ ] `dotnet test PptPowerKeys.sln` — все зелёные (≥93 + новые).
7. [ ] `npm run typecheck` и `npm run validate:prod` — зелёные.
8. [ ] PR с `Sprint`/`Task ID` S05-002, `Closes #<issue>`.

## Зависимости

- S05-001 Done (PR #32). Блокеров нет.

## Примечание для builder

- Ветка: `cursor/S05-002-snap-to-grid-ea49`
- Не менять `CommandCatalog` / `CommandIds`.
- Обновить `docs/PRODUCT_CONTEXT.md` (журнал решений S05-002).

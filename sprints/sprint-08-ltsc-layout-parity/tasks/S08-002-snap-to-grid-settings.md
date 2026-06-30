# S08-002 — Snap-to-grid (LayoutOptions + local UserSettings)

> Передача builder'у: `/builder выполни S08-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S08-002` |
| **Спринт** | `sprint-08-ltsc-layout-parity` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (reference) |
| **Статус** | Done |
| **Issue** | [#64](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/64) |
| **PR** | [#65](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/65) |

## Цель

Включить **snap-to-grid 0.1 cm** для всех 32 ServerLayout команд на Windows line — parity с Web Add-in
(S05-002): `UserSettings.SnapToGrid` → `LayoutOptions.SnapToGrid` → `LayoutEngine` post-process `GridSnap`.

## Контекст (после S08-001)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 32 ServerLayout via `LayoutEngine.IsLayoutCommand`; `LayoutRequest.Options` **не передаётся** |
| Core | `GridSnap.GridStepCm = 0.1`; `LayoutEngine.MaybeSnapToGrid` уже реализован |
| Web Add-in | `getLayoutOptions()` из `UserSettings.snapToGrid`; API stateless `options` на каждый layout call |
| Windows | **Нет** persistence `UserSettings`; **нет** UI toggle snap |

## Решения architect (зафиксировано)

### Persistence — local JSON (Core shape)

- Использовать **`PptPowerKeys.Core.Settings.UserSettings`** (не VstoLegacy).
- Path: **`%AppData%/PptPowerKeys/UserSettings.json`** (Roaming — sync across domain profiles; document in README).
- Load on add-in startup; Save on toggle change.
- JSON shape **совместим** с Web export/import v1 (`profile`, `snapToGrid`, `shortcuts[]`, optional `addupDisplayMode`).
- **S08-002 scope:** persist **`SnapToGrid` only** required; shortcuts/profile — default empty/Custom OK (S10 expands).

### LayoutOptions wiring

```csharp
var options = new LayoutOptions { SnapToGrid = settings.SnapToGrid };
// GridStepCm = default 0.1 — не менять
request.Options = options;
```

Все 32 ServerLayout commands получают snap автоматически через existing `ExecuteServerLayout`.

### UI — minimal ribbon toggle (в scope S08-002)

- Ribbon **checkbox** «Snap to grid (0.1 cm)» в group Layout (или Settings stub group).
- `getPressed` / `onAction` — read/write `UserSettings.SnapToGrid` + Save.
- Полный Settings task pane — **S10**; layout command buttons — **S08-003**.

### Import/export

**Anti-scope S08-002** — только local file round-trip для snap flag.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `PptPowerKeys.Windows/Settings/WindowsUserSettingsStore.cs` (new) | Load/Save Core `UserSettings` |
| `PptPowerKeys.Windows/Host/CommandRouter.cs` | Inject settings; pass `LayoutOptions` |
| `PptPowerKeys.Windows/ThisAddIn.cs` | Wire store + router |
| `PptPowerKeys.Windows/UI/RibbonTab.xml` | Checkbox snap toggle |
| `PptPowerKeys.Windows/UI/PowerKeysRibbon.cs` | Toggle handlers |
| `PptPowerKeys.Windows/README.md` | Snap QA steps |
| Optional tests | Unit test: store round-trip; mock router gets SnapToGrid=true |

## Анти-scope

- Full Settings panel / Shortcut Manager (S10)
- Ribbon buttons for all 32 layout cmds (S08-003)
- CopyAndAlign (S08-004)
- Api HTTP / sync with Web backend
- Изменение `GridSnap` math (already tested in Core)
- VstoLegacy*

## Критерии приёмки

- [ ] Ribbon checkbox toggles snap; persists across restart PowerPoint
- [ ] With snap **on**: AlignLeft (and ≥1 other layout cmd, e.g. SameWidth) snaps geometry to 0.1 cm grid (manual Windows QA)
- [ ] With snap **off**: behavior matches S08-001 (no snap)
- [ ] `LayoutEngineTests` / existing Core tests unchanged green
- [ ] `dotnet test PptPowerKeys.sln` — зелёный
- [ ] JSON file uses `snapToGrid` camelCase compatible with Web export shape
- [ ] Manual QA note in PR/README

## Зависимости

- S08-001 Done (PR #63)

## Трассировка

Issue `#N` → `cursor/S08-002-snap-to-grid-*` → PR `Closes #N`

## Reference (Web parity)

- `src/PptPowerKeys.AddIn/src/runtime/commandContext.ts` — `getLayoutOptions()`
- `docs/PRODUCT_CONTEXT.md` — S05-002 journal
- `src/PptPowerKeys.Tests/LayoutEngineTests.cs` — `AlignLeft_WithSnapToGrid_SnapsResultGeometry`

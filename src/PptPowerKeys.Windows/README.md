# PptPowerKeys.Windows — LTSC / perpetual Office (Product Line B)

VSTO add-in for **Windows PowerPoint LTSC / perpetual Office**. Uses **in-process**
`PptPowerKeys.Core` (netstandard2.0). See [ADR-001](../../docs/adr/ADR-001-ltsc-windows-native-line.md).

> **Not** `PptPowerKeys.VstoLegacy*` — that tree is frozen historical scaffold.

## Build (Windows only)

Requirements:

- Windows 10/11
- Visual Studio 2022 with **Office/SharePoint development** workload (VSTO)
- PowerPoint 2019 / 2021 / LTSC 2024 (desktop)

Steps:

1. Open `src/PptPowerKeys.Windows.sln` in Visual Studio.
2. Restore/build — Core resolves as `netstandard2.0`.
3. Press F5 to debug-sideload into PowerPoint.
4. Ribbon tab **PowerKeys** shows six layout groups (32 commands), **Copy & Align** (4 commands), plus **Options** → snap checkbox.
5. Select 2+ shapes (anchor = last selected) → any layout button runs in-process Core (no HTTP).

## ServerLayout commands (S08-001)

`CommandRouter.Execute(CommandIds)` routes **all 32** `LayoutEngine.IsLayoutCommand` ids through the
in-process pipeline (no HTTP):

```
COM selection → ShapeBounds[] → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBounds
```

Anchor = **last** selected shape (unchanged from S07-003). `LayoutOptions.SnapToGrid` comes from
local `UserSettings.json` (S08-002). Non-layout / non-CopyAndAlign commands throw `NotSupportedException`.

### Routable commands (32)

**Alignment (12):** AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical,
AlignBottom, DistributeHorizontal, DistributeVertical, AlignLeftToRight, AlignRightToLeft,
AlignTopToBottom, AlignBottomToTop.

**Resize (20):** SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect,
WidthEqualsAnchorHeight, HeightEqualsAnchorWidth, StretchWidthToLeft, StretchWidthToRight,
StretchHeightToTop, StretchHeightToBottom, IncreaseWidthLarge, DecreaseWidthLarge,
IncreaseHeightLarge, DecreaseHeightLarge, IncreaseWidthSmall, DecreaseWidthSmall,
IncreaseHeightSmall, DecreaseHeightSmall, IncreaseSizeKeepAspect, DecreaseSizeKeepAspect.

Programmatic smoke (Immediate window / temporary debug hook):

```csharp
var router = Globals.ThisAddIn.CommandRouter;
var result = router.Execute(PptPowerKeys.Core.Commands.CommandIds.SameWidth);
// result.Changed, result.Message
```

## Copy-and-align HostScript (S08-004)

`CommandRouter.Execute(CommandIds)` routes **4** Copy-and-align commands through the host-script pipeline
(duplicate at source position → in-process Core layout → apply geometry on slide by id):

| CommandId | Layout step |
|-----------|-------------|
| CopyAndAlignLeft | AlignLeft |
| CopyAndAlignRight | AlignRight |
| CopyAndAlignTop | AlignTop |
| CopyAndAlignBottom | AlignBottom |

```
COM selection → ShapeBounds[] (originals)
  → COM Duplicate (clones at source Left/Top)
  → combined + anchorIndex = originals.Count - 1
  → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBoundsOnSlide
```

Parity with Web Add-in `runCopyAndAlign()` in `runCommand.ts`. Snap-to-grid (S08-002) applies via the same
`LayoutOptions` as ServerLayout commands.

Ribbon **PowerKeys** → **Copy & Align** (4 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute`.

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
var result = router.Execute(PptPowerKeys.Core.Commands.CommandIds.CopyAndAlignLeft);
```

### Manual QA — Copy-and-align

1. Open PowerPoint with the add-in loaded → **PowerKeys** tab → confirm **Copy & Align** group (4 buttons).
2. Draw 2+ rectangles at different positions; multi-select with **last** click = anchor.
3. Click **Copy + Left** — each shape is duplicated at its source position; clones align their left edges to the anchor's left edge.
4. Repeat for **Copy + Right**, **Copy + Top**, **Copy + Bottom** as needed.
5. Empty selection → error dialog: "Select one or more shapes first." (no crash).
6. With **Snap to grid** enabled (S08-002), aligned positions should land on 0.1 cm grid.

| Command | Setup | Expected |
|---------|-------|----------|
| **CopyAndAlignLeft** | 2+ shapes; anchor = last selected | Clones created; all clone left edges match anchor left |
| **CopyAndAlignRight** | 2+ shapes; anchor = last selected | Clones created; all clone right edges match anchor right |
| **CopyAndAlignTop** | 2+ shapes; anchor = last selected | Clones created; all clone top edges match anchor top |
| **CopyAndAlignBottom** | 2+ shapes; anchor = last selected | Clones created; all clone bottom edges match anchor bottom |

## Ribbon layout groups (S08-003)

All **32** ServerLayout commands are on the **PowerKeys** tab via a single callback `OnLayoutCommand` →
`CommandRouter.Execute(CommandIds)`. Control ids follow `btn{CommandIds}` (parsed by `RibbonCommandMap`).

| Group | Commands |
|-------|----------|
| **Alignment** | AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical, AlignBottom, DistributeHorizontal, DistributeVertical |
| **Stack** | AlignLeftToRight, AlignRightToLeft, AlignTopToBottom, AlignBottomToTop |
| **Size** | SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect, WidthEqualsAnchorHeight, HeightEqualsAnchorWidth |
| **Stretch** | StretchWidthToLeft, StretchWidthToRight, StretchHeightToTop, StretchHeightToBottom |
| **Nudge Large** | IncreaseWidthLarge, DecreaseWidthLarge, IncreaseHeightLarge, DecreaseHeightLarge, IncreaseSizeKeepAspect, DecreaseSizeKeepAspect |
| **Nudge Small** | IncreaseWidthSmall, DecreaseWidthSmall, IncreaseHeightSmall, DecreaseHeightSmall |
| **Options** | Snap to grid checkbox only (S08-002) |

### Manual QA — ribbon groups (≥1 command per group)

1. Open PowerPoint with the add-in loaded → **PowerKeys** tab; confirm **Bootstrap / Test** button is gone.
2. Draw 3 rectangles at different positions; multi-select with **last** click = anchor.
3. Run one command from each group (examples below); geometry updates in-process, no network.
4. Single-shape selection → no crash; command is a no-op with message in debug output.

| Group | Sample command | Expected |
|-------|----------------|----------|
| **Alignment** | **Left** | All left edges match anchor left |
| **Stack** | **To Right** | Each shape's left edge meets anchor's right edge |
| **Size** | **Same Width** | Non-anchor widths match anchor width |
| **Stretch** | **To Left** (Stretch width) | Width stretches toward anchor left |
| **Nudge Large** | **+ Width** | Width increases by large step |
| **Nudge Small** | **+ W** | Width increases by small step |
| **Options** | Toggle **Snap to grid** | Persists to `%AppData%\PptPowerKeys\UserSettings.json` |

## Snap-to-grid (S08-002)

`UserSettings.SnapToGrid` is persisted to **`%AppData%\PptPowerKeys\UserSettings.json`**
(Roaming profile — may sync on domain-joined machines). JSON uses camelCase (`snapToGrid`)
compatible with Web Add-in export/import v1.

Ribbon **PowerKeys** → **Options** → checkbox **Snap to grid (0.1 cm)** toggles the flag;
changes save immediately. All 32 ServerLayout commands receive
`LayoutOptions { SnapToGrid = settings.SnapToGrid }` via `CommandRouter` (grid step = default 0.1 cm).

### Manual QA — snap toggle

1. Open PowerPoint with the add-in loaded → **PowerKeys** tab.
2. Confirm checkbox **Snap to grid (0.1 cm)** is unchecked on first run (or matches prior session).
3. Check the box → run **Align Left** on 2+ shapes → left edges should land on 0.1 cm grid
   (use Format Shape position/size in cm to verify).
4. Uncheck → **Align Left** again → positions follow raw layout math (S08-001 behavior, no snap).
5. Check box, close PowerPoint, reopen → checkbox remains checked; `%AppData%\PptPowerKeys\UserSettings.json`
   contains `"snapToGrid": true`.

### Manual QA — snap geometry (snap ON)

| Command | Setup | Expected |
|---------|-------|----------|
| **AlignLeft** | 3 rectangles at different X; anchor = last selected | All left edges match anchor left **and** are multiples of 0.1 cm |
| **SameWidth** | 2+ shapes with different widths; anchor = widest | Non-anchor widths match anchor width; width/position values snap to 0.1 cm grid |

Steps:

1. Enable **Snap to grid (0.1 cm)** on the ribbon.
2. Draw shapes; multi-select with **last** click = anchor.
3. Run command (ribbon **Alignment** → **Left**, or **Size** → **Same Width**, or programmatic).
4. Inspect shape position/size in cm — values should be multiples of 0.1.

### Manual QA (Windows) — layout regression (snap OFF)

Regression minimum — three commands covering align, resize, and distribute:

| Command | Setup | Expected |
|---------|-------|----------|
| **AlignLeft** | 3 rectangles at different X; anchor = last selected | All left edges match anchor left |
| **SameWidth** | 2+ shapes with different widths; anchor = widest | Non-anchor widths match anchor width |
| **DistributeHorizontal** | 3+ shapes with uneven horizontal gaps; anchor unchanged | Equal gaps between shape bounds (anchor position fixed) |

Steps (each command):

1. Draw shapes on a slide; multi-select with **last** click = anchor.
2. Call `CommandRouter.Execute(CommandIds.<name>)` or use the matching ribbon button.
3. Confirm geometry updates in-process; no network activity.
4. Single-shape or empty selection → `LayoutResult.NoChange` with message (no crash).

### AlignLeft ribbon check (S07-003 / S08-003)

1. Draw three rectangles at different horizontal positions.
2. Multi-select: click first shapes, **last** click = anchor (rightmost or any anchor shape).
3. **PowerKeys** → **Alignment** → **Left** — all shapes' left edges match anchor's left edge.
4. Confirm no network calls (offline / air-gap safe for layout path).

## Solution layout

| Project | TFM | Role |
|---------|-----|------|
| `PptPowerKeys.Windows` | .NET Framework 4.8 VSTO | COM host, Ribbon, Task Pane (future) |
| `PptPowerKeys.Core` | netstandard2.0 (from this sln) | Shared business logic |

Root `PptPowerKeys.sln` (Linux CI) does **not** include this solution.

## Related docs

- [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)
- [`sprints/epic-ltsc-windows-native/ROADMAP.md`](../../sprints/epic-ltsc-windows-native/ROADMAP.md)

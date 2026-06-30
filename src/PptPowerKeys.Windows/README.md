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
4. Ribbon tab **PowerKeys** → **Test** shows bootstrap message with Core catalog count.
5. Select 2+ shapes (anchor = last selected) → **Align Left** aligns to anchor left edge (in-process Core, no HTTP).

## ServerLayout commands (S08-001)

`CommandRouter.Execute(CommandIds)` routes **all 32** `LayoutEngine.IsLayoutCommand` ids through the
in-process pipeline (no HTTP):

```
COM selection → ShapeBounds[] → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBounds
```

Anchor = **last** selected shape (unchanged from S07-003). `LayoutOptions.SnapToGrid` comes from
local `UserSettings.json` (S08-002). Non-layout commands throw `NotSupportedException`.

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
```

Ribbon exposes **Align Left** only until S08-003; other commands are testable via `CommandRouter` API.

## Snap-to-grid (S08-002)

`UserSettings.SnapToGrid` is persisted to **`%AppData%\PptPowerKeys\UserSettings.json`**
(Roaming profile — may sync on domain-joined machines). JSON uses camelCase (`snapToGrid`)
compatible with Web Add-in export/import v1.

Ribbon **PowerKeys** → **Layout** → checkbox **Snap to grid (0.1 cm)** toggles the flag;
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
3. Run command (ribbon **Align Left**, or `CommandRouter.Execute(CommandIds.SameWidth)` for SameWidth).
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
2. Call `CommandRouter.Execute(CommandIds.<name>)` (ribbon Align Left for AlignLeft, or programmatic for others).
3. Confirm geometry updates in-process; no network activity.
4. Single-shape or empty selection → `LayoutResult.NoChange` with message (no crash).

### AlignLeft ribbon check (S07-003)

1. Draw three rectangles at different horizontal positions.
2. Multi-select: click first shapes, **last** click = anchor (rightmost or any anchor shape).
3. **PowerKeys** → **Align Left** — all shapes' left edges match anchor's left edge.
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

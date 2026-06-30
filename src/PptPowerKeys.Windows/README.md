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

Anchor = **last** selected shape (unchanged from S07-003). `LayoutOptions` is `null` (snap-to-grid: S08-002).
Non-layout commands throw `NotSupportedException`.

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

### Manual QA (Windows)

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

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
4. Ribbon tab **PowerKeys** shows six layout groups (32 commands), **Position** (2 commands),
   **Copy & Align** (4 commands), plus **Options** → snap checkbox.
5. Select 2+ shapes (anchor = last selected) → any layout button runs in-process Core (no HTTP).

## Manual QA

Consolidated M2 test matrix (32 ServerLayout + snap + Copy-and-align + position clipboard):
[`docs/migration/06-windows-layout-qa.md`](../../docs/migration/06-windows-layout-qa.md).

## ServerLayout commands (S08-001)

`CommandRouter.Execute(CommandIds)` routes **all 32** `LayoutEngine.IsLayoutCommand` ids through the
in-process pipeline (no HTTP):

```
COM selection → ShapeBounds[] → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBounds
```

Anchor = **last** selected shape (unchanged from S07-003). `LayoutOptions.SnapToGrid` comes from
local `UserSettings.json` (S08-002). Non-layout / non-host-script commands throw `NotSupportedException`.

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

## Position clipboard (S08-005)

`CommandRouter.Execute(CommandIds)` routes **CopyObjectPosition** and **PasteObjectPosition** through an
in-memory session store (`PositionClipboardStore`) — Left/Top only, **not** persisted to disk.

| Command | Behavior |
|---------|----------|
| CopyObjectPosition | Read anchor (last selected) Left/Top → store → *"Copied position (X, Y)."* |
| PasteObjectPosition | Apply stored Left/Top to all selected shapes (width/height unchanged) |

Parity with Web Add-in `positionClipboard.ts` and `copyObjectPosition` / `pasteObjectPosition` in
`powerpoint.ts`.

Ribbon **PowerKeys** → **Position** (2 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute`.

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
router.Execute(PptPowerKeys.Core.Commands.CommandIds.CopyObjectPosition);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.PasteObjectPosition);
```

Copy → paste manual steps: see [06-windows-layout-qa.md](../../docs/migration/06-windows-layout-qa.md#copy--paste-flow-required-pr-manual-check).

## Ribbon layout groups (S08-003)

All **32** ServerLayout commands are on the **PowerKeys** tab via a single callback `OnLayoutCommand` →
`CommandRouter.Execute(CommandIds)`. Control ids follow `btn{CommandIds}` (parsed by `RibbonCommandMap`).

| Group | Commands |
|-------|----------|
| **Alignment** | AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical, AlignBottom, DistributeHorizontal, DistributeVertical |
| **Stack** | AlignLeftToRight, AlignRightToLeft, AlignTopToBottom, AlignBottomToTop |
| **Position** | CopyObjectPosition, PasteObjectPosition |
| **Size** | SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect, WidthEqualsAnchorHeight, HeightEqualsAnchorWidth |
| **Stretch** | StretchWidthToLeft, StretchWidthToRight, StretchHeightToTop, StretchHeightToBottom |
| **Nudge Large** | IncreaseWidthLarge, DecreaseWidthLarge, IncreaseHeightLarge, DecreaseHeightLarge, IncreaseSizeKeepAspect, DecreaseSizeKeepAspect |
| **Nudge Small** | IncreaseWidthSmall, DecreaseWidthSmall, IncreaseHeightSmall, DecreaseHeightSmall |
| **Copy & Align** | CopyAndAlignLeft, CopyAndAlignRight, CopyAndAlignTop, CopyAndAlignBottom |
| **Options** | Snap to grid checkbox only (S08-002) |

## Snap-to-grid (S08-002)

`UserSettings.SnapToGrid` is persisted to **`%AppData%\PptPowerKeys\UserSettings.json`**
(Roaming profile — may sync on domain-joined machines). JSON uses camelCase (`snapToGrid`)
compatible with Web Add-in export/import v1.

Ribbon **PowerKeys** → **Options** → checkbox **Snap to grid (0.1 cm)** toggles the flag;
changes save immediately. All 32 ServerLayout commands receive
`LayoutOptions { SnapToGrid = settings.SnapToGrid }` via `CommandRouter` (grid step = default 0.1 cm).

Snap manual QA: [06-windows-layout-qa.md](../../docs/migration/06-windows-layout-qa.md#snap-to-grid-regression).

## Solution layout

| Project | TFM | Role |
|---------|-----|------|
| `PptPowerKeys.Windows` | .NET Framework 4.8 VSTO | COM host, Ribbon, Task Pane (future) |
| `PptPowerKeys.Core` | netstandard2.0 (from this sln) | Shared business logic |

Root `PptPowerKeys.sln` (Linux CI) does **not** include this solution.

## Related docs

- [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)
- [`docs/migration/06-windows-layout-qa.md`](../../docs/migration/06-windows-layout-qa.md)
- [`sprints/epic-ltsc-windows-native/ROADMAP.md`](../../sprints/epic-ltsc-windows-native/ROADMAP.md)

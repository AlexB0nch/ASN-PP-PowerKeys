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
4. Ribbon tab **PowerKeys** shows six layout groups (32 commands), **Objects** (6 commands),
   **Duplicate** (4 commands), **Order** (6 commands), **Multi-slide** (2 commands), **Position** (2 commands), **Copy & Align** (4 commands), plus **Options** → snap checkbox.
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

## Insert shapes (S09-001)

`CommandRouter.Execute(CommandIds)` routes **6** insert-shape HostScript commands through COM on the active slide
(no selection required). Parity with Web Add-in `insertShape` / `insertTextBox` in `powerpoint.ts` and success
messages in `runCommand.ts`.

| CommandId | COM behavior | Success message |
|-----------|--------------|-----------------|
| InsertRectangle | `AddShape(msoShapeRectangle)` 100,100 150×100 pt | *Rectangle inserted.* |
| InsertSquare | `AddShape(msoShapeRectangle)` 100,100 100×100 pt | *Rectangle inserted.* |
| InsertEllipse | `AddShape(msoShapeOval)` 100,100 150×100 pt | *Ellipse inserted.* |
| InsertLine | `AddLine(100,150,250,150)` horizontal ~150 pt | *Line inserted.* |
| InsertTextbox | `AddTextbox` 100,100 200×80 pt | *Text box inserted.* |
| InsertArrow | line + `EndArrowheadStyle = msoArrowheadTriangle` | *Line inserted.* |

Ribbon **PowerKeys** → **Objects** (6 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute` → `ComHostAdapter.InsertShape`.

### Manual QA (Insert shapes)

1. Open a presentation in Normal view with at least one slide active.
2. For each **Objects** button (Rectangle, Square, Ellipse, Line, Textbox, Arrow):
   - Click the button with **no shapes selected**.
   - Confirm a new shape appears near the top-left at the documented size/position.
   - Confirm status message matches the table above (if your build surfaces `CommandExecutionResult.Message`).
3. **InsertArrow:** select the new line → confirm a **triangle end arrowhead** is visible (COM advantage over Web partial).
4. Switch to Slide Sorter (no active slide in Normal view) → click any Insert button → expect a clear error
   (*No active slide…*).

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
var result = router.Execute(PptPowerKeys.Core.Commands.CommandIds.InsertRectangle);
// result.Changed == true, result.Message == "Rectangle inserted."
```

## Smart Duplicate (S09-002)

`CommandRouter.Execute(CommandIds)` routes **4** smart-duplicate HostScript commands through COM clone +
in-process `DuplicationEngine.ComputeDuplicate` with per-direction gap memory (`DuplicateGapStore`).

| CommandId | Behavior |
|-----------|----------|
| DuplicateRight / Left / Up / Down | COM `Duplicate()` per selected shape → set clone `Left`/`Top` from Core offset |

Gap memory (parity with Web `duplicateGapMemory.ts` / S05-005):

- First duplicate in a direction → `gap = 0` (touching).
- Repeat duplicate of the same `CommandId` → uses remembered gap.
- In-memory session scope — **not** persisted to disk.

Ribbon **PowerKeys** → **Duplicate** (4 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute`.

### Manual QA (Smart Duplicate)

1. Select one or more shapes on the active slide.
2. Click **Duplicate → Right** twice → second clone is the same offset as the first (touching, gap 0).
3. Manually move the first clone to create a visible gap, then duplicate right again — gap memory still uses
   the **requested** gap (0 unless a prior command stored a non-zero gap); status shows `(gap X pt)` when gap > 0.
4. Switch direction (**Duplicate → Down**) → first down duplicate uses gap 0 independently of right.
5. Empty selection → clear error (*Select one or more shapes first.*).

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
var result = router.Execute(PptPowerKeys.Core.Commands.CommandIds.DuplicateRight);
// result.Changed == true, result.Message starts with "Duplicated"
```

## Group / Ungroup / Z-order (S09-003)

`CommandRouter.Execute(CommandIds)` routes **6** group/z-order HostScript commands through COM on the
current shape selection. Parity with Web Add-in `groupSelectedShapes`, `ungroupSelectedShape`, `setZOrder`
in `powerpoint.ts` and success messages in `runCommand.ts`.

| CommandId | COM behavior | Success message |
|-----------|--------------|-----------------|
| Group | `ShapeRange.Group()` (≥2 shapes) | *Grouped N shape(s).* |
| Ungroup | `Shape.Ungroup()` (exactly 1 group) | *Ungrouped.* |
| BringToFront | `ZOrder(msoBringToFront)` per shape | *Brought to front.* |
| SendToBack | `ZOrder(msoSendToBack)` per shape | *Sent to back.* |
| BringForward | `ZOrder(msoBringForward)` per shape | *Brought forward.* |
| SendBackward | `ZOrder(msoSendBackward)` per shape | *Sent backward.* |

Validation errors match Web `powerpoint.ts`:

- Group with &lt;2 shapes → *Select at least two shapes to group.*
- Ungroup with ≠1 selection → *Select exactly one group to ungroup.*
- Ungroup non-group → *Selected shape is not a group.*
- Z-order with empty selection → *Select one or more shapes first.*

Ribbon **PowerKeys** → **Order** (6 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute` → `ComHostAdapter.GroupSelectedShapes` / `UngroupSelectedShape` / `ApplyZOrderToSelection`.

### Manual QA (Group / Ungroup / Z-order)

1. Insert two rectangles (**Objects**). Select both → **Order → Group** → shapes become one group;
   status *Grouped 2 shape(s).*
2. Select the group → **Order → Ungroup** → status *Ungrouped.*; child shapes are separate again.
3. Select a single rectangle (not a group) → **Ungroup** → error *Selected shape is not a group.*
4. Select only one shape → **Group** → error *Select at least two shapes to group.*
5. Overlap two shapes. Select both → **Bring to front** / **Send to back** / **Forward** / **Backward** —
   confirm stacking order changes; status messages match the table above.
6. Clear selection → any z-order button → error *Select one or more shapes first.*

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
router.Execute(PptPowerKeys.Core.Commands.CommandIds.Group);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.BringToFront);
```

## Multi-slide paste / remove shapes (S09-004)

`CommandRouter.Execute(CommandIds)` routes **2** multi-slide shape HostScript commands through COM copy/paste
and delete-by-name. Parity with Web Add-in `pasteShapeToSelectedSlides`, `removeShapeFromSelectedSlides` in
`powerpoint.ts` and success messages in `runCommand.ts`.

| CommandId | COM behavior | Success message |
|-----------|--------------|-----------------|
| PasteShapeToSelectedSlides | `Shape.Copy()` + `Slide.Shapes.Paste()` per target slide (≥2 slides, skip source slide, same Left/Top/Width/Height) | *Pasted shape to N slide(s).* |
| RemoveShapeFromSelectedSlides | Delete shapes by exact `Name` on each selected slide (iterate shapes backwards) | *Removed X shape(s) from Y slide(s).* |

Validation errors match Web `powerpoint.ts`:

- Paste with &lt;2 slides → *Select two or more slides first.*
- Remove with 0 slides → *Select one or more slides first.*
- Either command with ≠1 shape on active slide → *Select exactly one shape on the active slide first.*
- Remove with empty shape name → *Selected shape has no name. Name the shape first.*

Ribbon **PowerKeys** → **Multi-slide** (2 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute` → `ComHostAdapter.PasteShapeToSelectedSlides` / `RemoveShapeFromSelectedSlides`.

### Manual QA (Multi-slide paste / remove)

1. Create a deck with at least 3 slides. On slide 1, insert a rectangle and name it `Logo` (Selection Pane).
2. **Paste shape:** select the rectangle on slide 1, then multi-select slides 1–3 in the thumbnail pane
   (Ctrl+click). Click **Multi-slide → Paste Shape** → rectangle appears on slides 2 and 3 at the same
   position/size; slide 1 unchanged; status *Pasted shape to 2 slide(s).*
3. Select only 1 slide → **Paste Shape** → error *Select two or more slides first.*
4. Multi-select 2+ slides with no shape selected → **Paste Shape** → error *Select exactly one shape on the active slide first.*
5. **Remove shape:** on slide 1 select the named `Logo` shape; multi-select slides 1–3 → **Multi-slide → Remove Shape**
   → all `Logo` shapes deleted across slides; status *Removed N shape(s) from 3 slide(s).*
6. Select a shape with no name → **Remove Shape** → error *Selected shape has no name. Name the shape first.*
7. Multi-select slides with no shape selected → **Remove Shape** → error *Select exactly one shape on the active slide first.*

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
router.Execute(PptPowerKeys.Core.Commands.CommandIds.PasteShapeToSelectedSlides);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.RemoveShapeFromSelectedSlides);
```

### Manual QA (Format colors)

1. Open a presentation with a themed Slide Master. Insert a rectangle and select it.
2. **Fill color:** click **Format → Fill** → fill changes to first theme/recent palette color; status
   *Fill color #RRGGBB applied to 1 shape(s).* Repeat click → cycles palette; recent colors persist in
   `%AppData%\PptPowerKeys\UserSettings.json` (`recentColors` array).
3. **Line color:** select shape → **Format → Line** → outline color cycles; status *Line color …*
4. **Text color:** insert text box, type text, select → **Format → Text Color** → font color cycles.
   Shape without text → error *Selected shape(s) have no text to color.*
5. No selection → Fill/Line/Text → *Select one or more shapes first.*
6. **Toggle black/white** (programmatic / future shortcut): black fill → white; colored fill → black.

Ribbon **PowerKeys** → **Format** (3 buttons) → `OnHostScriptCommand` → `HostScriptCommandMap` →
`CommandRouter.Execute` → `ComHostAdapter` + `ColorPaletteBuilder` + `FormatColorCycleStore`.

Programmatic smoke:

```csharp
var router = Globals.ThisAddIn.CommandRouter;
router.Execute(PptPowerKeys.Core.Commands.CommandIds.FillColor);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.LineColor);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.TextColor);
router.Execute(PptPowerKeys.Core.Commands.CommandIds.ToggleFillBlackWhite);
```

## Ribbon layout groups (S08-003)

All **32** ServerLayout commands are on the **PowerKeys** tab via a single callback `OnLayoutCommand` →
`CommandRouter.Execute(CommandIds)`. Control ids follow `btn{CommandIds}` (parsed by `RibbonCommandMap`).

| Group | Commands |
|-------|----------|
| **Alignment** | AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical, AlignBottom, DistributeHorizontal, DistributeVertical |
| **Stack** | AlignLeftToRight, AlignRightToLeft, AlignTopToBottom, AlignBottomToTop |
| **Objects** | InsertRectangle, InsertSquare, InsertEllipse, InsertLine, InsertTextbox, InsertArrow |
| **Duplicate** | DuplicateRight, DuplicateLeft, DuplicateDown, DuplicateUp |
| **Order** | BringToFront, SendToBack, BringForward, SendBackward, Group, Ungroup |
| **Multi-slide** | PasteShapeToSelectedSlides, RemoveShapeFromSelectedSlides |
| **Format** | FillColor, LineColor, TextColor |
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

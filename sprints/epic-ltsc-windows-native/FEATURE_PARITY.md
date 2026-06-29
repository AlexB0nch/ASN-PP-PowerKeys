# Feature Parity Plan — Web Add-in → PptPowerKeys.Windows

> Source of truth каталога: `src/PptPowerKeys.Core/Commands/CommandCatalog.cs` (79 команд).  
> HostScript spec (Office.js): `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`, `runCommand.ts`.

## Классификация

| Class | Meaning |
|-------|---------|
| **direct** | Core in-process; COM read/write ShapeBounds only |
| **rewrite** | New COM implementation; logic may call Core helpers |
| **unlock** | `OfficeJsSupport.None` on Web → full COM on Windows line |
| **settings** | Native task pane / ribbon |

---

## Alignment (18)

| CommandId | Class | Web source | Windows implementation |
|-----------|-------|------------|------------------------|
| AlignLeft … DistributeVertical (8) | direct | LayoutEngine via API | Core.LayoutEngine in-process |
| AlignLeftToRight … AlignBottomToTop (4) | direct | LayoutEngine | Core in-process |
| CopyAndAlignLeft … CopyAndAlignBottom (4) | rewrite | clone + layout API | COM duplicate + Core layout |
| CopyObjectPosition | rewrite | in-memory + Office.js | in-memory + COM Left/Top |
| PasteObjectPosition | rewrite | in-memory + Office.js | in-memory + COM Left/Top |

---

## Resize (20) — all **direct** (ServerLayout)

SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect, WidthEqualsAnchorHeight, HeightEqualsAnchorWidth, StretchWidthToLeft/Right, StretchHeightToTop/Bottom, Increase/Decrease Width/Height Large/Small, Increase/DecreaseSizeKeepAspect.

---

## Objects (19)

| CommandId | Class | Notes |
|-----------|-------|-------|
| InsertRectangle, InsertSquare, InsertEllipse, InsertLine, InsertTextbox | rewrite | COM Shapes.Add* |
| InsertArrow | rewrite | Partial on Web |
| DuplicateRight/Left/Up/Down | rewrite | Core DuplicationEngine + COM clone |
| Group | rewrite | COM Group |
| Ungroup | rewrite | COM Ungroup |
| **Regroup** | **unlock** | Web None |
| BringToFront, SendToBack, BringForward, SendBackward | rewrite | COM ZOrder |
| PasteShapeToSelectedSlides | rewrite | Multi-slide COM |
| RemoveShapeFromSelectedSlides | rewrite | By name match COM |

---

## Format (5)

| CommandId | Class | Notes |
|-----------|-------|-------|
| FillColor, LineColor, TextColor | rewrite | Theme + recent; COM format |
| ToggleFillBlackWhite | rewrite | COM fill |
| **FormatPainter** | **unlock** | Web None; COM PickUp/Apply |

---

## Text (6)

| CommandId | Class | Notes |
|-----------|-------|-------|
| PasteUnformatted | rewrite | Clipboard COM |
| **PasteFormatted** | **unlock** | Web None |
| AddupTextFields | rewrite | Core NumberAggregator + COM text read |
| ReplaceWithEllipsis | rewrite | COM text |
| ToggleSuperscript, ToggleSubscript | rewrite | COM font |

---

## Slides (8)

| CommandId | Class | Notes |
|-----------|-------|-------|
| **ToggleZoom** | **unlock** | Web None |
| **ToggleSlideSorter** | **unlock** | Web None |
| **StartSlideShow** | **unlock** | Web None |
| **ToggleGrid** | **unlock** | Web None |
| **ToggleGuides** | **unlock** | Web None |
| **PrintSlide** | **unlock** | Web None |
| CopySlide | rewrite | COM duplicate slide |
| MoveSlidesToBackup | rewrite | COM move slides to deck end |

---

## Settings (3) — **settings**

OpenShortcutManager, OpenColorScheme, ResetToDefaults — WPF task pane panels; `UserSettings` JSON compatible with Web.

---

## Sprint mapping

| Sprint | Commands |
|--------|----------|
| S08 | Alignment (18) + Resize (20) = 38 layout-related |
| S09 | Objects (19) + Format (5) + Text (6) = 30 |
| S10 | Slides (8 incl. 6 unlock) + Settings (3) + integration |
| S11 | ShortcutManager binds all 76 hotkey-eligible (79 − 3 Settings) |

---

## Shared reuse

| Asset | Reuse |
|-------|-------|
| `PptPowerKeys.Core/*` | Layout, duplication, addup, palette, settings, catalog |
| `CommandCatalog` | Metadata + feasibility flags |
| `AddIn/src/office/powerpoint.ts` | COM rewrite specification |
| `VstoLegacy/UI/RibbonTab.xml` | Ribbon layout reference only |
| `PptPowerKeys.Api` | Optional; skip for air-gap LTSC |

---

## Summary counts

| Class | Count |
|-------|-------|
| direct | 32 |
| rewrite | 38 |
| unlock | 9 |
| settings | 3 |
| **Total** | **79** (+ Settings execution kind overlap) |

# Windows layout manual QA (Sprint 08 / M2)

Consolidated manual test matrix for **PptPowerKeys.Windows** layout beta: 32 ServerLayout commands,
snap-to-grid, 4 Copy-and-align, and 2 position-clipboard commands.

Product line: **LTSC / perpetual Office** (Product Line B). See
[`04-powerpoint-ltsc-windows-native.md`](04-powerpoint-ltsc-windows-native.md).

## Prerequisites

| Requirement | Notes |
|-------------|-------|
| **OS** | Windows 10/11 |
| **IDE** | Visual Studio 2022 with **Office/SharePoint development** workload (VSTO) |
| **PowerPoint** | Desktop 2019 / 2021 / LTSC 2024 |
| **Sideload** | Open `src/PptPowerKeys.Windows.sln` → F5 debug-sideload |
| **Ribbon** | **PowerKeys** tab visible with layout groups, Copy & Align, Position, Options |

**Anchor rule (all layout commands):** multi-select shapes; the **last** clicked shape is the anchor.

**Snap-to-grid:** when enabled, geometry snaps to a **0.1 cm** grid (Consulting Mode). Toggle via
**PowerKeys** → **Options** → **Snap to grid (0.1 cm)**. Persists to
`%AppData%\PptPowerKeys\UserSettings.json`.

## M2 scope summary

| Category | Count | Ribbon handler |
|----------|-------|----------------|
| ServerLayout | 32 | `OnLayoutCommand` |
| Copy-and-align | 4 | `OnHostScriptCommand` |
| Position clipboard | 2 | `OnHostScriptCommand` |
| Snap toggle | 1 | `OnSnapToGridToggle` |

Pipeline (ServerLayout):

```
COM selection → ShapeBounds[] → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBounds
```

## ServerLayout — ribbon groups (32 commands)

Spot-check **≥1 command per group** with 2–3 rectangles; anchor = last selected.

| Group | Sample command | Setup | Expected |
|-------|----------------|-------|----------|
| **Alignment** | AlignLeft | 3 shapes at different X | All left edges match anchor left |
| **Stack** | AlignLeftToRight | 2+ shapes | Each shape's left edge meets anchor's right edge |
| **Size** | SameWidth | 2+ shapes, different widths | Non-anchor widths match anchor width |
| **Stretch** | StretchWidthToLeft | 2+ shapes | Width stretches toward anchor left |
| **Nudge Large** | IncreaseWidthLarge | 1+ shapes | Width increases by large step |
| **Nudge Small** | IncreaseWidthSmall | 1+ shapes | Width increases by small step |

### ServerLayout regression (snap OFF)

Minimum three commands covering align, resize, distribute:

| Command | Setup | Expected |
|---------|-------|----------|
| **AlignLeft** | 3 rectangles at different X; anchor = last | All left edges match anchor left |
| **SameWidth** | 2+ shapes with different widths; anchor = widest | Non-anchor widths match anchor width |
| **DistributeHorizontal** | 3+ shapes, uneven gaps; anchor fixed | Equal horizontal gaps between bounds |

**Empty / single selection:** no crash; no-op with message in debug output.

## Snap-to-grid regression

| Check | Steps | Expected |
|-------|-------|----------|
| Toggle persistence | Enable snap → close/reopen PowerPoint | Checkbox remains checked; `UserSettings.json` has `"snapToGrid": true` |
| Snap ON | Enable snap → **Align Left** on 2+ shapes | Left edges on 0.1 cm grid (verify in Format Shape, cm) |
| Snap OFF | Disable snap → **Align Left** again | Raw layout math, no grid snap |
| Snap + resize | Enable snap → **Same Width** | Width/position values snap to 0.1 cm grid |

## Copy-and-align (4 commands)

Pipeline: duplicate at source position → Core layout → apply on slide by id.

| Command | Setup | Expected |
|---------|-------|----------|
| **CopyAndAlignLeft** | 2+ shapes; anchor = last | Clones created; clone left edges match anchor left |
| **CopyAndAlignRight** | 2+ shapes; anchor = last | Clones created; clone right edges match anchor right |
| **CopyAndAlignTop** | 2+ shapes; anchor = last | Clones created; clone top edges match anchor top |
| **CopyAndAlignBottom** | 2+ shapes; anchor = last | Clones created; clone bottom edges match anchor bottom |

**Empty selection:** error dialog *"Select one or more shapes first."* (no crash).

With snap enabled, aligned positions should land on the 0.1 cm grid.

## Position clipboard (2 commands)

In-memory session store (Left/Top only; **not** persisted). Parity with Web Add-in
`positionClipboard.ts`.

| Command | Setup | Expected |
|---------|-------|----------|
| **CopyObjectPosition** | 1+ shapes; anchor = last | Message *"Copied position (X, Y)."*; anchor Left/Top stored |
| **PasteObjectPosition** | Prior copy; 1+ shapes selected | All selected shapes move to copied Left/Top; **width/height unchanged** |
| **PasteObjectPosition** (no prior copy) | No copy yet | Error *"Copy a position first (Copy object position)."* |
| **CopyObjectPosition** (empty) | No selection | Error *"Select a shape first."* |

### Copy → paste flow (required PR manual check)

1. Draw **Shape A** (anchor) and **Shape B** at different positions.
2. Select **Shape A** last → **PowerKeys** → **Position** → **Copy Position**.
3. Select **Shape B** (alone or with others) → **Paste Position**.
4. Confirm **Shape B** Left/Top matches **Shape A**; **Shape B** width/height unchanged.
5. Restart PowerPoint session → paste without copy → error (clipboard is session-only, not disk).

## Offline / air-gap

All layout and host-script commands run **in-process** (no HTTP). Verify with network disabled:
ribbon layout buttons and Copy/Paste position still work.

## Related

- [`src/PptPowerKeys.Windows/README.md`](../../src/PptPowerKeys.Windows/README.md) — build and programmatic smoke
- Web reference: `src/PptPowerKeys.AddIn/src/office/positionClipboard.ts`, `runCommand.ts`

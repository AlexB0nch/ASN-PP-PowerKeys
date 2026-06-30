# S09-005 — Format colors + palette (4 HostScript commands + palette infra)

> Передача builder'у: `/builder выполни S09-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-005` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S09-001…004 Done |

## Цель

Реализовать **Format color commands** на Windows line — 4 HostScript команды + инфраструктура smart palette (parity Web Sprint 04).

| CommandId | Поведение | Success message (match Web) |
|-----------|-----------|----------------------------|
| FillColor | Cycle palette → COM solid fill | `Fill color #RRGGBB applied to N shape(s).` |
| LineColor | Cycle palette → COM line color + visible | `Line color #RRGGBB applied to N shape(s).` |
| TextColor | Cycle palette → COM font color (text frames only) | `Text color #RRGGBB applied to N shape(s).` |
| ToggleFillBlackWhite | Toggle fill near-black ↔ white per shape | `Toggled fill on N shape(s).` |

**FormatPainter — anti-scope** (Web None → S10 unlock).  
**OpenColorScheme** — Settings UI, S10 (не ColorDialog на ribbon FillColor).

## Контекст (после S09-001…004)

| Компонент | Ожидаемое состояние |
|-----------|---------------------|
| `CommandRouter` | Objects HostScript S09-001…004 + layout (38) |
| `WindowsUserSettingsStore` | `%AppData%/PptPowerKeys/UserSettings.json` (snap, addup mode) |
| Web color UX | `runPaletteColorCommand` — **cycle**, не modal picker |
| Core | `ColorPaletteBuilder` (theme ≤10 + recent ≤5), `ThemeColor` |
| Web theme read | `themeColors.ts` — accent1–6 + dark1/2 + light1/2 |
| Legacy ribbon | `VstoLegacy/UI/RibbonTab.xml` → `grpFormat` (4 кнопки, без FormatPainter) |

## Алгоритм (match Web)

### FillColor / LineColor / TextColor — palette cycle

```
1. shapeIds = ReadSelectedShapeIds(); empty → "Select one or more shapes first."
2. themeColors = ReadPresentationThemeColors()  // COM Slide Master (full on desktop)
3. recentColors = RecentColorsStore.Get()
4. palette = ColorPaletteBuilder.Build(themeColors, recentColors, DefaultFallbackPalette)
5. hex = FormatColorCycleState.NextColor(command, shapeIds, palette)
6. count = ApplyFillColor(hex) | ApplyLineColor(hex) | ApplyTextColor(hex)
7. RecentColorsStore.Record(hex)  // FIFO max 5, dedupe, persist AppData
8. message: "Fill|Line|Text color {hex} applied to {count} shape(s)."
```

**Cycle semantics** (mirror `formatColorState.ts`):

- Отдельный cycle state для каждой команды (`FillColor`, `LineColor`, `TextColor`).
- Fingerprint selection = shape ids joined `\u001f` (selection order).
- При смене fingerprint → index = 0; при повторном нажатии с тем же selection → index++ (mod palette length).

### ToggleFillBlackWhite

```
1. shapes = selected; empty → "Select one or more shapes first."
2. for each shape:
     currentFillHex = ReadFillColor(shape)
     next = isNearBlack(currentFillHex) ? "#FFFFFF" : "#000000"
     ApplyFillColor(shape, next)
3. message: "Toggled fill on {count} shape(s)."
```

**isNearBlack:** RGB each `< 48` (match Web `powerpoint.ts`).

### Theme colors (COM — desktop full parity)

Read from active presentation **Slide Master** `Theme.ThemeColorScheme`:

| Slot | MsoThemeColorSchemeIndex |
|------|--------------------------|
| dark1, light1, dark2, light2 | msoThemeDark1 … msoThemeLight2 |
| accent1 … accent6 | msoThemeAccent1 … msoThemeAccent6 |

Convert each `Colors(index).RGB` → uppercase `#RRGGBB` via `ThemeColor` helpers.  
Fallback: `DefaultFallbackPalette` = Web `DEFAULT_PALETTE` from `formatColorState.ts` (10 hex).

Refresh theme on each color command (cheap) or cache per presentation session — builder choice; architect: **refresh per command** for MVP simplicity.

### Recent colors persistence

| Aspect | Decision |
|--------|----------|
| Storage | `%AppData%/PptPowerKeys/RecentColors.json` |
| Shape | `{ "recentColors": ["#RRGGBB", ...] }` (max 5, newest first) |
| Scope | Windows line only in S09; Web uses separate localStorage key |
| Record | After successful Fill/Line/Text apply (not Toggle B/W) |

**Не** расширять `UserSettings` JSON в S09 (Settings UI import/export — S10).

## Решения architect

### FormatColorCommands helper

New file `Host/FormatColorCommands.cs`:

```csharp
public static class FormatColorCommands
{
    public static bool IsFormatColorCommand(CommandIds command) => command switch
    {
        CommandIds.FillColor or CommandIds.LineColor or CommandIds.TextColor
            or CommandIds.ToggleFillBlackWhite => true,
        _ => false,
    };

    public static bool IsPaletteCycleCommand(CommandIds command) => ...
}
```

### Supporting classes (Windows project)

| Class | Role |
|-------|------|
| `FormatColorCycleState.cs` | Per-command cycle + selection fingerprint |
| `RecentColorsStore.cs` | Load/save/record recent hex list |
| `PresentationThemeReader.cs` | COM → `IReadOnlyList<string>` theme hex |
| `ColorRgbConverter.cs` (optional) | OLE RGB ↔ `#RRGGBB` |

**Core in-process:** `ColorPaletteBuilder.Build(...)` — **без** Api HTTP (air-gap LTSC).

### CommandRouter

```csharp
if (FormatColorCommands.IsFormatColorCommand(command))
    return ExecuteFormatColor(command);
```

### ComHostAdapter extensions

| Method | COM behavior |
|--------|--------------|
| `IReadOnlyList<string> ReadSelectedShapeIds()` | Selection order ids (reuse `ShapeBoundsId.FromComShape`) |
| `int ApplyFillColor(string hex)` | `Fill.Solid()` + `ForeColor.RGB` |
| `int ApplyLineColor(string hex)` | `Line.ForeColor.RGB`, `Line.Visible = msoTrue` |
| `int ApplyTextColor(string hex)` | `TextFrame.TextRange.Font.Color.RGB`; skip shapes without text frame; error if 0 applied: «Selected shape(s) have no text to color.»,
| `int ToggleFillBlackWhite()` | Per-shape read fill → toggle |
| `string ReadShapeFillHex(Shape)` | For toggle logic |

### HostScriptCommandMap + Ribbon

Extend `TryParse` for `FormatColorCommands.IsFormatColorCommand`.

Add **`grpFormat`** (reference legacy, **без** FormatPainter):

| Control id | Label | imageMso |
|------------|-------|----------|
| `btnFillColor` | Fill | ShapeFillColorPicker |
| `btnLineColor` | Line | OutlineColorPicker |
| `btnTextColor` | Text Color | FontColorPicker |
| `btnToggleFillBlackWhite` | B/W Fill | (ShapeFill toggle icon) |

All `onAction="OnHostScriptCommand"`.

Update `PptPowerKeys.Windows/README.md` — Format section + manual QA (cycle 3× same selection, recent persist restart).

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/FormatColorCommands.cs` | Command guards |
| `Host/FormatColorCycleState.cs` | Palette cycle logic |
| `Host/RecentColorsStore.cs` | JSON persist |
| `Host/PresentationThemeReader.cs` | COM theme → hex list |
| `Host/CommandRouter.cs` | Format color handlers |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | Apply fill/line/text/toggle |
| `UI/HostScriptCommandMap.cs` | Include 4 commands |
| `UI/RibbonTab.xml` | `grpFormat` 4 buttons |
| `PptPowerKeys.Windows/README.md` | Docs + QA |
| `PptPowerKeys.Tests/FormatColorCycleStateTests.cs` | Cycle + fingerprint |
| `PptPowerKeys.Tests/RecentColorsStoreTests.cs` (optional) | FIFO/dedupe if testable without COM |

## Анти-scope

- **FormatPainter** (S10)
- **OpenColorScheme** / WPF Color Picker panel (S10 Settings)
- Modal **ColorDialog** on ribbon FillColor (Web cycles palette; picker = Settings)
- Api `/api/colors/build-palette` calls from Windows line
- Core schema change to `UserSettings` for recent colors
- Text commands (S09-006)
- Eyedropper / custom HEX input UI

## Критерии приёмки

- [ ] Fill/Line/Text cycle palette on repeat press with same selection
- [ ] Cycle resets when selection changes (fingerprint)
- [ ] Theme colors read from Slide Master via COM (fallback to default 10)
- [ ] Recent colors persist in `%AppData%/PptPowerKeys/RecentColors.json` across restarts
- [ ] `ColorPaletteBuilder` used in-process (Core)
- [ ] ToggleFillBlackWhite: near-black ↔ white per shape
- [ ] TextColor error when no text frames: match Web message
- [ ] Success messages match Web format
- [ ] Ribbon 4 buttons wired
- [ ] `dotnet test PptPowerKeys.sln` green (cycle/recent unit tests)
- [ ] Manual QA in PR (theme deck + cycle + restart recent)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S09-001…004 Done
- Core `ColorPaletteBuilder` + tests (S04-001, in main)
- Web reference: Sprint 04 (`S04-001…003`)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — `runPaletteColorCommand`, ToggleFillBlackWhite
- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts` — cycle, recent, DEFAULT_PALETTE
- `src/PptPowerKeys.AddIn/src/office/themeColors.ts` — theme slots
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — applyFillColor, applyLineColor, applyTextColor, toggleFillBlackWhite
- `src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs`
- `sprints/sprint-04-smart-color-picker/tasks/S04-001-theme-colors-from-presentation.md`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpFormat`

## Трассировка

Issue `#N` → `cursor/S09-005-format-colors-*` → PR `Closes #N`

## Copy-paste промпт (новая сессия `/architect`)

```
/architect

Sprint 09 — S09-005 Format colors + palette (4 HostScript commands).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-005-format-colors-palette.md
- sprints/sprint-04-smart-color-picker/tasks/S04-001-theme-colors-from-presentation.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- sprints/sprint-09-ltsc-objects-format-text/backlog.md
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs
- src/PptPowerKeys.Windows/UI/HostScriptCommandMap.cs
- src/PptPowerKeys.Windows/Settings/WindowsUserSettingsStore.cs (pattern)
- src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs
- src/PptPowerKeys.AddIn/src/office/formatColorState.ts
- src/PptPowerKeys.AddIn/src/office/themeColors.ts
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (applyFillColor, toggleFillBlackWhite)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (FillColor, LineColor, TextColor, ToggleFillBlackWhite)
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpFormat)

S09-001…004 Done. Issue S09-005 → backlog In Progress → /builder выполни S09-005 → приёмка → merge.
FormatPainter/OpenColorScheme — anti-scope (S10). После merge: backlog Done, PRODUCT_CONTEXT journal (S09-005).
```

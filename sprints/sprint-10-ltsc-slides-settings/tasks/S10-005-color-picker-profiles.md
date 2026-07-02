# S10-005 — Color picker panel COM + consulting presets (последняя задача Sprint 10)

> Передача builder'у: `/builder выполни S10-005`  
> **Последняя задача Sprint 10** — после merge **architect** закрывает спринт (не builder).

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-005` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (reuse) |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S10-004 Done (PR #100) |

## Цель

Заменить placeholder на вкладке **Colors** в WPF Settings pane полноценным **Smart Color Picker** —
parity Web `ColorPickerPanel.tsx`, с COM theme colors и recent colors из `UserSettings`.

| Возможность | Web reference | Windows (S10-005) |
|-------------|---------------|-------------------|
| Theme swatches | `readPresentationThemeColors` | `ComHostAdapter.ReadPresentationThemeColors()` |
| Recent swatches | `getRecentColors` / UserSettings | `WindowsUserSettingsStore.GetRecentColors()` |
| Merged palette | `ColorPaletteBuilder` via Api | Core in-process `ColorPaletteBuilder.Build` |
| Apply Fill/Line/Text | `applyFillColor` / `applyLineColor` / `applyTextColor` | `ComHostAdapter.Apply*` (reuse) |
| Pick from shape | `readColorFromSelection` | **New** `ComHostAdapter.ReadColorFromSelection` |
| Custom HEX | S06-005 HEX input | WPF TextBox + `ThemeColor.IsValidHex` |
| OpenColorScheme | Opens picker tab | `ITaskPaneService.ShowColorPicker()` (replace placeholder) |
| Consulting presets | McKinsey `Alt+L` → OpenColorScheme | Picker opens; fallback = `DefaultColorPalette` |

**Не новые CommandIds** — только UI + COM wiring. **79/79** routed остаётся (S10-004).

## Контекст (после S10-004)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | **79/79** commands routed |
| Settings pane | `SettingsPane.xaml` — **Colors** tab = placeholder «S10-005» |
| `OpenColorScheme` | `TaskPaneService.ShowColorsPlaceholder()` |
| Format color cycle | `ExecuteFormatColor` — theme + recent + `ColorPaletteBuilder` (reuse logic) |
| Ribbon | `btnOpenColorScheme` → `SettingsCommandMap` (`btnColorScheme` alias) |
| Tests | **316** dotnet tests; `CommandRouterRoutabilityTests` |

## UX parity (match Web `ColorPickerPanel.tsx`)

### Layout (Colors tab)

```
Smart Color Picker
[Warning if theme fallback]

Theme colors     — swatch grid (≤10, from Slide Master or DefaultColorPalette)
Recent           — swatch grid (≤5, from UserSettings.recentColors; hide if empty)

Custom HEX       — TextBox + Set button; live validation (#RRGGBB)

Pick from shape  — [Fill] [Line] [Text] → ReadColorFromSelection(first selected)

Apply            — [Apply Fill] [Apply Line] [Apply Text]
Refresh palette  — reload theme from active presentation (optional button or on tab show)
```

### Selection & apply flow

```
1. User selects swatch OR enters valid HEX OR picks from shape
2. selectedColor = normalized #RRGGBB (ThemeColor.NormalizeHex)
3. Apply Fill|Line|Text:
     shapes = ReadSelectedShapeBounds(); empty → "Select one or more shapes first."
     count = ApplyFillColor|LineColor|TextColor(selectedColor)
     RecordRecentColor(selectedColor)
     message: "Fill|Line|Text color #RRGGBB applied to N shape(s)."
4. Refresh recent swatches in UI
```

### Pick from shape (match Web messages)

| Step | Behavior |
|------|----------|
| No selection | `Select a shape first.` |
| No text frame (text pick) | `Selected shape has no text frame.` |
| Empty color | `No fill|line|text color on selected shape.` |
| Success | Set selected swatch + `RecordRecentColor` + feedback |

### Theme fallback

If `ReadPresentationThemeColors()` empty/throws → use `DefaultColorPalette.FallbackTheme` + show warning:
`Theme colors unavailable — using default palette.` (match Web MessageBar).

### Consulting presets (scope clarification)

- **Не** отдельные McKinsey/BCG color grids (Web их не имеет в picker).
- **Да:** fallback palette = consulting-deck-like defaults (`DefaultColorPalette`).
- **Да:** McKinsey profile shortcut `Alt+L` → `OpenColorScheme` открывает этот picker (S10-004 profiles уже работают).

## Решения architect

### UI — `ColorPickerControl.xaml`

- New UserControl `UI/ColorPickerControl.xaml` + `.cs`.
- Replace placeholder in `SettingsPane.xaml` Colors tab with `<local:ColorPickerControl />`.
- Inject dependencies: `IComHostAdapter` (via factory/Globals), `WindowsUserSettingsStore`, status callback (MessageBox or status TextBlock in pane).

### Shared palette helper (avoid duplication with `ExecuteFormatColor`)

New `Host/FormatColorPaletteProvider.cs`:

```csharp
public static IReadOnlyList<string> GetActivePalette(IComHostAdapter host, WindowsUserSettingsStore store)
{
    var theme = host.ReadPresentationThemeColors();
    var recent = store.GetRecentColors();
    return ColorPaletteBuilder.Build(theme, recent, DefaultColorPalette.FallbackTheme);
}
```

Refactor `ExecuteFormatColor` to use provider (minimal diff).

### ComHostAdapter — `ReadColorFromSelection`

```csharp
string ReadColorFromSelection(ColorPickSource source); // fill | line | text
```

COM: first shape in selection; fill `Fill.ForeColor.RGB`, line `Line.ForeColor.RGB`, text `Font.Color.RGB`;
convert via `ColorRgbHelper.OleRgbToHex`; validate with `ThemeColor.IsValidHex`.

### ITaskPaneService rename

| Old | New |
|-----|-----|
| `ShowColorsPlaceholder()` | `ShowColorPicker()` |

Update `CommandRouter.ExecuteSettings` OpenColorScheme branch + interface + tests.

Message: `Color scheme opened.` or keep `Settings pane opened (colors).`

### Refresh policy

- Reload theme + recent when Colors tab selected (or picker loaded).
- After Apply / Pick from shape → refresh recent grid.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `UI/ColorPickerControl.xaml` + `.cs` | Full picker UI |
| `UI/SettingsPane.xaml` | Replace Colors placeholder |
| `UI/TaskPaneService.cs` | `ShowColorPicker`, wire control |
| `Settings/ITaskPaneService.cs` | Rename method |
| `Host/ComHostAdapter.cs` + `IComHostAdapter.cs` | `ReadColorFromSelection` |
| `Host/FormatColorPaletteProvider.cs` | Shared palette build |
| `Host/CommandRouter.cs` | Use provider in `ExecuteFormatColor`; OpenColorScheme → ShowColorPicker |
| `PptPowerKeys.Windows.csproj` | New files |
| `PptPowerKeys.Windows/README.md` | Color picker QA matrix |
| `PptPowerKeys.Tests/FormatColorPaletteProviderTests.cs` (optional) | Palette helper unit test |

## Анти-scope

- **Screen eyedropper** (Web Browser EyeDropper — нет на WPF без native API stretch)
- Global hotkeys / ShortcutManager hook (**S11**)
- New CommandIds
- Api / AddIn changes
- VstoLegacy*
- Separate consulting color preset grids
- Sprint retrospective (architect post-merge)

## Критерии приёмки (builder PR)

- [ ] Colors tab shows working picker (no S10-005 placeholder text)
- [ ] `OpenColorScheme` opens Colors tab with picker (`ShowColorPicker`)
- [ ] Theme swatches from Slide Master; fallback warning + `DefaultColorPalette`
- [ ] Recent swatches from `UserSettings.recentColors` (max 5)
- [ ] Apply Fill/Line/Text with Web-matching success messages
- [ ] Pick from shape (fill/line/text) + `RecordRecentColor`
- [ ] Custom HEX input validates `#RRGGBB`
- [ ] Applied colors persist in `%AppData%/PptPowerKeys/UserSettings.json`
- [ ] `FormatColorPaletteProvider` shared with ribbon cycle commands
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA in PR (theme deck, apply fill, pick from shape, restart → recent)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S10-004 Done (PR #100) — Settings pane + `ITaskPaneService`
- S09-005 — theme read + recent colors infra
- Web Sprint 04–06 — `ColorPickerPanel.tsx`, HEX/pick-from-shape (S06-005)

## Reference

- `src/PptPowerKeys.AddIn/src/taskpane/ColorPickerPanel.tsx`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `readColorFromSelection`, apply*Color
- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts` — `DEFAULT_PALETTE`
- `src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs`
- `src/PptPowerKeys.Core/Colors/DefaultColorPalette.cs`
- `src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs` (Alt+L → OpenColorScheme)
- `src/PptPowerKeys.Windows/UI/SettingsPane.xaml` (Colors placeholder)
- `src/PptPowerKeys.Windows/Host/CommandRouter.cs` (`ExecuteFormatColor`)

## Трассировка

Issue `#N` → `cursor/S10-005-color-picker-*` → PR `Closes #N`

---

## Architect post-merge (Sprint 10 close — **не builder**)

После merge PR S10-005 architect выполняет:

| # | Действие |
|---|----------|
| 1 | Backlog S10-005 → **Done**, Issue закрыть |
| 2 | `sprints/sprint-10-ltsc-slides-settings/retrospective.md` |
| 3 | `goals.md` DoD checkboxes `[x]` (79 commands, Settings JSON v1, color picker COM) |
| 4 | `docs/PRODUCT_CONTEXT.md` — journal S10-005 + **Sprint 10 complete** + **M3 Feature beta** |
| 5 | `sprints/README.md` — Sprint 10 **Done**, Sprint 11 **Next** |
| 6 | `sprints/epic-ltsc-windows-native/ROADMAP.md` — kickoff S11 |

### Retrospective outline

- **M3 Feature beta achieved:** 79/79 commands, 9/9 None unlocks, Settings + Color picker
- **S10-001…005** PR table
- **Metrics:** dotnet test count, Windows command breakdown
- **Next:** S11 — global hotkeys, MSI, QA ship

---

## Copy-paste промпт (новая сессия `/architect` — полный цикл)

```
/architect

Sprint 10 — S10-005 Color picker COM panel (последняя задача спринта, закрытие Sprint 10).
S10-001…004 Done (#93, #95, #97, #100). 79/79 commands routed. Colors tab = placeholder.

Прочитай:
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-005-color-picker-profiles.md
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/backlog.md
- sprints/sprint-10-ltsc-slides-settings/goals.md
- .github/review/CHECKLIST.md
- src/PptPowerKeys.AddIn/src/taskpane/ColorPickerPanel.tsx
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (readColorFromSelection, apply*Color)
- src/PptPowerKeys.Windows/UI/SettingsPane.xaml (Colors placeholder)
- src/PptPowerKeys.Windows/UI/TaskPaneService.cs
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs (ReadPresentationThemeColors, Apply*Color)
- src/PptPowerKeys.Windows/Host/CommandRouter.cs (ExecuteFormatColor, OpenColorScheme)
- src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs

Шаг 1 — постановка builder:
Issue S10-005 → backlog In Progress → /builder выполни S10-005

Шаг 2 — приёмка PR builder:
- ColorPickerControl replaces placeholder; OpenColorScheme → ShowColorPicker
- Theme + recent swatches; Apply Fill/Line/Text; pick from shape; HEX input
- RecordRecentColor; FormatColorPaletteProvider shared with cycle commands
- dotnet test green; CHECKLIST.md; manual QA note

Шаг 3 — после merge (architect):
- retrospective.md, goals DoD, PRODUCT_CONTEXT (M3 + Sprint 10 close)
- sprints/README + epic ROADMAP → Sprint 11 Next

Screen eyedropper — anti-scope. Global hotkeys — S11.
```

# S09-005 — Format colors + palette (4 COM HostScript commands)

> Передача builder'у: `/builder выполни S09-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-005` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (`ColorPaletteBuilder`) |
| **Статус** | Done |
| **Issue** | [#86](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/86) |
| **PR** | [#87](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/87) |

## Цель

Реализовать **4 Format HostScript** на Windows line (parity с Web Add-in `formatColorState.ts`,
`powerpoint.ts`, `runCommand.ts`):

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| FillColor | cycle palette + `applyFillColor` | `Shape.Fill.Solid()` + `ForeColor.RGB` |
| LineColor | cycle palette + `applyLineColor` | `Shape.Line.ForeColor.RGB` |
| TextColor | cycle palette + `applyTextColor` | `TextFrame.TextRange.Font.Color.RGB` |
| ToggleFillBlackWhite | `toggleFillBlackWhite` | fill near-black → white, else → black |

## Контекст (после S09-004)

| Компонент | Состояние |
|-----------|-----------|
| `ColorPaletteBuilder` | Core merge theme≤10 + recent≤5 (S04-001) |
| AddIn | theme read + localStorage recent + cycle fingerprint (S04-003) |
| `CommandRouter` | Objects wave Done; Format → `NotSupportedException` |
| `UserSettings` | `SnapToGrid` persisted; **no** `RecentColors` yet |

## Алгоритм (зафиксировано — match Web)

### FillColor / LineColor / TextColor

```
1. shapes = ReadSelectedShapeBounds()
2. if empty → error "Select one or more shapes first."
3. theme = ReadPresentationThemeColors()  // Slide Master accent1–6 + dark1/2 + light1/2
4. recent = settingsStore.GetRecentColors()
5. palette = ColorPaletteBuilder.Build(theme, recent, DefaultColorPalette.FallbackTheme)
6. color = FormatColorCycleStore.NextPaletteColor(command, palette, shapeIds)
7. count = ApplyFillColor|LineColor|TextColor(color)
8. settingsStore.RecordRecentColor(color)
9. return "Fill|Line|Text color {hex} applied to {count} shape(s)."
```

**Cycle:** fingerprint = shape ids joined `\u001f`; reset index on new selection; advance on repeat.

### ToggleFillBlackWhite

```
1. range = selected shapes; if empty → error
2. for each shape: read Fill.ForeColor.RGB; if near-black (r,g,b < 48) → white else black
3. return "Toggled fill on {count} shape(s)."
```

### Recent colors persist

`UserSettings.RecentColors` (max 5, FIFO, dedupe) → `%AppData%/PptPowerKeys/UserSettings.json` (camelCase).
Parity с Web `localStorage` key `ppt-powerkeys-recent-colors`.

## Решения architect

### CommandRouter API

```csharp
if (FormatColorCommands.IsFormatColorCommand(command)) → ExecuteFormatColor(command)
```

### ComHostAdapter extensions

| Method | Returns | Behavior |
|--------|---------|----------|
| `ReadPresentationThemeColors()` | hex list | Slide Master `ThemeColorScheme` (10 slots) |
| `ApplyFillColor(hex)` | count | Solid fill on selection |
| `ApplyLineColor(hex)` | count | Line visible + color |
| `ApplyTextColor(hex)` | count | Font color; 0 text → throw |
| `ToggleFillBlackWhite()` | count | Black/white toggle |

### Ribbon

- Новая группа **Format** (`grpFormat`) — 3 кнопки (VSTO parity): Fill, Line, Text Color.
- `ToggleFillBlackWhite` — CommandRouter only (shortcut S11); **no** ribbon button (VSTO has no toggle btn).
- `FormatPainter` / `OpenColorScheme` — anti-scope (S10).

### Tests (Linux CI)

- `FormatColorCommandsTests`, `FormatColorCycleStoreTests`, `ColorRgbHelperTests`
- `HostScriptCommandMapTests` — 3 new `btn*` cases
- Link new `.cs` files in `PptPowerKeys.Tests.csproj`
- Optional: `UserSettings` round-trip for `RecentColors`

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Core/Colors/DefaultColorPalette.cs` | Shared fallback theme (10 hex) |
| `Core/Settings/UserSettings.cs` | `RecentColors` property |
| `Host/FormatColorCommands.cs` | Command set helper |
| `Host/FormatColorCycleStore.cs` | Palette cycle state |
| `Host/ColorRgbHelper.cs` | Hex ↔ OLE RGB, near-black |
| `Host/IComHostAdapter.cs` | Color methods |
| `Host/ComHostAdapter.cs` | COM implementation |
| `Host/CommandRouter.cs` | Route 4 commands |
| `Settings/WindowsUserSettingsStore.cs` | Recent colors persist |
| `UI/HostScriptCommandMap.cs` | Parse 3 btn ids |
| `UI/RibbonTab.xml` | grpFormat (3 buttons) |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Format colors |

## Анти-scope

- **FormatPainter**, **OpenColorScheme** (S10)
- Core `ColorPaletteBuilder` merge logic changes
- Api / AddIn changes (Web spec already Done)
- VstoLegacy edits
- Color Picker UI / eyedropper (S04/S06 Done on Web only)

## Критерии приёмки

- [ ] 4 commands routed via `CommandRouter.Execute`
- [ ] Palette cycle + `ColorPaletteBuilder` merge (theme + recent + fallback)
- [ ] Recent colors persisted in `UserSettings.json`
- [ ] Theme from Slide Master (10 slots)
- [ ] Messages match Web `runCommand.ts`
- [ ] Ribbon grpFormat (3 buttons) wired via `OnHostScriptCommand`
- [ ] Unit tests for pure helpers + map tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR: `cursor/s09-005-format-colors-palette-b84a`, Task ID, `Closes #86`

## Зависимости

- S04-001 (`ColorPaletteBuilder`)
- S09-004 Done (#82 / #83)

## Reference files (Web spec)

- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts`
- `src/PptPowerKeys.AddIn/src/office/themeColors.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (grpFormat)

## Трассировка

Issue #86 → `cursor/s09-005-format-colors-palette-b84a` → PR `Closes #86`

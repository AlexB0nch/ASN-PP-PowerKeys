# S09-005 — Format colors + palette (5 HostScript commands)

> Передача builder'у: `/builder выполни S09-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-005` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Зависимости** | S09-004 Done |

## Цель

| CommandId | Behavior |
|-----------|----------|
| FillColor | Theme-aware picker → COM `Fill.ForeColor` |
| LineColor | Line color + visible |
| TextColor | TextFrame font color (skip non-text shapes) |
| ToggleFillBlackWhite | Toggle fill black ↔ white (match Web) |
| (palette infra) | Core `ColorPaletteBuilder` + recent colors store |

**FormatPainter — anti-scope** (S10 unlock).

## Алгоритм (match Web)

- Read theme/master colors via COM slide → feed `ColorPaletteBuilder`.
- Recent colors: `%AppData%/PptPowerKeys/UserSettings.json` extension or separate `RecentColors.json`
  (Windows equivalent of Web `localStorage` recent palette).
- Picker UI: minimal native (ColorDialog) or reuse WPF stub from S10 prep — architect: **ColorDialog MVP**
  for S09 unless Settings pane already exists.

## Решения architect

- `FormatCommands.cs`, `RecentColorsStore.cs` (persist, max N entries).
- `ComHostAdapter`: `ApplyFillColor`, `ApplyLineColor`, `ApplyTextColor`, `ToggleFillBlackWhite`.
- Ribbon color buttons (legacy reference in VstoLegacy).
- Core tests for palette building unchanged; add store tests if new JSON.

## Критерии приёмки

- [ ] 4 format commands apply to selection
- [ ] Recent colors persist across sessions (AppData)
- [ ] Theme palette available to picker flow
- [ ] `dotnet test PptPowerKeys.sln` green

## Reference

- `src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — applyFillColor, applyLineColor, applyTextColor
- Web Sprint 04 smart color picker docs

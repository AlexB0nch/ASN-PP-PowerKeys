# S10-005 — Color picker panel COM + consulting profiles

> Передача builder'у: `/builder выполни S10-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-005` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Issue** | TBD |
| **PR** | — |

## Цель

Заменить placeholder на вкладке **Colors** в WPF task pane полноценным **Color Picker** —
parity Web `ColorPickerPanel.tsx`, с COM theme colors и consulting presets.

| Возможность | Web reference | Windows |
|-------------|---------------|---------|
| Theme swatches | `readPresentationThemeColors` | `ComHostAdapter.ReadPresentationThemeColors` |
| Recent colors | localStorage / UserSettings | `WindowsUserSettingsStore.GetRecentColors` |
| Apply Fill/Line/Text | `powerpoint.ts` HostScript | `ComHostAdapter.ApplyFillColor/LineColor/TextColor` |
| Consulting presets | theme palette merge | `ColorPaletteBuilder` + `DefaultColorPalette` |

## Контекст (после S10-004)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | **79/79** commands routed |
| Settings pane | WPF `SettingsPane.xaml` — **Colors** tab = placeholder «S10-005» |
| `OpenColorScheme` | `TaskPaneService.ShowColorsPlaceholder()` |
| Format color cycle | `ExecuteFormatColor` already uses theme + recent via Core |

## Решения architect

### UI — заменить Colors placeholder

- `ColorPickerPane.xaml` (или секция в `SettingsPane`) — swatch grid (theme + recent),
  Apply Fill / Line / Text buttons, optional HEX input
- `OpenColorScheme` → show Colors tab + focus picker (не placeholder)
- После apply — `WindowsUserSettingsStore.RecordRecentColor`

### COM / Core reuse

- Theme: existing `ReadPresentationThemeColors` + `ColorPaletteBuilder.Build`
- Apply: reuse `IComHostAdapter` fill/line/text methods (как `ExecuteFormatColor`)
- **Не** дублировать palette math — только UI + wiring

### Consulting profiles

- Preset swatch layouts из `DefaultColorPalette.FallbackTheme` / consulting constants в Core
- Parity с Web consulting palette sections (если есть в `ColorPickerPanel.tsx`)

### Eyedropper

**Anti-scope** (как Web S06-005 stretch) — опционально позже; HEX input — nice-to-have если просто.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `UI/ColorPickerPane.xaml` + `.cs` (new) | Swatches + apply buttons |
| `UI/SettingsPane.xaml` | Replace Colors placeholder with `ColorPickerPane` |
| `UI/TaskPaneService.cs` | `ShowColorsPlaceholder` → show real picker |
| `README.md` | Color picker QA steps |

## Анти-scope

- Global hotkeys ShortcutManager (**S11**)
- Api / AddIn changes
- VstoLegacy*
- Eyedropper API (unless trivial on WPF)

## Критерии приёмки

- [ ] `OpenColorScheme` opens working color picker (not placeholder)
- [ ] Swatches reflect presentation theme + recent colors from UserSettings
- [ ] Apply Fill/Line/Text changes selection via COM
- [ ] Applied colors recorded in `recentColors` (max 5, dedupe)
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR with Task ID S10-005

## Зависимости

- S10-004 Done (PR #100)

## Reference files

- `src/PptPowerKeys.AddIn/src/taskpane/ColorPickerPanel.tsx`
- `src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs`
- `src/PptPowerKeys.Windows/Host/ComHostAdapter.cs` (`ReadPresentationThemeColors`)
- `src/PptPowerKeys.Windows/Host/CommandRouter.cs` (`ExecuteFormatColor`)

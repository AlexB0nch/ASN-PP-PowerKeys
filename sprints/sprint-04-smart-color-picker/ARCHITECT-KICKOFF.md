# Кикофф для architect — Sprint 04 (Smart Color Picker)

> Новая сессия `/architect`. Sprint 03 Done — `sprint-03-settings/retrospective.md`.

## 1. Где мы сейчас
- **76 команд** wired; Settings + Shortcut Manager работают (S03-001…003, PR #23–#27).
- **Format colors (S02-002):** `FillColor`, `LineColor`, `TextColor` — cycle через `formatColorState.ts`:
  - `DEFAULT_PALETTE` — 10 фиксированных hex (theme-like)
  - `recentColors` — in-memory, max 5, теряются при reload task pane
- **`OpenColorScheme`:** stub в `App.tsx` — «Smart Color Picker — planned (Sprint 04)»
- **Core:** `ThemeColor.cs` (normalize hex, luminance) — без palette engine
- **Legacy (read-only):** `VstoLegacy/Core/ColorSchemeReader.cs` — stub; `ColorPickerForm.cs`

## 2. Продуктовая цель (README / VSTO)
- `Alt+G` / `Alt+L`: палитра Slide Master (10 theme) + 5 recent; picker UI для `OpenColorScheme`
- Повторное нажатие Fill/Line — cycle без диалога (уже есть в S02-002)

## 3. Рекомендуемая декомпозиция (architect уточняет)
| ID | Содержание |
|----|------------|
| **S04-001** | Office.js: read theme colors from presentation + fallback palette |
| **S04-002** | ColorPicker UI + wire `OpenColorScheme` |
| **S04-003** | Integrate theme+recent with Fill/Line/Text + persist recent |

Можно объединить в 1–2 задачи если scope мал.

## 4. Ключевые файлы
- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `applyFillColor`, etc.
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`, `runCommand.ts`
- `src/PptPowerKeys.Core/Colors/ThemeColor.cs`
- `README.md` — Smart Color Picker spec

## 5. Office.js research (builder must verify)
- `presentation.slideMasters`, theme colors API — requirement sets, Web availability
- Если read недоступен на Web — UI показывает fallback + label «theme colors unavailable»

## 6. Инварианты
- HostScript colors — Office.js setters; palette logic testable in Core где возможно
- `VstoLegacy*` не трогать

## 7. Процесс
Task → backlog → `/builder` → приёмка → merge → `docs/PRODUCT_CONTEXT.md`

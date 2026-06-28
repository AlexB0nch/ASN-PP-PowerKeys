# S02-002 — Format: fill/line/text color, toggle black-white

> Передача builder'у: `/builder выполни S02-002`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-002` |
| **Спринт** | `sprint-02-functionality` |
| **Компонент** | AddIn |
| **Статус** | In Progress |

## Цель
Реализовать исполнение команд категории **Format**, которые сейчас падают в «not wired up yet».
Пользователь PowerPoint Online должен иметь рабочие заливка/обводка/цвет текста и toggle чёрный-белый;
для `FormatPainter` — явная деградация (support=`None`).

## Контекст
- VSTO «ShortCut Tools» для `FillColor`/`LineColor` использует палитру Slide Master + recent colors
  с циклическим переключением (`README.md`). На Web чтение темы ограничено (`OfficeJsSupport.Partial`).
- **S02-002 — pragmatic Web parity:** фиксированная палитра + in-memory «recent» + цикл по повторному нажатию
  с тем же выделением; без Slide Master picker (вынести в будущую задачу Smart Color Picker).
- Format-команды — **HostScript** (`CommandCatalog`), математика layout не нужна.

## Scope
### HostScript — реализовать
| Command ID | Подход |
|------------|--------|
| `ToggleFillBlackWhite` | Для выделенных фигур: если заливка близка к чёрному → белый, иначе → чёрный (`shape.fill.setSolidColor`) |
| `TextColor` | Применить цвет к `textFrame.textRange.font.color` выделенных фигур с текстом; цикл по палитре |
| `FillColor` | `shape.fill.setSolidColor(color)`; цикл по палитре при повторном нажатии с тем же selection fingerprint |
| `LineColor` | `shape.lineFormat.color` (или эквивалент Office.js); цикл по палитре |
| `FormatPainter` | Явное сообщение: «Format painter is not supported on PowerPoint Web.» |

### Палитра и состояние (AddIn)
- Модуль состояния, напр. `formatColorState.ts`:
  - дефолтная палитра (~10 цветов, разумные theme-like hex);
  - recent colors (до 5, FIFO);
  - индекс цикла + fingerprint последнего выделения (ids в порядке) для Fill/Line/Text color.
- При новом выделении — сброс индекса цикла, применить первый цвет палитры.
- При повторном нажатии с тем же выделением — следующий цвет по кругу.
- После применения — добавить цвет в recent (если ещё нет).

### Office.js helpers (`powerpoint.ts`)
- `toggleFillBlackWhite()`
- `applyFillColor(hex)`, `applyLineColor(hex)`, `applyTextColor(hex)` — или единый helper
- Чтение текущего fill для toggle (load `fill.foregroundColor` / type если доступно)
- Понятные ошибки: нет выделения, фигура без текста (для TextColor)

## Анти-scope
- Smart Color Picker / чтение палитры Slide Master (отдельная будущая задача).
- Fluent UI color picker dialog (не требуется в S02-002).
- FormatPainter эмуляция (копирование набора свойств).
- Text / Slides / Alignment / Objects команды.
- Core / Api изменения (Format — чистый HostScript).
- `VstoLegacy*`.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts` (новый)

## Office.js feasibility
- `shape.fill.setSolidColor("#RRGGBB")` — Partial (нет theme palette read).
- `shape.lineFormat.color` — Partial.
- `textFrame.textRange.font.color` — Full.
- `FormatPainter` — None.

## Критерии приёмки (Definition of Done)
1. [ ] `ToggleFillBlackWhite`, `FillColor`, `LineColor`, `TextColor` не попадают в default «not wired up yet».
2. [ ] `FormatPainter` возвращает явное сообщение о неподдержке на Web.
3. [ ] Fill/Line/Text color циклируют палитру при повторном нажатии с тем же выделением; recent colors обновляются.
4. [ ] Toggle black/white работает на выделенных фигурах с заливкой.
5. [ ] Понятные ошибки при пустом выделении / TextColor без текста.
6. [ ] `dotnet test PptPowerKeys.sln` — зелёный.
7. [ ] `npm run typecheck`, `npm run validate:prod` — зелёные.
8. [ ] PR: ветка `cursor/S02-002-format-colors-a065`, Task ID `S02-002`.

## Зависимости
- Нет блокеров от S02-001 (Format независим).

## Примечание для builder
- Цвета Office.js — строки `"#RRGGBB"` (проверь типы в `@types/office-js`).
- Не ломай уже работающие HostScript-команды из main (Insert, ZOrder, Addup).
- Если S02-001 ещё не в main — не смешивай в один PR; ветка от актуального `main`.

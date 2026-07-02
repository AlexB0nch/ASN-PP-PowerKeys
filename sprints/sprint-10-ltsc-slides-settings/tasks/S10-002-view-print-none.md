# S10-002 — None unlock: view/print (6 COM commands)

> Передача builder'у: `/builder выполни S10-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-002` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | In Review |
| **Issue** | — |
| **PR** | #95 |

## Цель

Реализовать **6 OfficeJs None unlocks** на Windows line — команды, которые на Web деградируют
(`unsupportedWebCommands.ts`), но доступны через COM на Desktop:

| CommandId | VSTO ribbon | COM target |
|-----------|-------------|------------|
| ToggleZoom | `btnToggleZoom` | View zoom / zoom dialog |
| ToggleSlideSorter | `btnSlideSorter` | Slide sorter view |
| StartSlideShow | — | Start slideshow from current slide |
| ToggleGrid | — | `View.GridLines` toggle |
| ToggleGuides | — | `View.Guides` toggle |
| PrintSlide | `btnPrintSlide` | Print current slide / print dialog |

## Контекст (после S10-001)

| Комponent | Состояние |
|-----------|-----------|
| `CommandRouter` | 67 commands routed |
| Web | None → explicit degradation messages |
| Windows | Slides grpSlides has CopySlide only; extend for view/print buttons |

## Решения architect

### CommandRouter

- Новый `ViewPrintCommands.IsViewPrintCommand` → `ExecuteViewPrint(command)`
- Каждая команда — прямой COM host script (без Core layout)

### Ribbon

- Расширить **Slides** (`grpSlides`): `btnToggleZoom`, `btnSlideSorter`, `btnPrintSlide` (VSTO parity)
- Grid/Guides/SlideShow — CommandRouter + shortcuts (no ribbon btn unless VSTO had them)

### Tests

- `ViewPrintCommandsTests` + `HostScriptCommandMapTests` для ribbon ids

## Анти-scope

- FormatPainter, PasteFormatted, Regroup (S10-003)
- Settings UI (S10-004)
- Api / AddIn changes

## Критерии приёмки

- [x] 6 commands routed via `CommandRouter.Execute`
- [x] COM behavior matches VSTO / PowerPoint Desktop expectations
- [x] Ribbon buttons for Zoom, Sorter, Print in `grpSlides`
- [x] Unit tests + `dotnet test PptPowerKeys.sln` green (289 passed)
- [ ] PR with Task ID S10-002

## Reference files

- `src/PptPowerKeys.AddIn/src/taskpane/unsupportedWebCommands.ts`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (grpSlides)
- `sprints/epic-ltsc-windows-native/FEATURE_PARITY.md`

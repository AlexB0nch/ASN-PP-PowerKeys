# S10-001 — Slide HostScript commands (2 COM commands)

> Передача builder'у: `/builder выполни S10-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-001` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Done |
| **Issue** | — |
| **PR** | #93 (merged) |

## Цель

Реализовать **2 Slide HostScript** на Windows line (parity с Web Add-in `powerpoint.ts`, `runCommand.ts`):

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| CopySlide | duplicate selected slide after source | `Slide.Duplicate()` or export/insert |
| MoveSlidesToBackup | move selected slides to deck end | `Slide.MoveTo` / COM reorder |

## Контекст (после S09-006)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 65 commands; Slides → `NotSupportedException` |
| Web spec | `duplicateSelectedSlide`, `moveSelectedSlidesToBackup` Done |
| VSTO ribbon | `grpSlides` reference (Copy Slide btn) |

## Алгоритм (match Web)

### CopySlide

```
1. slides = selected slide range; empty → "Select a slide first."
2. duplicate first selected slide immediately after source
3. return "Slide duplicated."
```

### MoveSlidesToBackup

```
1. slides = selected; empty → "Select one or more slides first."
2. move each selected slide to end of deck (preserve relative order or match Web: high index first)
3. return "Moved {count} slide(s) to backup (end of deck)."
```

## Решения architect

### CommandRouter

```csharp
if (SlideCommands.IsSlideCommand(command)) → ExecuteSlide(command)
```

### ComHostAdapter

| Method | Behavior |
|--------|----------|
| `DuplicateSelectedSlide()` | COM duplicate active/first selected slide |
| `MoveSelectedSlidesToBackup()` | returns count moved |

### Ribbon

- Новая группа **Slides** (`grpSlides`) — минимум `btnCopySlide` (VSTO parity).
- `MoveSlidesToBackup` — CommandRouter + shortcuts (no VSTO ribbon btn).

### Tests

- `SlideCommandsTests` + `HostScriptCommandMapTests` for `btnCopySlide`

## Анти-scope

- View/print None unlocks (S10-002)
- FormatPainter, PasteFormatted, Regroup (S10-003)
- Settings UI (S10-004)
- Api / AddIn changes

## Критерии приёмки

- [x] 2 commands routed via `CommandRouter.Execute`
- [x] Messages match Web `runCommand.ts`
- [x] Ribbon `grpSlides` with `btnCopySlide`
- [x] Unit tests + `dotnet test PptPowerKeys.sln` green
- [x] PR #93 merged, Task ID S10-001

## Reference files

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (grpSlides)

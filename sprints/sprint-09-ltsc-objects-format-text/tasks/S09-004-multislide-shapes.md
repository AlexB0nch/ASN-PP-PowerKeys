# S09-004 — Multi-slide paste/remove shapes (2 HostScript commands)

> Передача builder'у: `/builder выполни S09-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-004` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S09-001…003 Done |

## Цель

Реализовать **multi-slide shape operations** на Windows line — 2 HostScript команды через COM.

| CommandId | Поведение | Success message (match Web) |
|-----------|-----------|----------------------------|
| PasteShapeToSelectedSlides | Клонировать **одну** фигуру с активного слайда на каждый другой выделенный слайд | `Pasted shape to N slide(s).` |
| RemoveShapeFromSelectedSlides | Удалить на выделенных слайдах все фигуры с тем же `Name`, что у эталона | `Removed N shape(s) from M slide(s).` |

Parity с Web Add-in (`S05-004`): `powerpoint.ts` + `runCommand.ts`.  
**Нет** parity в VSTO legacy — команды добавлены в Web line как инновация.

## Контекст (после S09-001…003)

| Компонент | Ожидаемое состояние |
|-----------|---------------------|
| `CommandRouter` | Insert* (6) + Duplicate* (4) + Group/Z-order (6) + layout (38) |
| `HostScriptCommandMap` | Расширен S09-001…003 |
| `ComHostAdapter` | Insert, clone on **active slide**, group/z-order — **нет** multi-slide |
| Web UX spec | [`sprints/sprint-05-advanced-features/tasks/S05-004-multi-slide-paste-remove.md`](../../sprint-05-advanced-features/tasks/S05-004-multi-slide-paste-remove.md) |
| Catalog | `OfficeJsSupport.Partial`, HostScript, **Objects**, defaultShortcut null |

## Алгоритм (match Web / S05-004)

### PasteShapeToSelectedSlides

```
1. slideRange = GetSelectedSlides()  // COM SlideRange
2. if slideRange.Count < 2 → error "Select two or more slides first."
3. sourceShape = GetSingleSelectedShapeOnActiveSlide()
4. if sourceShape == null → error "Select exactly one shape on the active slide first."
5. sourceSlide = ActiveWindow.View.Slide  // slide under edit
6. Load sourceLeft, sourceTop, sourceWidth, sourceHeight from sourceShape
7. pasted = 0
8. for each slide in slideRange:
     if slide.SlideIndex == sourceSlide.SlideIndex → skip
     CopyShapeToSlide(sourceShape, slide, geometry)
     pasted++
9. message: "Pasted shape to {pasted} slide(s)."
```

**UX (зафиксировано S05-004):**

- Пользователь выделяет **≥2 слайда** (Slide Sorter / thumbnails).
- На **активном** слайде выделена **ровно одна** фигура-шаблон.
- Слайд-источник **пропускается**, если он тоже в multi-selection.
- Геометрия клона = source (`Left`, `Top`, `Width`, `Height` в points).

### RemoveShapeFromSelectedSlides

```
1. slideRange = GetSelectedSlides()
2. if slideRange.Count < 1 → error "Select one or more slides first."
3. sourceShape = GetSingleSelectedShapeOnActiveSlide()
4. if sourceShape == null → error "Select exactly one shape on the active slide first."
5. targetName = sourceShape.Name
6. if string.IsNullOrEmpty(targetName) → error "Selected shape has no name. Name the shape first."
7. shapesRemoved = 0; slidesProcessed = slideRange.Count
8. for each slide in slideRange:
     for each shape on slide (reverse order when deleting):
       if shape.Name == targetName (case-sensitive):
         shape.Delete(); shapesRemoved++
9. message: "Removed {shapesRemoved} shape(s) from {slidesProcessed} slide(s)."
```

**UX:** 0 совпадений на слайде — **не ошибка**. Удаление по **exact name**, case-sensitive.

## Решения architect

### MultiSlideShapeCommands helper

New file `Host/MultiSlideShapeCommands.cs`:

```csharp
public static class MultiSlideShapeCommands
{
    public static bool IsMultiSlideShapeCommand(CommandIds command) =>
        command == CommandIds.PasteShapeToSelectedSlides
        || command == CommandIds.RemoveShapeFromSelectedSlides;
}
```

### CommandRouter

Extend `Execute`:

```csharp
if (MultiSlideShapeCommands.IsMultiSlideShapeCommand(command))
    return command switch
    {
        CommandIds.PasteShapeToSelectedSlides => ExecutePasteShapeToSelectedSlides(),
        CommandIds.RemoveShapeFromSelectedSlides => ExecuteRemoveShapeFromSelectedSlides(),
        _ => throw new InvalidOperationException(...),
    };
```

Return `CommandExecutionResult` with Web-matching messages.

### ComHostAdapter extensions

| Method | Behavior |
|--------|----------|
| `SlideRange GetSelectedSlideRange()` | `Selection.Type == ppSelectionSlides` → `Selection.SlideRange`; else throw «Select … slides first.» |
| `Shape GetSingleSelectedShapeOnActiveSlide()` | `ppSelectionShapes`, Count == 1; else null / throw path in caller |
| `Slide GetActiveSlide()` | Reuse existing private helper if present |
| `int PasteShapeToSelectedSlides()` | Algorithm above |
| `(int slidesProcessed, int shapesRemoved) RemoveShapeFromSelectedSlides()` | Algorithm above |

#### COM paste strategy (Windows desktop)

Предпочтительный путь — **native Copy/Paste** (полнее Web recreate):

```
sourceShape.Copy();
ShapeRange pasted = targetSlide.Shapes.Paste();
Shape clone = pasted[1];
clone.Left = sourceLeft; clone.Top = sourceTop;
clone.Width = sourceWidth; clone.Height = sourceHeight;
```

Fallback при ошибке Paste для экзотических типов — явное сообщение (можно адаптировать Web:
«Shape paste is not supported for this shape type…»). На desktop большинство типов поддерживается через Copy/Paste.

#### COM slide selection

- Multi-slide: пользователь в **Slide Sorter** → `Selection.SlideRange`.
- Если selection не slides (например только shapes) — paste/remove возвращают ошибку про slides.
- **Не** смешивать с CopySlide / MoveSlidesToBackup (Slides category, S10).

### HostScriptCommandMap

Extend `TryParse`:

```csharp
|| MultiSlideShapeCommands.IsMultiSlideShapeCommand(command)
```

### Ribbon

Новая подгруппа **`grpMultiSlide`** (или 2 кнопки в `grpObjects` после S09-001):

| Control id | Label | imageMso | onAction |
|------------|-------|----------|----------|
| `btnPasteShapeToSelectedSlides` | Paste to Slides | SlideDuplicate | OnHostScriptCommand |
| `btnRemoveShapeFromSelectedSlides` | Remove by Name | ShapeDelete | OnHostScriptCommand |

`btn{CommandIds}` — парсится существующим `HostScriptCommandMap`.

Update `PptPowerKeys.Windows/README.md` — multi-slide section + manual QA matrix.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/MultiSlideShapeCommands.cs` | Command id guard |
| `Host/CommandRouter.cs` | Paste/remove handlers |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | Slide range, paste, remove-by-name |
| `UI/HostScriptCommandMap.cs` | Include 2 commands |
| `UI/RibbonTab.xml` | 2 buttons (grpMultiSlide or grpObjects) |
| `PptPowerKeys.Windows/README.md` | Docs + manual QA |
| `PptPowerKeys.Tests/MultiSlideShapeCommandsTests.cs` (optional) | `IsMultiSlideShapeCommand` |

## Анти-scope

- **CopySlide** / **MoveSlidesToBackup** (Slides, S10)
- Remove by type / partial name / regex
- Paste на слайд-источник (explicit skip)
- Core / Api / AddIn changes (команды уже в каталоге с S05-004)
- Persist template shape / slide registry
- Group/Z-order (S09-003), Format/Text (S09-005/006)

## Критерии приёмки

- [ ] Paste: ≥2 slides, 1 source shape, skip source slide, same geometry
- [ ] Paste errors match Web strings (`Select two or more slides first.`, `Select exactly one shape…`)
- [ ] Remove: ≥1 slide, exact name match case-sensitive, empty name error
- [ ] Remove: 0 matches on a slide is OK; aggregate counts in message
- [ ] Ribbon 2 buttons → `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` parses both ids
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA in PR: sorter 3 slides, paste logo; remove by shared name
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S09-001…003 Done (HostScript pipeline + ComHostAdapter patterns)
- Web spec S05-004 Done (behavior reference)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `pasteShapeToSelectedSlides`, `removeShapeFromSelectedSlides`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — cases + success messages
- `sprints/sprint-05-advanced-features/tasks/S05-004-multi-slide-paste-remove.md` — UX decisions

## Трассировка

Issue `#N` → `cursor/S09-004-multislide-shapes-*` → PR `Closes #N`

## Copy-paste промпт (новая сессия `/architect`)

```
/architect

Sprint 09 — S09-004 Multi-slide paste/remove shapes (2 HostScript commands).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-004-multislide-shapes.md
- sprints/sprint-05-advanced-features/tasks/S05-004-multi-slide-paste-remove.md (Web UX spec)
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- sprints/sprint-09-ltsc-objects-format-text/backlog.md
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs
- src/PptPowerKeys.Windows/UI/HostScriptCommandMap.cs
- src/PptPowerKeys.Windows/Host/GroupAndZOrderCommands.cs (pattern)
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (pasteShapeToSelectedSlides, removeShapeFromSelectedSlides)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (PasteShapeToSelectedSlides, RemoveShapeFromSelectedSlides)

S09-001…003 Done. Issue S09-004 → backlog In Progress → /builder выполни S09-004 → приёмка → merge.
CopySlide/MoveSlidesToBackup — anti-scope (S10). После merge: backlog Done, PRODUCT_CONTEXT journal (S09-004).
```

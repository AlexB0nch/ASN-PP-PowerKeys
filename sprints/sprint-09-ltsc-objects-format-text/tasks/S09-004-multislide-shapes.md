# S09-004 — Multi-slide paste / remove shapes (2 COM HostScript commands)

> Передача builder'у: `/builder выполни S09-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-004` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | In Progress |
| **Issue** | [#82](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/82) |
| **PR** | TBD |

## Цель

Реализовать **2 HostScript команды** multi-slide paste/remove на Windows line (parity с Web Add-in
`pasteShapeToSelectedSlides`, `removeShapeFromSelectedSlides`):

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| PasteShapeToSelectedSlides | clone 1 source shape onto every other selected slide; skip source slide; same geometry | `Shape.Copy()` + `Slide.Shapes.Paste()` per target slide |
| RemoveShapeFromSelectedSlides | delete all shapes with same `name` on each selected slide | iterate slides + `Shape.Delete()` by exact name |

Web spec (эталон UX): `S05-004-multi-slide-paste-remove.md`, `powerpoint.ts`, `runCommand.ts`.

## Контекст (после S09-003)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | ServerLayout + CopyAndAlign + Position + Insert* + Duplicate* + Group/Z-order; Paste/Remove multi-slide → `NotSupportedException` |
| `HostScriptCommandMap` | CopyAndAlign + Position + Insert* + Duplicate* + Group/Z-order |
| `CommandIds` / `CommandCatalog` | `PasteShapeToSelectedSlides` / `RemoveShapeFromSelectedSlides` уже в Core (S05-004 Web) |
| Web AddIn | HostScript helpers Done (S05-004) |

## Алгоритм (зафиксировано — match Web)

### PasteShapeToSelectedSlides

```
1. slideRange = Selection.SlideRange  // Type == ppSelectionSlides
2. if slideRange.Count < 2 → error "Select two or more slides first."
3. shapeRange = Selection.ShapeRange OR read single shape on active slide
4. if shapeRange.Count != 1 → error "Select exactly one shape on the active slide first."
5. source = shapeRange[1]; capture Left, Top, Width, Height
6. sourceSlide = slide containing source (ActiveWindow.View.Slide or parent of source)
7. for each slide in slideRange:
     if slide.SlideIndex == sourceSlide.SlideIndex → skip
     else:
       source.Copy()
       pasted = targetSlide.Shapes.Paste()  // first shape in range
       set Left/Top/Width/Height from source
       pastedCount++
8. return "Pasted shape to N slide(s)."
```

**COM note:** когда `Selection.Type == ppSelectionSlides`, source shape читаем с **активного слайда**
(`ActiveWindow.View.Slide`): если текущий `Selection` — слайды, shape selection может быть недоступен
в том же `Selection` — допустимо временно переключить чтение через `View.Slide` + последнее выделение
фигуры на активном слайде (match Web: 1 shape on active slide + ≥2 slides in selection).

### RemoveShapeFromSelectedSlides

```
1. slideRange = Selection.SlideRange  // Count >= 1
2. if slideRange.Count == 0 → error "Select one or more slides first."
3. exactly 1 selected shape on active slide → targetName = shape.Name
4. if targetName empty → error "Selected shape has no name. Name the shape first."
5. shapesRemoved = 0; slidesProcessed = slideRange.Count
6. for each slide in slideRange:
     iterate shapes **backwards** (Count down to 1):
       if shape.Name == targetName → shape.Delete(); shapesRemoved++
7. return "Removed X shape(s) from Y slide(s)."
```

## Решения architect

### CommandRouter API

```csharp
if (MultiSlideShapeCommands.IsMultiSlideShapeCommand(command)) → ExecuteMultiSlideShape(command)
```

`MultiSlideShapeCommands.cs` — static `IsMultiSlideShapeCommand` + 2 ids (по аналогии с `GroupZOrderCommands`).

### ComHostAdapter extensions

| Method | Returns | Behavior |
|--------|---------|----------|
| `int PasteShapeToSelectedSlides()` | count pasted | COM copy/paste per target slide; throws on validation errors |
| `(int SlidesProcessed, int ShapesRemoved) RemoveShapeFromSelectedSlides()` | aggregates | COM delete by name; throws on validation errors |

Добавить в `IComHostAdapter` + `ComHostAdapter.cs`.

### HostScriptCommandMap + Ribbon

- Новая группа **Multi-slide** (`grpMultiSlide`) — 2 кнопки:
  - `btnPasteShapeToSelectedSlides`
  - `btnRemoveShapeFromSelectedSlides`
- `onAction="OnHostScriptCommand"`; расширить `HostScriptCommandMap.TryParse` для 2 ids.
- Labels/imageMso: `Paste` / `ObjectDelete` (или аналог из catalog titles).

### Success messages (match `runCommand.ts`)

| CommandId | Message |
|-----------|---------|
| PasteShapeToSelectedSlides | `Pasted shape to {count} slide(s).` |
| RemoveShapeFromSelectedSlides | `Removed {shapesRemoved} shape(s) from {slidesProcessed} slide(s).` |

### Tests (Linux CI)

- `MultiSlideShapeCommandsTests` — `IsMultiSlideShapeCommand` true/false.
- Расширить `HostScriptCommandMapTests` — 2 новых `btn*` cases.
- Link `MultiSlideShapeCommands.cs` в `PptPowerKeys.Tests.csproj`.
- `dotnet test PptPowerKeys.sln` green.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/MultiSlideShapeCommands.cs` | New — command set helper |
| `Host/IComHostAdapter.cs` | Paste / Remove multi-slide methods |
| `Host/ComHostAdapter.cs` | COM implementation |
| `Host/CommandRouter.cs` | Route 2 commands |
| `UI/HostScriptCommandMap.cs` | Parse 2 btn ids |
| `UI/RibbonTab.xml` | grpMultiSlide (2 buttons) |
| `PptPowerKeys.Windows.csproj` | Compile new file (if needed) |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Multi-slide paste/remove section |

## Анти-scope

- **CopySlide** / **MoveSlidesToBackup** (S10 — Slides None/Partial unlock)
- Core / Api / AddIn changes (Web spec already Done)
- Format / Text (S09-005…006)
- VstoLegacy edits
- Partial name match / delete by type — only exact `shape.Name` (case-sensitive)
- Paste onto source slide when it is in selection (explicit skip)

## Критерии приёмки

- [ ] Both commands routed via `CommandRouter.Execute`
- [ ] Paste: ≥2 slides, 1 source shape, skip source slide, same Left/Top/Width/Height
- [ ] Remove: ≥1 slide, exact name match, aggregates in status message; empty name → error
- [ ] Validation error strings match Web `powerpoint.ts`
- [ ] Ribbon grpMultiSlide (2 buttons) wired via `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` + `MultiSlideShapeCommands` unit tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR: ветка `cursor/S09-004-multislide-shapes-<suffix>`, Task ID, `Closes #<issue>`
- [ ] `.github/review/CHECKLIST.md` — scope OK
- [ ] После merge: backlog S09-004 → **Done**; `PRODUCT_CONTEXT` journal (S09-004 Windows parity)

## Зависимости

- S05-004 Done (Web spec + Core catalog)
- S09-003 Done (#78 / #79)

## Reference files (Web spec)

- `sprints/sprint-05-advanced-features/tasks/S05-004-multi-slide-paste-remove.md`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `pasteShapeToSelectedSlides`, `removeShapeFromSelectedSlides`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — cases + messages
- `src/PptPowerKeys.Windows/Host/GroupZOrderCommands.cs` — pattern for command set helper

## Трассировка

Issue → `cursor/S09-004-multislide-shapes-<suffix>` → PR `Closes #<issue>`

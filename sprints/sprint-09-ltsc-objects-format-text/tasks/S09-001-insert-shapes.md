# S09-001 — Insert shapes (6 COM HostScript commands)

> Передача builder'у: `/builder выполни S09-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-001` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Done |
| **Issue** | [#73](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/73) |
| **PR** | [#74](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/74) |

## Цель

Реализовать **первую волну Objects HostScript** на Windows line: 6 команд вставки фигур через COM
на активный слайд (parity с Web Add-in `insertShape` / `insertTextBox`).

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| InsertRectangle | `addGeometricShape(rectangle)` 100,100 150×100 | `Shapes.AddShape(msoShapeRectangle)` |
| InsertSquare | same as rectangle on Web; catalog: equalize W/H | rectangle with **width = height = 100** |
| InsertEllipse | `addGeometricShape(ellipse)` 100,100 150×100 | `Shapes.AddShape(msoShapeOval)` |
| InsertLine | `addLine()` | `Shapes.AddLine(BeginX, BeginY, EndX, EndY)` |
| InsertTextbox | `addTextBox("")` 100,100 200×80 | `Shapes.AddTextbox(...)` |
| InsertArrow | `addLine()` on Web (partial) | line + **end arrowhead** (COM full parity) |

## Контекст (после S08)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | ServerLayout (32) + CopyAndAlign (4) + Position (2); Insert* → `NotSupportedException` |
| `HostScriptCommandMap` | Только CopyAndAlign + Position |
| `RibbonTab.xml` | Layout + CopyAndAlign + Position; **нет** grpObjects |
| Web spec | `powerpoint.ts` lines ~147–169, ~378–388; `runCommand.ts` Insert* cases |

## Алгоритм (зафиксировано — match Web + catalog)

```
1. slide = ActivePresentation.SlideWindow.View.Slide  // or equivalent active slide
2. shapes = slide.Shapes
3. switch command:
     InsertRectangle → auto = AddShape(msoShapeRectangle); Left=100, Top=100, Width=150, Height=100
     InsertSquare    → same as rectangle; Width=Height=100
     InsertEllipse   → auto = AddShape(msoShapeOval); Left=100, Top=100, Width=150, Height=100
     InsertLine      → AddLine(100, 150, 250, 150)  // horizontal ~150pt near top-left
     InsertTextbox   → AddTextbox(..., Left=100, Top=100, Width=200, Height=80)
     InsertArrow     → line as InsertLine + Line.EndArrowheadStyle = msoArrowheadTriangle
4. return CommandExecutionResult { Changed=true, Message="Rectangle inserted." }  // match runCommand.ts strings
```

**Единицы:** Office.js и COM PowerPoint используют **points** — те же числа, что в Web spec.

**Selection:** вставка не требует выделения; при отсутствии активного слайда — понятная ошибка.

## Решения architect

### CommandRouter API

Расширить `Execute(CommandIds)`:

```csharp
if (InsertShapeCommands.IsInsertShape(command)) → ExecuteInsertShape(command)
```

Ввести `InsertShapeCommands.cs` (static `IsInsertShape` + set of 6 ids) — по аналогии с `CopyAndAlignCommands` / `PositionCommands`.

### ComHostAdapter extensions

| Method | Behavior |
|--------|----------|
| `InsertShape(CommandIds command)` | COM insert on active slide; throws `InvalidOperationException` if no slide |

Добавить в `IComHostAdapter` + реализацию в `ComHostAdapter.cs`.

### HostScriptCommandMap + Ribbon

- Новая группа **Objects** (`grpObjects`) — 6 кнопок, ids `btnInsertRectangle` … `btnInsertArrow` (match `CommandIds` enum names).
- Reference layout: `VstoLegacy/UI/RibbonTab.xml` grpObjects (labels/imageMso).
- `onAction="OnHostScriptCommand"`; расширить `HostScriptCommandMap.TryParse` для 6 Insert ids.

### Tests (Linux CI)

- `InsertShapeCommandsTests` — `IsInsertShape` true/false.
- Расширить `HostScriptCommandMapTests` — 6 новых `btnInsert*` cases.
- Link `InsertShapeCommands.cs` в `PptPowerKeys.Tests.csproj` (как CopyAndAlign).
- `dotnet test PptPowerKeys.sln` green.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/InsertShapeCommands.cs` | New — command set helper |
| `Host/IComHostAdapter.cs` | `InsertShape(CommandIds)` |
| `Host/ComHostAdapter.cs` | COM Shapes.Add* |
| `Host/CommandRouter.cs` | Route Insert* |
| `UI/HostScriptCommandMap.cs` | Parse 6 btn ids |
| `UI/RibbonTab.xml` | grpObjects (6 buttons) |
| `PptPowerKeys.Windows.csproj` | Compile new file |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Insert shapes section |

## Анти-scope

- Duplicate*, Group, Z-order (S09-002…003)
- Format / Text (S09-005…006)
- HTTP Api
- Core changes
- VstoLegacy edits
- Global hotkeys (S11)

## Критерии приёмки

- [ ] All 6 Insert commands routed via `CommandRouter.Execute`
- [ ] Shapes appear on active slide at documented positions/sizes
- [ ] InsertArrow has visible end arrowhead (COM advantage over Web partial)
- [ ] Ribbon grpObjects (6 buttons) wired via `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` + `InsertShapeCommands` unit tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR manual QA note + `.github/review/CHECKLIST.md`

## Зависимости

- Sprint 08 Done (S08-001…005, #63–#71)

## Reference files (Web spec)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `insertShape`, `insertTextBox`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Insert* cases + success messages
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — grpObjects button ids/imageMso
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs` — Insert* metadata

## Трассировка

Issue `#N` → `cursor/S09-001-insert-shapes-364b` → PR `Closes #N`

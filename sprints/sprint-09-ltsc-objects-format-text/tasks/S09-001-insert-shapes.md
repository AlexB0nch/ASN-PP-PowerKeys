# S09-001 — Insert shapes (6 HostScript commands)

> Передача builder'у: `/builder выполни S09-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-001` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |

## Цель

Реализовать **первую HostScript-волну Sprint 09**: 6 команд вставки фигур на активный слайд через COM.

| CommandId | COM / поведение |
|-----------|-----------------|
| InsertRectangle | `Shapes.AddShape(msoShapeRectangle, …)` |
| InsertSquare | Rectangle с **равными** Width и Height |
| InsertEllipse | `Shapes.AddShape(msoShapeOval, …)` |
| InsertLine | `Shapes.AddLine(beginX, beginY, endX, endY)` |
| InsertArrow | `Shapes.AddShape(msoShapeRightArrow, …)` (лучше legacy/Web line-only) |
| InsertTextbox | `Shapes.AddTextbox(msoTextOrientationHorizontal, …)` |

Parity с Web Add-in `runCommand.ts` + `powerpoint.ts` (`insertShape`, `insertTextBox`).

## Контекст (после Sprint 08)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 38 layout-related; Insert* → `NotSupportedException` |
| `HostScriptCommandMap` | CopyAndAlign (4) + Position (2) only |
| `ComHostAdapter` | Read/apply bounds, clone, position — **нет** insert |
| Web defaults | rectangle/ellipse: L=100, T=100, W=150, H=100; textbox: W=200, H=80; line/arrow: `addLine()` |
| Legacy ribbon | `VstoLegacy/UI/RibbonTab.xml` → `grpObjects` (6 кнопок) |

## Алгоритм (match Web)

### InsertRectangle / InsertEllipse

```
1. slide = ActivePresentation.Slides[ActiveWindow.Selection.SlideRange.SlideIndex]
2. shape = slide.Shapes.AddShape(type, left, top, width, height)
3. defaults: left=100, top=100, width=150, height=100  (points)
4. message: "Rectangle inserted." / "Ellipse inserted."
```

### InsertSquare

```
Same as rectangle, but width == height (e.g. 100×100 at 100,100).
message: "Rectangle inserted." on Web for both — Windows may use "Square inserted." (clearer).
```

### InsertLine

```
slide.Shapes.AddLine(50, 50, 200, 50)  // horizontal segment near top-left
message: "Line inserted."
```

Web `InsertArrow` also calls `insertShape("line")` — на Windows **предпочтительно** `msoShapeRightArrow`
с теми же default bounds что rectangle (100,100,150,100) для видимой стрелки.

### InsertTextbox

```
slide.Shapes.AddTextbox(msoTextOrientationHorizontal, 100, 100, 200, 80)
message: "Text box inserted."
```

**Не требует** предварительного selection (как Web).

## Решения architect

### InsertShapeCommands helper

New file `Host/InsertShapeCommands.cs`:

```csharp
public static class InsertShapeCommands
{
    public static bool IsInsertShapeCommand(CommandIds command) => command switch
    {
        CommandIds.InsertRectangle or CommandIds.InsertSquare or CommandIds.InsertEllipse
            or CommandIds.InsertLine or CommandIds.InsertArrow or CommandIds.InsertTextbox => true,
        _ => false,
    };
}
```

### CommandRouter

Extend `Execute`:

```csharp
if (InsertShapeCommands.IsInsertShapeCommand(command))
    return ExecuteInsertShape(command);
```

`ExecuteInsertShape` delegates to `_host.InsertShape(command)` and returns `CommandExecutionResult`
with success message (match Web strings where sensible).

### ComHostAdapter

| Method | Behavior |
|--------|----------|
| `InsertShape(CommandIds command)` | Resolve active slide; COM add shape; no selection required |
| `GetActiveSlide()` (private) | `Application.ActiveWindow.View.Slide` or equivalent |

Constants for default geometry — single place (e.g. `InsertShapeDefaults.cs` or nested static class)
so manual QA and future settings can reference one source.

### HostScriptCommandMap

Extend `TryParse` return true for `InsertShapeCommands.IsInsertShapeCommand(command)`.

### Ribbon

Add group **`grpObjects`** (reference `VstoLegacy/UI/RibbonTab.xml`):

| Control id | Label | imageMso |
|------------|-------|----------|
| `btnInsertRectangle` | Rectangle | ShapeRectangle |
| `btnInsertSquare` | Square | ShapeRectangle |
| `btnInsertEllipse` | Circle | ShapeOval |
| `btnInsertLine` | Line | ShapeStraightLine |
| `btnInsertTextbox` | Textbox | TextBoxInsert |
| `btnInsertArrow` | Arrow | ShapeRightArrow |

All `onAction="OnHostScriptCommand"`.

Place after `grpPosition` or before `grpSize` in `RibbonTab.xml`.

Update `PptPowerKeys.Windows/README.md` — Objects section + manual QA rows.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/InsertShapeCommands.cs` | Command id guard |
| `Host/CommandRouter.cs` | `ExecuteInsertShape` branch |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | `InsertShape(CommandIds)` |
| `UI/HostScriptCommandMap.cs` | Include insert commands |
| `UI/RibbonTab.xml` | `grpObjects` 6 buttons |
| `PptPowerKeys.Windows/README.md` | Insert shapes docs |
| `PptPowerKeys.Tests/InsertShapeCommandsTests.cs` (optional) | `IsInsertShapeCommand` coverage |

## Анти-scope

- Duplicate / Group / Z-order / colors / text (S09-002…006)
- Custom insert position from selection or mouse
- Persist last-used shape size
- Core changes (pure COM host script)
- Sprint retrospective (architect post-merge)

## Критерии приёмки

- [ ] All 6 Insert* commands routed via `CommandRouter.Execute`
- [ ] Shapes appear on **active slide** without prior selection
- [ ] InsertSquare produces equal width/height
- [ ] InsertArrow uses arrow auto-shape (not plain line)
- [ ] Ribbon 6 buttons wired → `OnHostScriptCommand`
- [ ] Success messages shown (existing ribbon error/success pattern from S08)
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA note in PR (Windows sideload: each insert button)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- Sprint 08 Done (S08-001…005, PR #63–#71)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Insert* cases
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `insertShape`, `insertTextBox`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpObjects`

## Трассировка

Issue `#N` → `cursor/S09-001-insert-shapes-*` → PR `Closes #N`

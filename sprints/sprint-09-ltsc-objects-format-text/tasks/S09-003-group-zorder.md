# S09-003 — Group / Ungroup / Z-order (6 HostScript commands)

> Передача builder'у: `/builder выполни S09-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-003` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S09-001, S09-002 Done |

## Цель

Реализовать **Group / Ungroup + Z-order** на Windows line — 6 HostScript команд через COM.

| CommandId | COM | Success message (match Web) |
|-----------|-----|----------------------------|
| Group | `ShapeRange.Group()` | `Grouped N shape(s).` |
| Ungroup | `Shape.Ungroup()` on group | `Ungrouped.` |
| BringToFront | `Shape.ZOrder(msoBringToFront)` | `Brought to front.` |
| SendToBack | `Shape.ZOrder(msoSendToBack)` | `Sent to back.` |
| BringForward | `Shape.ZOrder(msoBringForward)` | `Brought forward.` |
| SendBackward | `Shape.ZOrder(msoSendBackward)` | `Sent backward.` |

Parity с Web Add-in `runCommand.ts` + `powerpoint.ts`.

**Regroup — anti-scope** (Web `OfficeJsSupport.None` → S10 unlock).

## Контекст (после S09-001…002)

| Комponent | Ожидаемое состояние |
|-----------|---------------------|
| `CommandRouter` | Insert* (6) + Duplicate* (4) + layout extras (38) |
| `HostScriptCommandMap` | Insert + Duplicate + CopyAndAlign + Position |
| `ComHostAdapter` | Insert, clone, bounds — **нет** group/ungroup/z-order |
| Web validation | Group ≥2 shapes; Ungroup exactly 1 group; Z-order all selected |
| Legacy ribbon | `VstoLegacy/UI/RibbonTab.xml` → `grpOrder` (6 кнопок) |

## Алгоритм (match Web)

### Group

```
1. selection = ActiveWindow.Selection
2. if Type != ppSelectionShapes OR ShapeRange.Count < 2
     → error "Select at least two shapes to group."
3. count = ShapeRange.Count
4. ShapeRange.Group()
5. message: "Grouped {count} shape(s)."
```

### Ungroup

```
1. selection = ActiveWindow.Selection
2. if Type != ppSelectionShapes OR Count != 1
     → error "Select exactly one group to ungroup."
3. shape = ShapeRange[1]
4. if shape.Type != MsoShapeType.msoGroup
     → error "Selected shape is not a group."
5. shape.Ungroup()
6. message: "Ungrouped."
```

### Z-order (BringToFront / SendToBack / BringForward / SendBackward)

```
1. selection = ActiveWindow.Selection
2. if no shapes selected → error "Select one or more shapes first." (consistent with other HostScript)
3. for each shape in ShapeRange (1..Count):
     shape.ZOrder(mapped MsoZOrderCmd)
4. message per command (see table above)
```

Web applies Z-order to **each** selected shape in selection order — повторить на COM.

## Решения architect

### GroupAndZOrderCommands helper

New file `Host/GroupAndZOrderCommands.cs` (или `ObjectOrderCommands.cs`):

```csharp
public static class GroupAndZOrderCommands
{
    public static bool IsGroupOrZOrderCommand(CommandIds command) => command switch
    {
        CommandIds.Group or CommandIds.Ungroup
            or CommandIds.BringToFront or CommandIds.SendToBack
            or CommandIds.BringForward or CommandIds.SendBackward => true,
        _ => false,
    };

    public static bool TryMapToZOrderCmd(CommandIds command, out MsoZOrderCmd zOrderCmd) { … }
}
```

`TryMapToZOrderCmd` — только для 4 Z-order ids; Group/Ungroup обрабатываются отдельно в router.

### CommandRouter

Extend `Execute`:

```csharp
if (GroupAndZOrderCommands.IsGroupOrZOrderCommand(command))
    return ExecuteGroupOrZOrder(command);
```

Handlers:

| Method | Delegates to |
|--------|--------------|
| `ExecuteGroup()` | `_host.GroupSelection()` → count in message |
| `ExecuteUngroup()` | `_host.UngroupSelection()` |
| `ExecuteZOrder(command)` | `_host.SetZOrder(msoCmd)` |

Return `CommandExecutionResult` (existing S08 pattern). Exceptions → `InvalidOperationException` with Web-matching text → ribbon `MessageBox`.

### Com...HostAdapter extensions

| Method | Behavior |
|--------|----------|
| `int GroupSelection()` | Validate ≥2 shapes; `ShapeRange.Group()`; return pre-group count |
| `void UngroupSelection()` | Validate exactly 1 group; `Ungroup()` |
| `int SetZOrder(MsoZOrderCmd cmd)` | Apply to all selected shapes; return count |

Private helper `GetSelectedShapeRange()` — reuse pattern from `ReadSelectedShapeBounds` (null/empty → throw).

**Note:** After `Group()`, selection typically becomes the new group — не требуется для success message (count taken before Group).

### HostScriptCommandMap

Extend `TryParse`:

```csharp
return CopyAndAlignCommands.IsCopyAndAlign(command)
    || PositionCommands.IsPositionCommand(command)
    || InsertShapeCommands.IsInsertShapeCommand(command)   // S09-001
    || DuplicateCommands.IsDuplicateCommand(command)       // S09-002
    || GroupAndZOrderCommands.IsGroupOrZOrderCommand(command);
```

### Ribbon

Add group **`grpOrder`** (reference `VstoLegacy/UI/RibbonTab.xml`):

| Control id | Label | imageMso | onAction |
|------------|-------|----------|----------|
| `btnBringToFront` | To Front | ObjectBringToFront | OnHostScriptCommand |
| `btnSendToBack` | To Back | ObjectSendToBack | OnHostScriptCommand |
| `btnBringForward` | Forward | ObjectBringForward | OnHostScriptCommand |
| `btnSendBackward` | Backward | ObjectSendBackward | OnHostScriptCommand |
| `btnGroup` | Group | ObjectsGroup | OnHostScriptCommand |
| `btnUngroup` | Ungroup | ObjectsUngroup | OnHostScriptCommand |

Разместить после `grpDuplicate` (S09-002) или `grpObjects` (S09-001) в `RibbonTab.xml`.

Update `PptPowerKeys.Windows/README.md` — Group/Ungroup/Z-order section + manual QA rows.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/GroupAndZOrderCommands.cs` | Command id guard + Z-order mapping |
| `Host/CommandRouter.cs` | Group/Ungroup/Z-order handlers |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | GroupSelection, UngroupSelection, SetZOrder |
| `UI/HostScriptCommandMap.cs` | Include group/z-order commands |
| `UI/RibbonTab.xml` | `grpOrder` 6 buttons |
| `PptPowerKeys.Windows/README.md` | Docs + manual QA |
| `PptPowerKeys.Tests/GroupAndZOrderCommandsTests.cs` (optional) | `IsGroupOrZOrderCommand`, Z-order mapping |

## Анти-scope

- **Regroup** (S10 unlock)
- Multi-slide paste/remove (S09-004)
- Format / Text commands (S09-005/006)
- Core changes (pure COM host script)
- Selection change after group (no auto-select new group)

## Критерии приёмки

- [ ] Group: error if `< 2` shapes; success `Grouped N shape(s).`
- [ ] Ungroup: errors match Web (`exactly one group`, `not a group`)
- [ ] 4 Z-order commands apply to **all** selected shapes
- [ ] Z-order success messages match Web strings
- [ ] Ribbon 6 buttons wired → `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` parses all 6 ids
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA note in PR (group 2 rects → ungroup; stack 3 shapes → front/back)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S09-001 Done (Insert shapes + HostScript pattern)
- S09-002 Done (Duplicate* + extended HostScriptCommandMap)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `groupSelectedShapes`, `ungroupSelectedShape`, `setZOrder`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Group, Ungroup, BringToFront… cases
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpOrder`

## Трассировка

Issue `#N` → `cursor/S09-003-group-zorder-*` → PR `Closes #N`

## Copy-paste промпт (новая сессия `/architect`)

```
/architect

Sprint 09 — S09-003 Group / Ungroup / Z-order (6 HostScript commands).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-003-group-zorder.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- sprints/sprint-09-ltsc-objects-format-text/backlog.md
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs
- src/PptPowerKeys.Windows/UI/HostScriptCommandMap.cs
- src/PptPowerKeys.Windows/Host/CopyAndAlignCommands.cs (pattern)
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (groupSelectedShapes, ungroupSelectedShape, setZOrder)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (Group, Ungroup, BringToFront…)
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpOrder)

S09-001…002 Done. Issue S09-003 → backlog In Progress → /builder выполни S09-003 → приёмка → merge.
Regroup — anti-scope (S10). После merge: backlog Done, PRODUCT_CONTEXT journal (S09-003).
```

# S09-003 — Group / Ungroup / Z-order (6 COM HostScript commands)

> Передача builder'у: `/builder выполни S09-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-003` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | In Progress |
| **Issue** | [#78](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/78) |
| **PR** | TBD |

## Цель

Реализовать **6 HostScript команд** Group / Ungroup / Z-order на Windows line (parity с Web Add-in
`groupSelectedShapes`, `ungroupSelectedShape`, `setZOrder`):

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| Group | `shapes.addGroup(selected)`; ≥2 shapes | `ShapeRange.Group()` |
| Ungroup | exactly 1 group → `group.ungroup()` | `Shape.Ungroup()` |
| BringToFront | `setZOrder(front)` on each selected | `Shape.ZOrder(msoBringToFront)` |
| SendToBack | `setZOrder(back)` | `Shape.ZOrder(msoSendToBack)` |
| BringForward | `setZOrder(forward)` | `Shape.ZOrder(msoBringForward)` |
| SendBackward | `setZOrder(backward)` | `Shape.ZOrder(msoSendBackward)` |

## Контекст (после S09-002)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | ServerLayout + CopyAndAlign + Position + Insert* + Duplicate*; Group/Ungroup/Z-order → `NotSupportedException` |
| `HostScriptCommandMap` | CopyAndAlign + Position + Insert* + Duplicate* |
| `RibbonTab.xml` | grpObjects + grpDuplicate; **нет** grpOrder |
| Web spec | `powerpoint.ts` ~356–427; `runCommand.ts` Group/Ungroup/BringToFront… cases |

## Алгоритм (зафиксировано — match Web)

### Group
```
1. range = selection.ShapeRange
2. if range.Count < 2 → error "Select at least two shapes to group."
3. range.Group()
4. return "Grouped N shape(s)."  // N = count before group
```

### Ungroup
```
1. range = selection.ShapeRange
2. if range.Count != 1 → error "Select exactly one group to ungroup."
3. if range[1].Type != msoGroup → error "Selected shape is not a group."
4. range[1].Ungroup()
5. return "Ungrouped."
```

### Z-order (BringToFront / SendToBack / BringForward / SendBackward)
```
1. range = selection.ShapeRange
2. if empty → error "Select one or more shapes first."
3. for each shape in range: shape.ZOrder(cmd)
4. return match runCommand.ts: "Brought to front." / "Sent to back." / "Brought forward." / "Sent backward."
```

## Решения architect

### CommandRouter API

```csharp
if (GroupZOrderCommands.IsGroupZOrderCommand(command)) → ExecuteGroupZOrder(command)
```

`GroupZOrderCommands.cs` — static `IsGroupZOrderCommand` + set of 6 ids (по аналогии с `InsertShapeCommands`).

### ComHostAdapter extensions

| Method | Behavior |
|--------|----------|
| `int GroupSelectedShapes()` | COM `ShapeRange.Group()`; throws if &lt;2 selected; returns pre-group count |
| `void UngroupSelectedShape()` | COM `Shape.Ungroup()`; throws if not exactly one group |
| `int ApplyZOrderToSelection(CommandIds command)` | COM `ZOrder` per shape; returns count updated |

Добавить в `IComHostAdapter` + `ComHostAdapter.cs`. Use `Microsoft.Office.Core.MsoZOrderCmd`.

### HostScriptCommandMap + Ribbon

- Новая группа **Order** (`grpOrder`) — 6 кнопок `btnBringToFront` … `btnUngroup`.
- Reference: `VstoLegacy/UI/RibbonTab.xml` grpOrder (labels/imageMso).
- `onAction="OnHostScriptCommand"`; расширить `HostScriptCommandMap.TryParse` для 6 ids.

### Tests (Linux CI)

- `GroupZOrderCommandsTests` — `IsGroupZOrderCommand` true/false.
- Расширить `HostScriptCommandMapTests` — 6 новых `btn*` cases.
- Link `GroupZOrderCommands.cs` в `PptPowerKeys.Tests.csproj`.
- `dotnet test PptPowerKeys.sln` green.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/GroupZOrderCommands.cs` | New — command set helper |
| `Host/IComHostAdapter.cs` | Group / Ungroup / ZOrder methods |
| `Host/ComHostAdapter.cs` | COM Group/Ungroup/ZOrder |
| `Host/CommandRouter.cs` | Route 6 commands |
| `UI/HostScriptCommandMap.cs` | Parse 6 btn ids |
| `UI/RibbonTab.xml` | grpOrder (6 buttons) |
| `PptPowerKeys.Windows.csproj` | Compile new file |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Group/Ungroup/Z-order section |

## Анти-scope

- **Regroup** (S10-003 — Web None unlock)
- Multi-slide paste/remove (S09-004)
- Format / Text (S09-005…006)
- HTTP Api / AddIn changes
- Core changes
- VstoLegacy edits

## Критерии приёмки

- [ ] All 6 commands routed via `CommandRouter.Execute`
- [ ] Group requires ≥2 shapes; Ungroup requires exactly one group
- [ ] Z-order applies to all selected shapes; empty selection → error
- [ ] Status messages match `runCommand.ts` strings
- [ ] Ribbon grpOrder (6 buttons) wired via `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` + `GroupZOrderCommands` unit tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR manual QA note + `.github/review/CHECKLIST.md`

## Зависимости

- S09-001 Done (#73 / #74)
- S09-002 Done (#75 / #76)

## Reference files (Web spec)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `groupSelectedShapes`, `ungroupSelectedShape`, `setZOrder`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Group/Ungroup/BringToFront… cases + messages
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — grpOrder button ids/imageMso
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs` — Group/Ungroup/Z-order metadata

## Трассировка

Issue `#78` → `cursor/S09-003-group-zorder-e5fe` → PR `Closes #78`

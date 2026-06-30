# S09-003 — Group / Ungroup / Z-order (4 HostScript commands)

> Передача builder'у: `/builder выполни S09-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-003` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Зависимости** | S09-002 Done |

## Цель

| CommandId | COM |
|-----------|-----|
| Group | `ShapeRange.Group()` — ≥2 shapes |
| Ungroup | `GroupItems.Ungroup()` — exactly 1 group |
| BringToFront, SendToBack, BringForward, SendBackward | `ZOrder(msoBringToFront)` etc. |

**Regroup — anti-scope** (Web None → S10).

## Алгоритм (match Web `powerpoint.ts`)

- Group: error if `< 2` selected
- Ungroup: error if not exactly one group shape
- Z-order: apply to all selected shapes

## Решения architect

- `ObjectCommands.cs` or split `GroupCommands` + `ZOrderCommands`.
- `ComHostAdapter`: `GroupSelection()`, `UngroupSelection()`, `SetZOrder(ZOrderAction)`.
- Ribbon `grpOrder` (4 Z-order) + Group/Ungroup buttons (legacy `grpOrderWeek` or extend `grpObjects`).
- Extend `HostScriptCommandMap`.

## Критерии приёмки

- [ ] Group/Ungroup with validation messages match Web
- [ ] 4 Z-order commands on multi-selection
- [ ] Ribbon wired
- [ ] `dotnet test PptPowerKeys.sln` green

## Reference

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `groupSelectedShapes`, `ungroupSelectedShape`, `setZOrder`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpOrder`

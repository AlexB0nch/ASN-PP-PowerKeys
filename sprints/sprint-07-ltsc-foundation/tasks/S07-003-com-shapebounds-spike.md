# S07-003 — ComHost ShapeBounds spike + AlignLeft POC

> Передача builder'у: `/builder выполни S07-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S07-003` |
| **Спринт** | `sprint-07-ltsc-foundation` |
| **Компонент** | Windows + Core |
| **Статус** | Todo |

## Цель

Proof-of-concept: **AlignLeft** end-to-end через COM host без HTTP:

```
Selection → ShapeBounds[] → Core.LayoutEngine → write back Left/Top/Width/Height
```

## Scope

| Item | Detail |
|------|--------|
| `ComHostAdapter` | Read selected shapes → `ShapeBounds[]`; apply results by shape id/name |
| `CommandRouter` | Minimal: route `AlignLeft` only |
| Anchor | Last selected shape in selection order |
| Ribbon | Wire AlignLeft button to router |
| Tests | Unit tests for adapter mapping where mockable; manual PP note required |

## Reference

- Core: `LayoutEngine`, `ShapeBounds`
- Web spec: `getSelectedShapeBounds` / `applyShapeBounds` in AddIn
- Mapping doc: `docs/migration/01-vsto-to-officejs-mapping.md`

## Анти-scope

- Other 78 commands
- ShortcutManager
- Task pane UI

## Критерии приёмки

- [ ] Manual: 2+ shapes, select anchor last, AlignLeft works in PowerPoint
- [ ] No HTTP to Api for this command path
- [ ] Core.LayoutEngine invoked in-process
- [ ] Code structured for S08 expansion (interfaces documented)

## Зависимости

- S07-001, S07-002 Done

## Трассировка

Issue → branch → PR

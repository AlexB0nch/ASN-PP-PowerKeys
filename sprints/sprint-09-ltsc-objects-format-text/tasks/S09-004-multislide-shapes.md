# S09-004 — Multi-slide paste/remove shapes (2 HostScript commands)

> Передача builder'у: `/builder выполни S09-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-004` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Зависимости** | S09-003 Done |

## Цель

| CommandId | Behavior |
|-----------|----------|
| PasteShapeToSelectedSlides | Clone selected shape(s) to each slide in selection |
| RemoveShapeFromSelectedSlides | Delete shapes **by name match** on selected slides |

Parity Web `powerpoint.ts` multi-slide helpers.

## Алгоритм (outline)

### PasteShapeToSelectedSlides

```
1. Require shape selection on source slide
2. slideRange = all selected slides in sorter/thumbnail selection
3. For each target slide ≠ source: COM duplicate shape, preserve name/geometry offset as Web
```

### RemoveShapeFromSelectedSlides

```
1. Read names from selected shapes on active slide
2. For each selected slide: delete shapes whose Name matches (case-sensitive per Web)
```

## Решения architect

- `ComHostAdapter.GetSelectedSlideIndices()`, `DuplicateShapeToSlide`, `RemoveShapesByNameOnSlides`.
- User-facing counts in `CommandExecutionResult.Message`.
- Ribbon buttons in Objects group (or dedicated multi-slide subgroup).

## Анти-scope

- CopySlide / MoveSlidesToBackup (Slides category, S10)

## Критерии приёмки

- [ ] Paste to 2+ slides works from shape selection
- [ ] Remove by name on multi-slide selection
- [ ] Error paths when no slides/shapes selected
- [ ] `dotnet test PptPowerKeys.sln` green

## Reference

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — paste/remove multi-slide functions
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`

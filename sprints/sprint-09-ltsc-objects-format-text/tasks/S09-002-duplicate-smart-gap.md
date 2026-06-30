# S09-002 — Duplicate* + smart gap (4 HostScript commands)

> Передача builder'у: `/builder выполни S09-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-002` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Зависимости** | S09-001 Done |

## Цель

4 команды **DuplicateRight/Left/Up/Down** с **smart gap memory** (parity Web `duplicateGapMemory.ts`).

## Алгоритм (match Web `runCommand.ts`)

```
1. sources = ReadSelectedShapeBounds(); empty → error
2. gap = DuplicateGapStore.Get(command)  // session, per-direction
3. for each source: target = DuplicationEngine.ComputeDuplicate(command, source, gap)
4. CloneSelectedAtSourcePositions + move clones to target Left/Top (reuse S08-004 patterns)
5. InferGap from actual positions → DuplicateGapStore.Set(command, gap)
6. message: "Duplicated N shape(s) (gap X pt)."
```

## Решения architect

- `DuplicateGapStore.cs` — in-memory, session scope (like Web localStorage per command id).
- `DuplicateCommands.cs` — `IsDuplicateCommand` wraps Core `DuplicationEngine.IsDuplicateCommand`.
- Reuse `ComHostAdapter.CloneSelectedAtSourcePositions` + `ApplyShapeBoundsOnSlide`.
- Ribbon `grpDuplicate` (4 buttons) → `OnHostScriptCommand`.
- Optional Core tests already cover `DuplicationEngine`; add Windows store unit tests if trivial.

## Анти-scope

- Insert shapes (S09-001), Group/Z-order (S09-003)

## Критерии приёмки

- [ ] 4 Duplicate* commands work with selection
- [ ] Gap remembered per direction within session
- [ ] Core `DuplicationEngine.ComputeDuplicate` used in-process
- [ ] Ribbon 4 buttons
- [ ] `dotnet test PptPowerKeys.sln` green

## Reference

- `src/PptPowerKeys.Core/Layout/DuplicationEngine.cs`
- `src/PptPowerKeys.AddIn/src/office/duplicateGapMemory.ts`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpDuplicate`

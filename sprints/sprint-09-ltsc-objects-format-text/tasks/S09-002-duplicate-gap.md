# S09-002 — Duplicate* + smart gap (Windows HostScript)

> Передача builder'у: `/builder выполни S09-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-002` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (`DuplicationEngine`) |
| **Статус** | In Progress |
| **Issue** | [#75](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/75) |

## Цель

Реализовать **4 Duplicate* HostScript** на Windows line с **Figma-like gap memory** (parity с Web Add-in
`duplicateGapMemory.ts` / S05-005):

| CommandId | Web behavior | Windows target |
|-----------|--------------|----------------|
| DuplicateRight | clone + `DuplicationEngine.ComputeDuplicate` + gap memory | COM `Shape.Duplicate()` + Core offset |
| DuplicateLeft | same | same |
| DuplicateDown | same | same |
| DuplicateUp | same | same |

## Контекст (после S09-001)

| Компонент | Состояние |
|-----------|-----------|
| `DuplicationEngine` | `ComputeDuplicate` + `InferGap` + tests — в Core (S02/S05) |
| `CommandRouter` | ServerLayout + CopyAndAlign + Position + Insert*; Duplicate* → `NotSupportedException` |
| `HostScriptCommandMap` | CopyAndAlign + Position + Insert* |
| Web spec | `runCommand.ts` Duplicate* cases; `duplicateGapMemory.ts` |

## Алгоритм (зафиксировано — match Web)

```
1. sources = ReadSelectedShapeBounds()
2. if sources empty → error "Select one or more shapes first."
3. gap = DuplicateGapStore.GetGap(command)   // 0 if first time in direction
4. for each source:
     target = DuplicationEngine.ComputeDuplicate(command, source, gap)
5. count = DuplicateSelectedAtPositions(sources, targets)  // COM clone + set Left/Top
6. DuplicateGapStore.SetGap(command, gap)
7. return "Duplicated N shape(s)" or "Duplicated N shape(s) (gap X pt)" when gap > 0
```

**Gap memory:** per `CommandId`, in-memory session scope (как `PositionClipboardStore`), **не** persisted.

## Решения architect

### CommandRouter API

```csharp
if (DuplicateCommands.IsDuplicateCommand(command)) → ExecuteDuplicate(command)
```

`DuplicateCommands.cs` — thin wrapper over `DuplicationEngine.IsDuplicateCommand`.

### ComHostAdapter extension

| Method | Behavior |
|--------|----------|
| `DuplicateSelectedAtPositions(sources, targets)` | COM `Duplicate()` per source; set clone `Left`/`Top` from targets; preserve W/H |

### DuplicateGapStore

In-memory `Dictionary<CommandIds, double>`; `GetGap` → 0 default; `SetGap` clamps to ≥ 0.

### HostScriptCommandMap + Ribbon

- Новая группа **Duplicate** (`grpDuplicate`) — 4 кнопки `btnDuplicateRight` … `btnDuplicateUp`.
- Reference: `VstoLegacy/UI/RibbonTab.xml` grpDuplicate (labels/imageMso).
- `onAction="OnHostScriptCommand"`.

### Tests (Linux CI)

- `DuplicateCommandsTests` — `IsDuplicateCommand` true/false.
- `DuplicateGapStoreTests` — get/set/clear, negative clamp.
- Расширить `HostScriptCommandMapTests` — 4 `btnDuplicate*` cases.
- Link new files в `PptPowerKeys.Tests.csproj`.
- `dotnet test PptPowerKeys.sln` green.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/DuplicateCommands.cs` | New — command set helper |
| `Host/DuplicateGapStore.cs` | New — in-memory gap memory |
| `Host/IComHostAdapter.cs` | `DuplicateSelectedAtPositions` |
| `Host/ComHostAdapter.cs` | COM duplicate + position |
| `Host/CommandRouter.cs` | Route Duplicate* |
| `UI/HostScriptCommandMap.cs` | Parse 4 btn ids |
| `UI/RibbonTab.xml` | grpDuplicate (4 buttons) |
| `PptPowerKeys.Windows.csproj` | Compile new files (+ missing Position* if needed) |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Duplicate section |

## Анти-scope

- Core `DuplicationEngine` changes (already complete)
- Group / Z-order (S09-003)
- Api / AddIn changes
- localStorage / UserSettings persist gap
- VstoLegacy edits

## Критерии приёмки

- [ ] All 4 Duplicate commands routed via `CommandRouter.Execute`
- [ ] First duplicate in direction uses gap=0; repeat uses remembered gap
- [ ] Status message includes `(gap X pt)` when gap > 0
- [ ] Ribbon grpDuplicate (4 buttons) wired via `OnHostScriptCommand`
- [ ] `HostScriptCommandMap` + `DuplicateCommands` + `DuplicateGapStore` unit tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR manual QA note + `.github/review/CHECKLIST.md`

## Зависимости

- S09-001 Done (#74)

## Трассировка

Issue `#75` → `cursor/S09-002-duplicate-gap-2893` → PR `Closes #75`

# Architect Kickoff — Sprint 09 LTSC Objects · Format · Text

> **Статус:** Ready to start. Первая задача: **S09-001**.

## Контекст

- **Sprint 08 Done** — M2 Layout beta: 38 команд (32 ServerLayout + 4 CopyAndAlign + 2 position).
  Последняя задача **S08-005** merged PR [#71](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/71) (Issue [#70](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/70)).
- `CommandRouter` + `ComHostAdapter` + `OnHostScriptCommand` / `HostScriptCommandMap` готовы к расширению.
- Web line Sprint 01–06 стабилен; Linux CI green (**169** dotnet tests после S08).

## Цель Sprint 09

**M3 Feature beta (wave 1):** HostScript parity для **Objects (19) + Format (5) + Text (6)** = **30 команд** через COM.

Parity matrix: [`../epic-ltsc-windows-native/FEATURE_PARITY.md`](../epic-ltsc-windows-native/FEATURE_PARITY.md).

### Anti-scope спринта

| Команда | Причина | Когда |
|---------|---------|-------|
| **Regroup** | Web `OfficeJsSupport.None` | S10 unlock |
| **PasteFormatted** | Web None | S10 unlock |
| **FormatPainter** | Web None | S10 unlock |
| Slides / Settings | Отдельный спринт | S10 |

Итого в S09: **30** HostScript (не 33).

## Задачи спринта

| ID | Файл | Команды | Builder |
|----|------|---------|---------|
| S09-001 | [`tasks/S09-001-insert-shapes.md`](./tasks/S09-001-insert-shapes.md) | Insert* (6) | `/builder выполни S09-001` |
| S09-002 | [`tasks/S09-002-duplicate-smart-gap.md`](./tasks/S09-002-duplicate-smart-gap.md) | Duplicate* (4) | после 001 |
| S09-003 | [`tasks/S09-003-group-zorder.md`](./tasks/S09-003-group-zorder.md) | Group, Ungroup, Z-order (4) | после 002 |
| S09-004 | [`tasks/S09-004-multislide-shapes.md`](./tasks/S09-004-multislide-shapes.md) | Paste/Remove multi-slide (2) | после 003 |
| S09-005 | [`tasks/S09-005-format-colors-palette.md`](./tasks/S09-005-format-colors-palette.md) | Fill/Line/Text color + palette (5) | после 004 |
| S09-006 | [`tasks/S09-006-text-addup.md`](./tasks/S09-006-text-addup.md) | Text + Addup (6) | после 005 |

## Инварианты

- Новый код только в `src/PptPowerKeys.Windows/` — **не** размораживать `VstoLegacy*`.
- Web spec для HostScript: `AddIn/src/office/powerpoint.ts`, `runCommand.ts`.
- Core helpers переиспользовать in-process: `DuplicationEngine`, `ColorPaletteBuilder`, `NumberAggregator`.
- `dotnet test PptPowerKeys.sln` must stay green (Core-only tests; COM manual on Windows).
- HostScript ribbon ids: `btn{CommandIds}` → `HostScriptCommandMap.TryParse`.
- Паттерн S08-004/005: `CommandExecutionResult` + user-facing `Message`.

## Архитектура HostScript (расширение S08)

```
OnHostScriptCommand
  → HostScriptCommandMap.TryParse
  → CommandRouter.Execute(CommandIds)
       ├─ LayoutEngine.IsLayoutCommand     (32) — S08
       ├─ CopyAndAlignCommands               (4) — S08
       ├─ PositionCommands                   (2) — S08
       ├─ InsertShapeCommands                (6) — S09-001
       ├─ DuplicateCommands                  (4) — S09-002
       ├─ ObjectCommands (group/z/multi)    (10) — S09-003/004
       ├─ FormatCommands                     (5) — S09-005
       └─ TextCommands                       (6) — S09-006
```

`ComHostAdapter` — все COM read/write; Core — чистая математика/агрегация.

## Процесс сессии

1. Issue **S09-001** → backlog In Progress
2. `/builder выполни S09-001`
3. Приёмка PR → merge → Done
4. Повторить S09-002…006
5. `retrospective.md` → kickoff S10

## Copy-paste промпт (S09 kickoff → S09-001)

```
/architect

Sprint 09 — LTSC Objects · Format · Text (HostScript wave 2, 30 команд).
Sprint 08 Done (#63–#71). S08-005 merged.

Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/ARCHITECT-KICKOFF.md
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-001-insert-shapes.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- sprints/epic-ltsc-windows-native/FEATURE_PARITY.md (Objects/Format/Text)
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/UI/HostScriptCommandMap.cs
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (insertShape, insertTextBox)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (Insert* cases)
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpObjects reference)

Issue S09-001 → backlog In Progress → /builder выполни S09-001 → приёмка → merge.
После S09-006: retrospective.md, goals DoD, PRODUCT_CONTEXT, kickoff S10.
```

## Copy-paste промпт (S09-003 — Group / Z-order)

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

## Copy-paste промпт (S09-006 — закрытие спринта)

```
/architect

Sprint 09 — S09-006 Text + Addup (последняя задача спринта).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-006-text-addup.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- src/PptPowerKeys.Core/Text/NumberAggregator.cs
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (Text cases)

S09-001…005 Done. Issue S09-006 → backlog In Progress → /builder S09-006 → приёмка → merge.
После merge: retrospective.md, goals DoD, PRODUCT_CONTEXT, закрыть Sprint 09 → kickoff S10.
```

# Architect Kickoff — Sprint 09 LTSC Objects · Format · Text

> **Статус:** Ready to start. Первая задача: **S09-001**.

## Контекст

- Sprint 08 **Done** — M2 Layout beta: 32 ServerLayout + 6 layout HostScript (CopyAndAlign + Position).
- `CommandRouter` + `ComHostAdapter` + `HostScriptCommandMap` готовы к расширению wave 2.
- Web line Sprint 01–06 стабилен; Linux CI green (`dotnet test` 169 passed at S08 close).

## Цель Sprint 09

**M3 Objects/Format/Text beta:** 30 HostScript команд (19 Objects + 5 Format + 6 Text) через COM, parity с Web spec в `powerpoint.ts` / `runCommand.ts`.

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S09-001 | [`tasks/S09-001-insert-shapes.md`](./tasks/S09-001-insert-shapes.md) | `/builder выполни S09-001` |
| S09-002 | `tasks/S09-002-duplicate-gap.md` (создать при старте) | после 001 |
| S09-003 | `tasks/S09-003-group-zorder.md` (создать) | после 002 |
| S09-004 | `tasks/S09-004-multi-slide-shapes.md` (создать) | после 003 |
| S09-005 | `tasks/S09-005-format-colors-palette.md` (создать) | после 004 |
| S09-006 | `tasks/S09-006-text-addup.md` (создать) | после 005 |

## Инварианты

- Web spec (`AddIn/src/office/powerpoint.ts`, `runCommand.ts`) — эталон поведения HostScript.
- `dotnet test PptPowerKeys.sln` must stay green (link Windows pure helpers into Tests).
- Не размораживать `VstoLegacy*` (только ribbon reference).
- `HostScriptCommandMap` расширяется; layout-команды остаются в `RibbonCommandMap`.

## Процесс сессии

1. Issue S09-001 → backlog **In Progress**
2. `/builder выполни S09-001`
3. Приёмка PR → merge → Done
4. Повторить для S09-002…006
5. `retrospective.md` → goals DoD → `PRODUCT_CONTEXT` → kickoff S10

## Copy-paste промпт (S09-001)

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

## Copy-paste промпт (S09-006 — закрытие спринта)

```
/architect

Sprint 09 — S09-006 Text + Addup (последняя задача спринта).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-006-text-addup.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md

S09-001…005 Done. Issue S09-006 → backlog In Progress → /builder S09-006 → приёмка → merge.
После merge: retrospective.md, goals DoD, PRODUCT_CONTEXT, закрыть Sprint 09 → kickoff S10.
```

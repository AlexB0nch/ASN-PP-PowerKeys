# Architect Kickoff — Sprint 07 LTSC Foundation

> **Статус:** Done (2026-06-29). См. [`retrospective.md`](./retrospective.md). Следующий: **Sprint 08**.

## Контекст

- Web Add-in Sprint 01–06 **Done** (79 cmd, primary line).
- LTSC users не могут sideload через Upload My Add-in / путают с `.ppam`.
- **ADR-001:** новая line `PptPowerKeys.Windows` (VSTO + Core in-process).

## Цель Sprint 07

M1 prototype: VSTO loads; **AlignLeft** via Core in-process.

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S07-001 | [tasks/S07-001-core-multitarget-netstandard.md](./tasks/S07-001-core-multitarget-netstandard.md) | `/builder выполни S07-001` |
| S07-002 | [tasks/S07-002-vsto-windows-host-bootstrap.md](./tasks/S07-002-vsto-windows-host-bootstrap.md) | после merge 001 |
| S07-003 | [tasks/S07-003-com-shapebounds-spike.md](./tasks/S07-003-com-shapebounds-spike.md) | после 001+002 |
| S07-004 | [tasks/S07-004-track0-ltsc-deploy-checklist.md](./tasks/S07-004-track0-ltsc-deploy-checklist.md) | параллельно |

## Инварианты

- **Не** редактировать `VstoLegacy*` для новых фич — только читать как reference.
- Новый код: `src/PptPowerKeys.Windows/`, `PptPowerKeys.Windows.sln`.
- Core changes must keep `dotnet test PptPowerKeys.sln` green.

## Процесс сессии

1. Issue S07-001 → backlog In Progress
2. `/builder выполни S07-001`
3. Приёмка PR → merge → Done
4. Повторить для S07-002…004
5. `retrospective.md` → kickoff S08

## Copy-paste промпт

```
/architect

Sprint 07 — LTSC Foundation (PptPowerKeys.Windows).
Прочитай:
- docs/migration/04-powerpoint-ltsc-windows-native.md
- docs/adr/ADR-001-ltsc-windows-native-line.md
- sprints/sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md
- sprints/sprint-07-ltsc-foundation/tasks/S07-001-core-multitarget-netstandard.md

Создай Issue S07-001, backlog In Progress, запусти /builder выполни S07-001.
После merge — S07-002 и далее по backlog.
```

# Architect Kickoff — Sprint 08 LTSC Layout Parity

> **Статус:** Ready to start. Первая задача: **S08-001**.

## Контекст

- Sprint 07 **Done** — M1: `PptPowerKeys.Windows` loads; AlignLeft via Core in-process.
- `ComHostAdapter` + `CommandRouter` готовы к расширению.
- Web line Sprint 01–06 стабилен; Linux CI green.

## Цель Sprint 08

**M2 Layout beta:** все 32 ServerLayout команды + snap-to-grid + copy-and-align extras.

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S08-001 | [`tasks/S08-001-serverlayout-pipeline-all-32.md`](./tasks/S08-001-serverlayout-pipeline-all-32.md) | `/builder выполни S08-001` |
| S08-002 | `tasks/S08-002-snap-to-grid-settings.md` (создать) | после merge 001 |
| S08-003 | `tasks/S08-003-ribbon-layout-group.md` (создать) | после 001 |
| S08-004 | `tasks/S08-004-copy-and-align-hostscript.md` (создать) | после 001–003 |
| S08-005 | `tasks/S08-005-position-clipboard-qa.md` (создать) | после 004 |

## Инварианты

- `ShapeBounds` boundary; anchor = последняя выделенная.
- `dotnet test PptPowerKeys.sln` must stay green.
- Не размораживать `VstoLegacy*`.

## Процесс сессии

1. Issue S08-001 → backlog In Progress
2. `/builder выполни S08-001`
3. Приёмка PR → merge → Done
4. Повторить для S08-002…005
5. `retrospective.md` → kickoff S09

## Copy-paste промпт

```
/architect

Sprint 08 — LTSC Layout Parity (PptPowerKeys.Windows).
Прочитай:
- sprints/sprint-08-ltsc-layout-parity/ARCHITECT-KICKOFF.md
- sprints/sprint-07-ltsc-foundation/retrospective.md
- docs/migration/04-powerpoint-ltsc-windows-native.md

Создай Issue S08-001, backlog In Progress, запусти /builder выполни S08-001.
```

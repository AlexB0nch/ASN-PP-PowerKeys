# Architect Kickoff — Epic LTSC Windows Native (Product Line B)

> **Первая сессия epic:** kickoff **Sprint 07** → задача **S07-001**.  
> Web Add-in (Sprint 01–06) **Done** — primary line без изменений приоритета.

## Прочитать перед работой

1. [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)
2. [`docs/adr/ADR-001-ltsc-windows-native-line.md`](../../docs/adr/ADR-001-ltsc-windows-native-line.md)
3. [`ROADMAP.md`](./ROADMAP.md) · [`FEATURE_PARITY.md`](./FEATURE_PARITY.md)
4. [`../sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md`](../sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md)
5. `docs/PRODUCT_CONTEXT.md` · `src/PptPowerKeys.VstoLegacy/FROZEN.md`

## Решение architect (зафиксировано)

**Предпочтительный путь:** Variant **D** — VSTO/COM **`PptPowerKeys.Windows`** + in-process **Core** + optional Companion.

**Не:** PPAM/VBA · не размораживать `VstoLegacy*` · не companion-only.

## Multi-sprint plan S07–S11

| Sprint | Фокус |
|--------|-------|
| S07 | Foundation, spikes, VSTO shell |
| S08 | Layout parity (38 cmd) |
| S09 | Objects, Format, Text |
| S10 | Slides, None unlock, Settings |
| S11 | Hotkeys, MSI, ship |

## Процесс каждой сессии

1. Issue по `.github/ISSUE_TEMPLATE/task.yml` (Task ID `S07-0YY`)
2. Backlog → In Progress
3. `/builder выполни S07-0YY`
4. Приёмка PR + `.github/review/CHECKLIST.md`
5. Post-merge: backlog Done, journal в `PRODUCT_CONTEXT.md`
6. После последней задачи спринта — `retrospective.md`

## Copy-paste: старт Sprint 07

```
/architect

Epic: PptPowerKeys.Windows (LTSC line). Sprint 07 — Foundation.
Прочитай:
- docs/migration/04-powerpoint-ltsc-windows-native.md
- sprints/epic-ltsc-windows-native/ROADMAP.md
- sprints/sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md
- sprints/sprint-07-ltsc-foundation/tasks/S07-001-core-multitarget-netstandard.md

Задача сессии: Issue для S07-001 → backlog In Progress → /builder S07-001 → приёмка → merge.
Не размораживать VstoLegacy — новый проект PptPowerKeys.Windows в S07-002.
```

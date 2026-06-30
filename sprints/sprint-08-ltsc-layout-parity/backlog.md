# Sprint 08 — Backlog

> Epic LTSC Windows Native · Architect kickoff: [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md)

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S08-001 | CommandRouter: all 32 ServerLayout commands | Windows + Core | **Done** | [#62](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/62) / [#63](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/63) |
| S08-002 | Snap-to-grid (LayoutOptions + local UserSettings) | Windows + Core | **Done** | [#64](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/64) / [#65](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/65) |
| S08-003 | Ribbon layout command group (32 ServerLayout) | Windows UI | **In Progress** | [#66](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/66) |
| S08-004 | Copy-and-align HostScript (4 commands) | Windows | **Todo** | — |
| S08-005 | Position clipboard + layout QA notes | Windows + docs | **Todo** | — |

## Порядок исполнения

1. **S08-001** ✓ — generic ServerLayout dispatch
2. **S08-002** ✓ — snap flag + `%AppData%` settings + ribbon checkbox
3. **S08-003** — ribbon buttons для layout cmds
4. **S08-004** — CopyAndAlign HostScript
5. **S08-005** — position clipboard + QA matrix

## Черновик S08-003 (следующая после S08-002)

> [`tasks/S08-003-ribbon-layout-group.md`](./tasks/S08-003-ribbon-layout-group.md)

- 32 ribbon buttons → generic `OnLayoutCommand` → `CommandRouter`
- 6 groups: Alignment, Stack, Size, Stretch, Nudge L/S
- Remove Bootstrap Test button

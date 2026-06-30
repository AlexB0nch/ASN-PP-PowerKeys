# Sprint 08 — Backlog

> Epic LTSC Windows Native · Architect kickoff: [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md)

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S08-001 | CommandRouter: all 32 ServerLayout commands | Windows + Core | **Done** | [#62](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/62) / [#63](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/63) |
| S08-002 | Snap-to-grid (LayoutOptions + local UserSettings) | Windows + Core | **Done** | [#64](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/64) / [#65](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/65) |
| S08-003 | Ribbon layout command group (32 ServerLayout) | Windows UI | **Done** | [#66](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/66) / [#67](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/67) |
| S08-004 | Copy-and-align HostScript (4 commands) | Windows | **Done** | [#68](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/68) / [#69](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/69) |
| S08-005 | Position clipboard + layout QA notes | Windows + docs | **In Progress** | [#70](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/70) |

## Порядок исполнения

1. **S08-001** ✓ — generic ServerLayout dispatch
2. **S08-002** ✓ — snap flag + `%AppData%` settings + ribbon checkbox
3. **S08-003** ✓ — ribbon buttons для layout cmds
4. **S08-004** ✓ — CopyAndAlign HostScript
5. **S08-005** — position clipboard + QA matrix ← **next**

## Черновик S08-005 (последняя задача Sprint 08)

> [`tasks/S08-005-position-clipboard-layout-qa.md`](./tasks/S08-005-position-clipboard-layout-qa.md)

- CopyObjectPosition / PasteObjectPosition (in-memory clipboard, Left/Top only)
- `docs/migration/06-windows-layout-qa.md` — consolidated M2 manual matrix
- Architect post-merge: `retrospective.md`, close Sprint 08

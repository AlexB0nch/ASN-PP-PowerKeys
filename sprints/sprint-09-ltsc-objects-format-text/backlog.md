# Sprint 09 — Backlog

> Epic LTSC Windows Native · Architect kickoff: [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md)

> **Status:** Ready — Sprint 08 Done (PR #71 merged).

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S09-001 | Insert shapes (6) | Windows | **Todo** | — |
| S09-002 | Duplicate* + smart gap | Windows + Core | **Todo** | — |
| S09-003 | Group / Ungroup / Z-order (4) | Windows | **Todo** | — |
| S09-004 | Multi-slide paste/remove shapes | Windows | **Todo** | — |
| S09-005 | Format colors + palette | Windows + Core | **Todo** | — |
| S09-006 | Text + Addup | Windows + Core | **Todo** | — |

## Порядок исполнения

1. **S09-001** — Insert* HostScript + ribbon `grpObjects`
2. **S09-002** — Duplicate* + `DuplicateGapStore` + Core `DuplicationEngine`
3. **S09-003** — Group/Ungroup + Z-order (Regroup → S10)
4. **S09-004** — PasteShapeToSelectedSlides / RemoveShapeFromSelectedSlides
5. **S09-005** — Fill/Line/Text color + recent palette (FormatPainter → S10)
6. **S09-006** — Text commands + Addup (PasteFormatted → S10)

**30 HostScript commands** total (Objects 17 + Format 5 + Text 5 + Addup counted in Text; Regroup/PasteFormatted/FormatPainter deferred).

Task files: [`tasks/`](./tasks/)

## Предыдущий спринт

Sprint 08 **Done** — [`../sprint-08-ltsc-layout-parity/retrospective.md`](../sprint-08-ltsc-layout-parity/retrospective.md)  
S08-005 merged: Issue [#70](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/70) / PR [#71](https://github.com/AlexB0nch/ASN-PP-PowerKeys/pull/71)


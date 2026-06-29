# Sprint 07 — Backlog

> Epic LTSC Windows Native · Architect kickoff: [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md)

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S07-001 | Core multitarget netstandard2.0 | Core + Tests | **Done** | PR #59 |
| S07-002 | PptPowerKeys.Windows VSTO bootstrap | Windows (new) | **Done** | PR #60 |
| S07-003 | ComHost ShapeBounds spike + AlignLeft POC | Windows + Core | **Done** | PR #61 |
| S07-004 | Track 0 LTSC deploy checklist (Central Deployment) | docs | **Done** | PR |

## Порядок исполнения

1. **S07-001** — блокирует ссылку Core из .NET Framework host
2. **S07-002** — VSTO shell (parallel после spike design, но до S07-003 integration)
3. **S07-003** — зависит от S07-001 + S07-002
4. **S07-004** — параллельно (docs-only)

## Черновик S07-001 (первая задача builder)

> [`tasks/S07-001-core-multitarget-netstandard.md`](./tasks/S07-001-core-multitarget-netstandard.md)

- `PptPowerKeys.Core.csproj`: `<TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>`
- Убрать/условно компилировать API-incompatible APIs для netstandard2.0
- Все существующие тесты зелёные на net8.0 (CI unchanged)

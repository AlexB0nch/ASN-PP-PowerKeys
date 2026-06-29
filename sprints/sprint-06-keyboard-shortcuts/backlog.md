# Sprint 06 — Backlog

> Architect декомпозировал Sprint 06 (2026-06-29). См. [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S06-001 | Shared Runtime + Keyboard Shortcuts (Tier 1) | AddIn + manifest + scripts + Tests | **Done** | #44 / PR #46 |
| S06-002 | `replaceShortcuts` ↔ UserSettings (все bindings) | AddIn | **Done** | #48 / PR #49 |
| S06-003 | Import/export settings JSON | Core + Api + AddIn | **Done** | #51 / PR #52 |
| S06-004 | Object Statistics MIN/MAX/AVG UI | AddIn | **Todo** | — |
| S06-005 | Color Picker eyedropper / HEX (stretch) | AddIn | **Todo** | — |

## Порядок исполнения (рекомендация architect)

1. **S06-001** — инфраструктура shared runtime + Tier 1 defaults; блокирует S06-002
2. **S06-002** — live sync Shortcut Manager; consulting profile apply → `replaceShortcuts`
3. **S06-004** — P2, следующая задача (Addup display mode UI)
4. **S06-005** — P3 deferred из Sprint 04

## Черновик S06-004 (постановка после merge S06-003)

> Полная спецификация: [`tasks/S06-004-object-statistics-min-max-avg-ui.md`](./tasks/S06-004-object-statistics-min-max-avg-ui.md)

- Core `NumberAggregator` + API `/api/text/addup` — **готовы**; status bar показывает все метрики сразу
- UI: dropdown **All | Sum | Min | Max | Average** в Settings (+ optional «Last addup» в Text)
- Persist `addupDisplayMode` в `UserSettings` (export/import v1 compatible)
- Core `AddupStatusFormatter` + tests; `runCommand` форматирует status по режиму
- **Анти-scope:** новые CommandIds; запись stats в shapes

# Sprint 05 — Backlog

> Architect декомпозировал Sprint 05 (2026-06-28). **S05-004 Done** (PR #39). **S05-005 In Progress** (optional stretch).

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S05-001 | Consulting profiles (McKinsey/BCG presets → UserSettings) | Core + Api + AddIn + Tests | **Done** | PR #32 |
| S05-002 | Snap-to-grid 0.1 cm (Core + layout apply) | Core + Api + AddIn + Tests | **Done** | #33 / PR #34 |
| S05-003 | Slide Backup Manager (`MoveSlidesToBackup`) | Core + AddIn + Tests | **Done** | #35 / PR #36 |
| S05-004 | Multi-slide shape paste/remove | Core + AddIn + Tests | **Done** | #37 / PR #39 |
| S05-005 | Smart Duplicate gap memory (optional stretch) | Core + AddIn + Tests | **In Progress** | #41 |

## Порядок исполнения (рекомендация architect)

1. **S05-001** — низкий risk, builds on S03 Settings; разблокирует Consulting Mode UX
2. **S05-002** — Core snap math; зависит от S05-001 только по `UserSettings` shape (можно параллельно после 001)
3. **S05-003** или **S05-004** — Partial Office.js; выбрать обе если capacity
4. **S05-005** — stretch после P2

# Sprint 05 — Consulting Mode, Backup, Multi-slide

> Контекст: Sprint 04 Done (Smart Color Picker). Фичи из README «Дополнительные инновационные функции».
> Кикофф — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта

Реализовать **продвинутые продуктовые фичи** поверх базового паритета команд (Sprint 02):
Consulting Mode (профили + snap-to-grid), Slide Backup Manager, Multi-slide операции — с **явной деградацией**
на PowerPoint Web там, где Office.js не даёт полного API.

## Office.js feasibility (зафиксировано architect, 2026-06-28)

| Тема | Desktop | PowerPoint Web | Решение |
|------|---------|----------------|---------|
| **Consulting profiles** (McKinsey/BCG → shortcuts) | Full | Full | Settings API + Core presets; **без** новых CommandIds |
| **Snap-to-grid 0.1 cm** | Full | Full | Core math на `ShapeBounds`; post-process в layout pipeline; настройка в `UserSettings` |
| **Snap-to-nearest-object** | Partial (VSTO) | **None** | **Anti-scope Sprint 05** — нет drag-hook / proximity API |
| **Slide Backup — move to end** | Partial | Partial | `slide.moveTo` (Api 1.8+) / export+delete+insert fallback; **новый** `MoveSlidesToBackup` |
| **Slide Backup — named section** | Full (COM) | **None** | Anti-scope: Office.js **нет** slide sections API |
| **Hide/show Backup section** | Full (COM) | **None** | **Anti-scope Sprint 05** |
| **Multi-slide paste shape** | Partial | Partial | `getSelectedSlides()` + `cloneShapeOnSlide` per slide; **новый** `PasteShapeToSelectedSlides` |
| **Multi-slide remove by name** | Partial | Partial | Iterate slides + delete by `shape.name`; **новый** `RemoveShapeFromSelectedSlides` |
| **Smart Duplicate gap memory** | Partial | Partial | Wire `gap` в HostScript + task-pane state; **без** нового CommandId |

## Декомпозиция `S05-0YY` (приоритет)

| Приоритет | ID | Тема | CommandCatalog | Feasibility |
|-----------|-----|------|----------------|-------------|
| **P1** | S05-001 | Consulting profiles (McKinsey/BCG presets) | UI-only / Settings | Full |
| **P1** | S05-002 | Snap-to-grid 0.1 cm | UI-only (`UserSettings`) | Full |
| **P2** | S05-003 | Slide Backup Manager (move to end) | **+** `MoveSlidesToBackup` | Partial |
| **P2** | S05-004 | Multi-slide paste / remove | **+** 2 CommandIds | Partial |
| **P3** | S05-005 | Smart Duplicate gap memory (stretch) | existing Duplicate* | Partial |

## Anti-scope (явно не в Sprint 05)

- Snap-to-nearest-object при drag/move
- Slide sections (создание/именование «Backup» section) и hide/show backup block
- Import/export settings JSON (README stretch)
- Object Statistics MIN/MAX/AVG UI
- Eyedropper / HEX (Sprint 04 deferred)
- `VstoLegacy*` — frozen

## Definition of Done спринта

- [x] **S05-001** — Consulting profiles (PR #32)
- [x] **S05-002** — Snap-to-grid 0.1 cm (P1) — PR #34
- [x] **S05-003** — Slide Backup Manager (P2 Backup) — PR #36
- [x] **S05-004** — Multi-slide paste/remove (P2) — PR #39
- [ ] (Optional) S05-005 — Smart Duplicate gap

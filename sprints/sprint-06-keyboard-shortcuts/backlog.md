# Sprint 06 — Backlog

> Architect декомпозировал Sprint 06 (2026-06-29). **P1 Done** (S06-001/002). Следующая: **S06-003**.

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S06-001 | Shared Runtime + Keyboard Shortcuts (Tier 1) | AddIn + manifest + scripts + Tests | **Done** | #44 / PR #46 |
| S06-002 | `replaceShortcuts` ↔ UserSettings (все bindings) | AddIn | **Done** | #48 / PR #49 |
| S06-003 | Import/export settings JSON | Core + Api + AddIn | **Todo** | — |
| S06-004 | Object Statistics MIN/MAX/AVG UI | AddIn | **Todo** | — |
| S06-005 | Color Picker eyedropper / HEX (stretch) | AddIn | **Todo** | — |

## Порядок исполнения (рекомендация architect)

1. **S06-001** — инфраструктура shared runtime + Tier 1 defaults; блокирует S06-002
2. **S06-002** — live sync Shortcut Manager; consulting profile apply → `replaceShortcuts`
3. **S06-003 / S06-004** — P2 stretch после P1
4. **S06-005** — P3 deferred из Sprint 04

## Черновик S06-003 (import/export settings JSON)

- **Export:** скачать `.json` текущих `UserSettings` (profile, snapToGrid, shortcuts) из Settings panel
- **Import:** выбрать файл → Core validate (CommandId in catalog, keys normalize) → editor → Save → `replaceShortcuts`
- Формат: тот же JSON что `UserSettings.Serialize` (VSTO parity); optional `schemaVersion: 1`
- Api: `POST /api/settings/import` validate-only или validate+merge; export может быть client-only blob
- Anti-scope: encrypt, cross-device sync, import catalog/commands

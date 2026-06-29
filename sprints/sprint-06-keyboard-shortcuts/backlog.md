# Sprint 06 — Backlog

> Architect декомпозировал Sprint 06 (2026-06-29). См. [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S06-001 | Shared Runtime + Keyboard Shortcuts (Tier 1) | AddIn + manifest + scripts + Tests | **Done** | #44 / PR #46 |
| S06-002 | `replaceShortcuts` ↔ UserSettings (все bindings) | AddIn | **Done** | #48 / PR #49 |
| S06-003 | Import/export settings JSON | Core + Api + AddIn | **Done** | #51 / PR #52 |
| S06-004 | Object Statistics MIN/MAX/AVG UI | Core + AddIn | **In Progress** | #54 |
| S06-005 | Color Picker eyedropper / HEX (stretch) | AddIn | **Todo** | — |

## Порядок исполнения (рекомендация architect)

1. **S06-001** — инфраструктура shared runtime + Tier 1 defaults; блокирует S06-002
2. **S06-002** — live sync Shortcut Manager; consulting profile apply → `replaceShortcuts`
3. **S06-003 / S06-004** — P2 stretch после P1
4. **S06-005** — P3 deferred из Sprint 04

## Черновик S06-002 (постановка после merge S06-001)

- Объявить runtime actions для **всех 79** CommandIds в manifest / shortcuts schema
- On Settings Save + on task pane load: `Office.actions.replaceShortcuts(map from UserSettings.shortcuts)`
- `Office.actions.areShortcutsInUse()` → warning в Shortcut Manager (non-blocking)
- Consulting profile apply (McKinsey/BCG) → `replaceShortcuts` после Save
- 9 `support=None` команд — handler вызывает existing unsupported path через `runCommand`

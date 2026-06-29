# Sprint 06 — Global Keyboard Shortcuts (Windows Desktop)

> Контекст: Sprint 05 Done (79 команд, consulting profiles, backup, multi-slide).
> Кикофф — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).
> Runbook: [`docs/migration/03-powerpoint-desktop-windows.md`](../../docs/migration/03-powerpoint-desktop-windows.md).

## Цель спринта

Включить **глобальные шорткаты** (Alt+1, Alt+D, …) на **PowerPoint Desktop Windows 2601+** через официальный
Office **Keyboard Shortcuts API** + **Shared Runtime**, синхронизировав их с Shortcut Manager (`UserSettings`).
На Web / Mac / старых версиях — **без регрессии**: task pane и Settings как сейчас.

## Office.js feasibility (зафиксировано architect, 2026-06-29)

| Тема | PP Win 2601+ | PowerPoint Web | Mac |
|------|--------------|----------------|-----|
| Shared Runtime (task pane + actions) | Full | Full (load) | Full (load) |
| Tier 1 hotkeys (`shortcuts.json` defaults) | Full | No-op / degraded | Limited per MS |
| `replaceShortcuts` из UserSettings | Full | No-op | Limited |
| `areShortcutsInUse` conflict hints | Full | No-op | Limited |
| Settings commands как hotkeys | Skip S06-001 | Skip | Skip |

## Декомпозиция `S06-0YY`

| Приоритет | ID | Тема | Компонент |
|-----------|-----|------|-----------|
| **P1** | S06-001 | Shared Runtime + Tier 1 keyboard shortcuts | AddIn + manifest + scripts |
| **P1** | S06-002 | `replaceShortcuts` ↔ все UserSettings bindings | AddIn |
| **P2** | S06-003 | Import/export settings JSON | Core + Api + AddIn |
| **P2** | S06-004 | Object Statistics MIN/MAX/AVG UI | AddIn (Core Addup готов) |
| **P3** | S06-005 | Color Picker eyedropper / HEX | AddIn (deferred S04) |

## Anti-scope (явно не в Sprint 06)

- VSTO legacy / COM keyboard hook
- Новые CommandIds (кроме уже существующих 79)
- Snap-to-nearest-object, slide sections
- Полный паритет VSTO ribbon tab (остаётся Home → PowerKeys button)
- Регистрация hotkeys без manifest action declaration (все 79 — только через S06-002 scope)

## Definition of Done спринта

- [x] **S06-001** — Shared Runtime + Tier 1 defaults (`shortcuts.json`, `associate` → `runCommand`) — PR #46
- [x] **S06-002** — Save/load Shortcut Manager → `Office.actions.replaceShortcuts` (76 hotkey-eligible actions) — PR #49
- [ ] **S06-003** — Import/export settings JSON (P2)
- [ ] Трассировка `S06-0YY` → Issue → PR → merge (для оставшихся задач)
- [ ] `dotnet test PptPowerKeys.sln` — зелёный
- [ ] AddIn: `typecheck`, `validate:prod`, `build:prod` — зелёные
- [x] `docs/PRODUCT_CONTEXT.md` + `03-powerpoint-desktop-windows.md` — hotkeys (S06-001/002)
- [ ] Manual QA note: PP Desktop Win 2601+ hotkeys (вне CI)

**P1 (hotkeys) — выполнен.** Следующая рекомендуемая задача: **S06-003** import/export JSON.

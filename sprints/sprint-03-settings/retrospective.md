# Sprint 03 — Retrospective

> **Статус:** завершён (2026-06-28). Все задачи Done.

## Итоги

| ID | PR | Результат |
|----|-----|-----------|
| S03-001 | #23 | `FileUserSettingsStore`, Docker volume `settings_data`, `IUserSettingsStore` |
| S03-002 | #25 | `SettingsPanel.tsx`, `userId.ts`, `X-User-Id`, Save/Reset, Settings-команды wired |
| S03-003 | #27 | `ShortcutManager.tsx`, editable bindings, duplicate-key hints, `ShortcutBindingValidator` |

## Definition of Done спринта — выполнено

- [x] Настройки сохраняются между рестартами API на VDS (file-backed JSON per user)
- [x] Settings UI открывается из категории Settings и из команд `OpenShortcutManager` / `ResetToDefaults`
- [x] Shortcut Manager: edit/add/remove bindings + conflict hints
- [x] Трассировка `S03-0YY`: task → backlog → PR → merge

## Ключевые решения

- **Office Web:** global hotkeys не перехватываются — MessageBar hint в Settings; Shortcut Manager хранит/редактирует привязки, но не регистрирует hotkeys.
- **Duplicate keys:** warning visible, Save **не блокируется** (architect decision S03-003).
- **User id:** client-generated UUID в `localStorage` до SSO (отдельная задача).
- **JSON shape:** `UserSettings` / `ShortcutBinding` совместим с legacy VSTO.

## Метрики

- `dotnet test`: 67 passed (было 61 после S03-001)
- AddIn: `typecheck`, `validate:prod` — зелёные

## Следующий спринт

**Sprint 04** — Smart Color Picker / Slide Master palette (`OpenColorScheme` stub в S03-002).

# Кикофф для архитектора — Sprint 03 (Settings) — ЗАВЕРШЁН

> Sprint 03 Done (2026-06-28). Ретроспектива — [`retrospective.md`](./retrospective.md).
> Следующий спринт: **Sprint 04** (Smart Color Picker).

## Итог Sprint 03

| ID | Статус | Содержание |
|----|--------|------------|
| S03-001 | **Done (#23)** | Persistent store + Docker volume |
| S03-002 | **Done (#25)** | Settings panel UI + wiring Settings commands |
| S03-003 | **Done (#27)** | Shortcut Manager (edit bindings, conflict hints) |

## Доставлено

- `FileUserSettingsStore` — JSON per user, atomic write, Docker volume
- `SettingsPanel` + `ShortcutManager` — profile, editable shortcuts, Save/Reset
- `getUserId()` → `X-User-Id`; Settings-команды wired
- `ShortcutBindingValidator` в Core + unit-тесты

## Ограничение Office Web

Global hotkeys не работают как VSTO — hint в Settings panel; Shortcut Manager для хранения/редактирования привязок.

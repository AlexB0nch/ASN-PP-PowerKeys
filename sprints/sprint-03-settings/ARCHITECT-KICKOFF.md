# Кикофф для архитектора — Sprint 03 (Settings)

> Входная точка для **новой сессии `/architect`**. Sprint 02 Done — `sprint-02-functionality/retrospective.md`.

## 1. Где мы сейчас (2026-06-28)
- **76 команд** wired; Settings API **персистентный** (S03-001, PR #23).
- **`FileUserSettingsStore`:** JSON per user в `SETTINGS_DATA_PATH` (default `/data/settings`), Docker volume `settings_data`.
- **AddIn (S03-002, PR #25):** Settings panel (`SettingsPanel.tsx`), `getUserId()` → `X-User-Id`, `resetSettings()`,
  Save/Reset, read-only shortcuts, Settings-команды wired.
- **Office Web caveat:** global hotkeys не работают как VSTO — hint в Settings panel.

## 2. Цель Sprint 03 (осталось)
| ID | Статус | Содержание |
|----|--------|------------|
| S03-001 | **Done (#23)** | Persistent store + Docker volume |
| S03-002 | **Done (#25)** | Settings panel UI + wiring Settings commands |
| **S03-003** | **In Progress** | Shortcut Manager (edit bindings, conflict hints) |

## 3. Следующая задача — S03-003
Shortcut Manager UI: редактирование key bindings, conflict hints. Опирается на Settings panel (S03-002)
и persistent API (S03-001). Office Web — хранение/редактирование привязок; фактическое срабатывание
глобальных hotkeys ограничено.

## 4. Ключевые файлы
- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`, `App.tsx`, `runCommand.ts`
- `src/PptPowerKeys.AddIn/src/services/api.ts`, `userId.ts`, `types.ts`
- `src/PptPowerKeys.Core/Settings/UserSettings.cs`

## 5. Инварианты
- JSON shape совместим с legacy VSTO; `VstoLegacy*` не менять.
- Web Add-in **не перехватывает global hotkeys** как VSTO — document in acceptance.

## 6. Процесс
Task-файл → backlog In Progress → `/builder` → приёмка → merge → `docs/PRODUCT_CONTEXT.md`.

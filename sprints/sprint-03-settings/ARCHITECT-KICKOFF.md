# Кикофф для архитектора — Sprint 03 (Settings)

> Входная точка для **новой сессии `/architect`**. Sprint 02 Done — `sprint-02-functionality/retrospective.md`.

## 1. Где мы сейчас (2026-06-28)
- **76 команд** wired; Settings API **персистентный** (S03-001, PR #23).
- **`FileUserSettingsStore`:** JSON per user в `SETTINGS_DATA_PATH` (default `/data/settings`), Docker volume `settings_data`.
- **Core:** `IUserSettingsStore`, `UserSettings`, `CreateDefaults()` из `CommandCatalog`.
- **AddIn:** `api.getSettings` / `saveSettings` есть; **нет** `resetSettings`, **нет** `X-User-Id`, **нет Settings UI**.
- Settings-команды: заглушка «Open the settings panel below.» без панели.

## 2. Цель Sprint 03 (осталось)
| ID | Статус | Содержание |
|----|--------|------------|
| S03-001 | **Done (#23)** | Persistent store + Docker volume |
| **S03-002** | **Todo** | Settings panel UI + wiring Settings commands |
| S03-003 | Todo | Shortcut Manager (edit bindings) — после S03-002 или объединить |

## 3. Следующая задача — S03-002
Settings panel в task pane (Fluent UI): load on mount, save, reset; wiring `OpenShortcutManager`,
`ResetToDefaults`, stub `OpenColorScheme`; client `X-User-Id` (localStorage) до SSO.

## 4. Ключевые файлы
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`, `runCommand.ts`
- `src/PptPowerKeys.AddIn/src/services/api.ts`, `types.ts`
- `src/PptPowerKeys.Api/Program.cs` — `POST /api/settings/reset`
- `src/PptPowerKeys.Core/Settings/UserSettings.cs`

## 5. Инварианты
- JSON shape совместим с legacy VSTO; `VstoLegacy*` не менять.
- Web Add-in **не перехватывает global hotkeys** как VSTO — document in acceptance.

## 6. Процесс
Task-файл → backlog In Progress → `/builder` → приёмка → merge → `docs/PRODUCT_CONTEXT.md`.

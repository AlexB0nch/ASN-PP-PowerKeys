# Кикофф для архитектора — Sprint 03 (Settings)

> Входная точка для **новой сессии `/architect`**. Sprint 02 завершён — см. `sprint-02-functionality/retrospective.md`.

## 1. Где мы сейчас
- **76 команд** wired; 9 None — `unsupportedWebCommands.ts` + warning UX.
- **Settings API** существует: `GET/PUT /api/settings`, `POST /api/settings/reset`, header `X-User-Id`.
- **`SettingsStore`** — **in-memory** (`ConcurrentDictionary`); данные теряются при рестарте контейнера.
- **AddIn:** `api.getSettings` / `saveSettings` есть; **UI настроек нет**; Settings-команды возвращают
  «Open the settings panel below.» без панели.
- **Core:** `UserSettings`, `ShortcutBinding`, `UserSettings.CreateDefaults()` из `CommandCatalog`.

## 2. Цель Sprint 03
1. **Персистентность** — file-backed store (JSON per user) + Docker volume на VDS.
2. **Settings UI** — React + Fluent UI: профиль, reset, секция шорткатов.
3. **Shortcut Manager** — таблица commandId ↔ keys, edit/save, conflict hints (best-effort).

## 3. Рекомендуемая декомпозиция (architect уточняет)
| ID | Содержание |
|----|------------|
| **S03-001** | Persistent `SettingsStore` (Core interface + Api file impl, tests, Docker volume) |
| **S03-002** | Settings panel UI + load/save/reset + wiring Settings commands |
| **S03-003** | Shortcut Manager (edit bindings, validation) — или объединить с S03-002 если scope мал |

`OpenColorScheme` — минимальный stub («Coming in Smart Color Picker sprint») или placeholder panel.

## 4. Ключевые файлы
- `src/PptPowerKeys.Api/Services/SettingsStore.cs`
- `src/PptPowerKeys.Core/Settings/UserSettings.cs`
- `src/PptPowerKeys.Api/Program.cs` — settings endpoints
- `src/PptPowerKeys.AddIn/src/services/api.ts`, `types.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`, `runCommand.ts`
- `docker-compose.yml`, `.github/workflows/deploy-vds.yml` — volume для settings data
- Legacy reference (read-only): `src/PptPowerKeys.VstoLegacy/Settings/`

## 5. Инварианты
- JSON shape `UserSettings` совместим с legacy (`Profile`, `Shortcuts[]` с `CommandId`, `Keys`).
- `VstoLegacy*` не менять.
- Api — тонкий слой; валидация shortcuts — Core если нужна.

## 6. Office Web caveat
Глобальные hotkeys в Web Add-in **не как VSTO**. Sprint 03 = **persistence + UI**; document limitation in task acceptance.

## 7. Процесс
Task-файл → backlog → `/builder` → приёмка → merge → `docs/PRODUCT_CONTEXT.md`.

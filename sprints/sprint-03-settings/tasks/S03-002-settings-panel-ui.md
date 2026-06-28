# S03-002 — Settings panel UI + wiring Settings commands

> Передача builder'у: `/builder выполни S03-002`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S03-002` |
| **Спринт** | `sprint-03-settings` |
| **Компонент** | AddIn |
| **Статус** | Done |
| **Issue** | #24 |
| **PR** | #25 |

## Цель
Дать пользователю **рабочий Settings UI** в task pane: загрузка/сохранение/сброс настроек через
персистентный API (S03-001) с идентификацией пользователя (`X-User-Id`), и wiring Settings-команд
из каталога. Сейчас Settings-команды возвращают бессмысленную заглушку «Open the settings panel below.»
без реальной панели.

## Контекст
- **S03-001 Done (PR #23):** `GET/PUT /api/settings`, `POST /api/settings/reset`, header `X-User-Id`,
  file-backed JSON per user в `SETTINGS_DATA_PATH`.
- **AddIn сейчас:** `api.getSettings()` / `saveSettings()` есть, но **без** `X-User-Id`; `resetSettings()` **нет**;
  Settings UI **нет**; `runCommand.ts` для `execution: "Settings"` — заглушка.
- Settings-команды в каталоге: `OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults` (все `OfficeJsSupport.Full`).
- DTO `UserSettings` ↔ `types.ts` уже синхронизированы (`profile`, `shortcuts[]` с `commandId`/`keys`).

## Scope

### 1. Client user id (`userId.ts` или аналог)
- Стабильный anonymous id в `localStorage` под ключом `pptpowerkeys-user-id`.
- Генерировать UUID при первом визите; переиспользовать при последующих.
- Экспортировать функцию `getUserId(): string` для использования в API-слое.
- До SSO (отдельная задача) — client-generated id; API нормализует в `FileUserSettingsStore`.

### 2. API client (`api.ts`)
- Добавить `resetSettings()` → `POST /api/settings/reset`.
- Все settings-вызовы (`getSettings`, `saveSettings`, `resetSettings`) передают header `X-User-Id`
  из `getUserId()`.
- Рефактор `request()` или локальный helper для settings headers — минимальный diff.

### 3. Settings panel (Fluent UI)
Новый компонент (например `SettingsPanel.tsx`) или collapsible секция в `App.tsx`:
- **Load on mount:** `api.getSettings()` → state.
- **Profile:** отображение `settings.profile` (read-only или editable input — на усмотрение builder,
  главное — round-trip save).
- **Shortcuts:** read-only список `commandId` + `keys` (полный Shortcut Manager с редактированием — **S03-003**).
- **Кнопки:** Save (`api.saveSettings`), Reset to defaults (`api.resetSettings` + reload state).
- **Hint:** Office Web Add-in **не перехватывает глобальные hotkeys** как VSTO — показать краткую
  подсказку в UI (Caption/MessageBar).
- Loading/error states; не блокировать основной каталог команд.

### 4. Wiring Settings-команд
Интеграция `runCommand.ts` ↔ `App.tsx` (callback/ref/event — на усмотрение builder):
| Команда | Поведение |
|---------|-----------|
| `OpenShortcutManager` | Открыть/развернуть Settings panel, scroll к секции shortcuts |
| `ResetToDefaults` | `api.resetSettings()` + reload panel state + status message |
| `OpenColorScheme` | Stub: status «Smart Color Picker — planned (Sprint 04)» |

Убрать заглушку `outcomeSuccess("Open the settings panel below.")` — panel реально появляется.

### 5. UX
- Settings panel доступен из категории Settings (accordion) и по команде OpenShortcutManager.
- После Save/Reset — понятный feedback в status bar (как у остальных команд).

## Анти-scope
- **Полный Shortcut Manager** (редактирование key bindings, conflict detection) — **S03-003**.
- SSO / Azure AD / `getAccessToken()` для `X-User-Id`.
- Изменения Core/Api persistence (S03-001 done).
- Smart Color Picker / Slide Master palette (Sprint 04+).
- `VstoLegacy*` изменения.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/services/userId.ts` (новый)
- `src/PptPowerKeys.AddIn/src/services/api.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx` (новый, или inline в App)
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`

## Критерии приёмки (Definition of Done)
1. [x] `getUserId()` сохраняет stable id в `localStorage` (`pptpowerkeys-user-id`).
2. [x] `getSettings` / `saveSettings` / `resetSettings` отправляют `X-User-Id`.
3. [x] Settings panel загружает settings on mount, отображает profile и read-only shortcuts.
4. [x] Save и Reset to defaults работают через API и обновляют UI.
5. [x] `OpenShortcutManager` открывает/скроллит к Settings panel.
6. [x] `ResetToDefaults` вызывает `resetSettings()` и перезагружает UI.
7. [x] `OpenColorScheme` показывает stub «Smart Color Picker — planned (Sprint 04)».
8. [x] UI hint про ограничение global hotkeys на Office Web.
9. [x] `dotnet test PptPowerKeys.sln` — зелёный (61 passed).
10. [x] `npm run typecheck`, `npm run validate:prod` — зелёные.
11. [x] PR #25: ветка `cursor/S03-002-settings-panel-ui-0813`, Task ID `S03-002`.

## Приёмка (architect, 2026-06-28)
- PR #25 merged в `main` (commit `89511d2`).
- CHECKLIST: scope соблюдён — AddIn only, Core/Api/VstoLegacy не тронуты.
- `settingsRequest()` с `X-User-Id`; `SettingsPanel` с profile/shortcuts/Save/Reset; Settings-команды wired.
- Локально повторены `dotnet test` (61), `npm run typecheck`, `npm run validate:prod` — зелёные.

## Зависимости
- **S03-001 Done** (PR #23) — persistent settings API.

## Примечание для builder
- DTO `UserSettings` ↔ `types.ts` — не менять без необходимости.
- Следуй Fluent UI conventions из `App.tsx`.
- Ветка: `cursor/S03-002-settings-panel-ui-0813`.
- Не трогать `VstoLegacy*`.

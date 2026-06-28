# S03-003 — Shortcut Manager (edit bindings, conflict hints)

> Передача builder'у: `/builder выполни S03-003`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S03-003` |
| **Спринт** | `sprint-03-settings` |
| **Компонент** | AddIn (+ опционально Core/Tests) |
| **Статус** | In Progress |
| **Issue** | #26 |
| **PR** | TBD |

## Цель
Заменить read-only секцию shortcuts в Settings panel на полноценный **Shortcut Manager**:
редактирование привязок клавиш, добавление/удаление bindings, подсказки о конфликтах (duplicate keys).
Сохранение через существующий `api.saveSettings()` без изменения API-контракта.

## Контекст
- **S03-001 Done (PR #23):** персистентный `FileUserSettingsStore`, `GET/PUT /api/settings`, `POST /api/settings/reset`.
- **S03-002 Done (PR #25):** `SettingsPanel.tsx` с profile, read-only shortcuts, Save/Reset, `getUserId()` → `X-User-Id`,
  Settings-команды wired (`OpenShortcutManager` → scroll к `#settings-shortcuts`).
- **Gap:** shortcuts отображаются как `commandId` + `keys` (Caption1), без редактирования.
- **Каталог команд:** `api.getCommands()` уже вызывается в `App.tsx` — `CommandDescriptor` содержит `id`, `title`, `defaultShortcut`.
- **Legacy reference (read-only):** `src/PptPowerKeys.VstoLegacy/` — `ShortcutManagerForm`, `UserSettings.Shortcuts`.
- **Office Web caveat:** global hotkeys не перехватываются — MessageBar hint сохранить/уточнить.

## Scope

### 1. Editable Shortcut Manager UI
Реализовать в `SettingsPanel.tsx` или выделенном `ShortcutManager.tsx` (на усмотрение builder):

| Действие | Поведение |
|----------|-----------|
| **Список bindings** | Для каждой строки: human-readable **title** (из каталога `api.getCommands()`, fallback — `commandId`) + editable **keys** (Fluent `Input`) |
| **Edit keys** | Изменение `keys` в локальном state `settings.shortcuts` |
| **Add binding** | Выбор `commandId` из каталога (Dropdown/Combobox) + ввод `keys`; добавить в список |
| **Remove binding** | Кнопка удаления строки |
| **Duplicate commandId** | При add — заменить существующую привязку или показать hint (зафиксируй в UI; предпочтительно replace) |

Загрузить каталог команд один раз (prop из `App.tsx` или локальный `api.getCommands()` в panel).

### 2. Conflict hints (duplicate keys)
**Решение architect:** предупреждение **не блокирует Save** — пользователь видит warning, но может сохранить
(редактирование «черновика» до финального Save; Office Web всё равно не исполняет global hotkeys).

- **Обязательно (client-side):** при duplicate `keys` (case-insensitive, trim) — видимый warning
  (MessageBar `intent="warning"` или подсветка строк) со списком конфликтующих bindings.
- **Опционально (Core):** `ShortcutBindingValidator` в `PptPowerKeys.Core/Settings/` + unit-тесты в
  `PptPowerKeys.Tests`, если логика нормализации keys нетривиальна. Минимум без Core — допустим.

### 3. Save / Reset (без изменений контракта)
- **Save:** существующий `api.saveSettings(settings)` — round-trip: после Save перезагрузка с API возвращает те же данные.
- **Reset:** `api.resetSettings()` → `UserSettings.CreateDefaults()` с сервера (уже работает в S03-002).
- После Save — feedback в status bar через `onFeedback` (`outcomeSuccess("Settings saved.")` — уже есть).

### 4. UX
- Сохранить MessageBar про ограничение Office Web (global hotkeys).
- Заголовок секции: «Shortcuts» (убрать «read-only»).
- `OpenShortcutManager` — `scrollToShortcuts()` к editable секции (обновить ref/id при рефакторе).
- Loading/error states не ломать; panel не блокирует каталог команд.

## Анти-scope
- Регистрация global hotkeys в Office Web (невозможно как VSTO).
- SSO / Azure AD для `X-User-Id`.
- Smart Color Picker (Sprint 04).
- Изменения persistence layer (`FileUserSettingsStore`, S03-001 done).
- Изменения API DTO / `types.ts` (если не требуется для UI).
- `VstoLegacy*` изменения.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx` (основной)
- `src/PptPowerKeys.AddIn/src/taskpane/ShortcutManager.tsx` (опционально, новый)
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx` (передача `commands` в panel, если нужно)
- `src/PptPowerKeys.Core/Settings/ShortcutBindingValidator.cs` (опционально)
- `src/PptPowerKeys.Tests/ShortcutBindingValidatorTests.cs` (опционально)

## Критерии приёмки (Definition of Done)
1. [ ] Пользователь может edit/add/remove shortcut bindings и **Save** → `GET /api/settings` возвращает те же `shortcuts`.
2. [ ] Duplicate `keys` → **видимое warning**, Save **не блокируется**.
3. [ ] Human-readable **title** команд из каталога (не только `commandId`).
4. [ ] MessageBar про Office Web global hotkeys сохранён.
5. [ ] `OpenShortcutManager` скроллит к editable shortcuts.
6. [ ] Reset to defaults восстанавливает bindings из `CreateDefaults()`.
7. [ ] `dotnet test PptPowerKeys.sln` — зелёный (61+ tests).
8. [ ] `npm run typecheck`, `npm run validate:prod` — зелёные.
9. [ ] PR в `main`, ветка `cursor/S03-003-shortcut-manager-*`, Task ID `S03-003`, `Closes #<issue>`.

## Зависимости
- **S03-001 Done** (PR #23) — persistent settings API.
- **S03-002 Done** (PR #25) — Settings panel shell + wiring.

## Примечание для builder
- JSON shape `ShortcutBinding` (`commandId`, `keys`) — совместим с legacy VSTO; не менять без необходимости.
- `types.ts` ↔ Api DTO — синхрон; новые поля не добавлять.
- Fluent UI conventions — как в `App.tsx` / `SettingsPanel.tsx`.
- Ветка: `cursor/S03-003-shortcut-manager-1dae` (или по шаблону cloud agent).
- Не трогать `VstoLegacy*`.

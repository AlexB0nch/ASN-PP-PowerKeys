# S05-001 — Consulting profiles (McKinsey / BCG presets)

> Передача builder'у: `/builder выполни S05-001`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S05-001` |
| **Спринт** | `sprint-05-advanced-features` |
| **Компонент** | Core + Api + AddIn + Tests |
| **Статус** | Done |
| **PR** | #32 |

## Цель

Дать пользователям **предустановленные профили шорткатов McKinsey и BCG** (README / Consulting Mode),
переиспользуя инфраструктуру Settings из Sprint 03. Профиль хранится в `UserSettings.profile`;
при выборе McKinsey/BCG подставляется соответствующий набор `shortcuts` из **Core presets** (тестируемо без PowerPoint).

## Контекст

- S03 Done: `UserSettings`, `FileUserSettingsStore`, `SettingsPanel`, `ShortcutManager`, `GET/PUT /api/settings`.
- Сейчас `profile` — свободный текст (`Input`); presets **не применяются**.
- VSTO legacy **не содержит** готовых McKinsey/BCG JSON — presets определяем в Core по README parity.
- **Новые CommandIds не нужны** — чисто Settings/UI.

## Scope

### 1. Core — `ConsultingProfilePresets` (новый)

Файл: `src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs`

- Константы имён: `McKinsey`, `BCG`, `Custom` (case-sensitive, как в VSTO tests).
- `IReadOnlyList<string> KnownProfiles` — упорядоченный список для UI.
- `bool IsKnownProfile(string profile)` — true для McKinsey/BCG/Custom.
- `IReadOnlyList<ShortcutBinding> GetShortcuts(string profile)`:
  - **Custom** → пустой список **или** throw; UI не вызывает apply для Custom.
  - **McKinsey** и **BCG** → фиксированные bindings (только `CommandId` из `CommandIds` enum / `CommandCatalog`).
- Все `CommandId` в preset должны существовать в `CommandCatalog`; keys нормализуются через `ShortcutBindingValidator.NormalizeKeys`.
- **McKinsey preset** (consulting-heavy, VSTO/README parity):
  - Alignment: `AlignLeft` Alt+1 … `DistributeVertical` Alt+8 (как catalog defaults)
  - Resize: `SameWidth` Alt+B, `SameHeight` Alt+H
  - Format: `FillColor` Alt+G, `OpenColorScheme` Alt+L
  - Objects: `DuplicateRight` Alt+D
  - Text: `AddupTextFields` Alt+A
- **BCG preset** (отличимый набор, без duplicate keys внутри профиля):
  - Alignment: Alt+1…Alt+8 (те же команды)
  - Resize: `SameWidth` Ctrl+Alt+B, `SameHeight` Ctrl+Alt+H
  - Format: `FillColor` Alt+G, `LineColor` Alt+L
  - Objects: `DuplicateDown` Alt+D, `DuplicateRight` Alt+Shift+D
  - Text: `AddupTextFields` Ctrl+Alt+A
- Юнит-тесты `ConsultingProfilePresetsTests.cs`:
  - оба preset возвращают ≥5 bindings, все CommandId валидны в catalog
  - нет duplicate keys внутри каждого preset
  - McKinsey ≠ BCG (хотя бы одна различающаяся binding)
  - `GetShortcuts("Unknown")` → ArgumentException или empty (зафиксируй в тесте)

### 2. Api — endpoint presets

- `GET /api/settings/profile-presets` → `{ profiles: string[], presets: { [name]: { profile, shortcuts } } }`  
  или leaner: `{ profiles: [{ name, shortcuts }] }` — на усмотрение builder, синхронизировать с `types.ts`.
- Тонкий слой: вызывает `ConsultingProfilePresets`, без дублирования логики.
- 1–2 integration test в `ApiIntegrationTests.cs`.

### 3. AddIn — Profile dropdown + Apply

`SettingsPanel.tsx`:

- Заменить free-text `Input` на **Dropdown** (`McKinsey`, `BCG`, `Custom`).
- При смене на **McKinsey** или **BCG**:
  - загрузить shortcuts из preset (локальный state `settings.shortcuts` заменяется);
  - показать `MessageBar intent="warning"`: «Applying preset replaces current shortcuts. Click Save to persist.»
- При смене на **Custom** — **не** затирать shortcuts (только меняется `profile` label).
- Опционально: кнопка **Apply preset** если dropdown уже на McKinsey/BCG — перезагрузить preset (на усмотрение; минимум — смена dropdown достаточна).
- `api.getProfilePresets()` в `api.ts` + типы в `types.ts`.
- Сохранение — существующий `api.saveSettings()` без изменения контракта `UserSettings`.

### 4. Reset behavior

- `Reset to defaults` → `UserSettings.CreateDefaults()` + `profile: "Custom"` (уже на сервере; убедиться profile сбрасывается).

## Анти-scope

- Snap-to-grid (S05-002)
- Import/export JSON settings
- Global hotkey capture на Web (hint MessageBar сохранить)
- Новые CommandIds / CommandCatalog entries
- `VstoLegacy*`

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs` (новый)
- `src/PptPowerKeys.Api/Program.cs` — endpoint
- `src/PptPowerKeys.Api/Contracts/` — DTO при необходимости
- `src/PptPowerKeys.Tests/ConsultingProfilePresetsTests.cs` (новый)
- `src/PptPowerKeys.Tests/ApiIntegrationTests.cs`
- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`
- `src/PptPowerKeys.AddIn/src/services/api.ts`, `types.ts`

## Критерии приёмки (Definition of Done)

1. [x] Core presets McKinsey/BCG с валидными CommandId; 10 unit-тестов.
2. [x] Api `GET /api/settings/profile-presets` зелёный в integration tests.
3. [x] Settings UI: dropdown McKinsey/BCG/Custom; выбор McKinsey/BCG подставляет shortcuts; Custom не затирает.
4. [x] Save/Reset round-trip через API сохраняет `profile` + `shortcuts`.
5. [x] `dotnet test PptPowerKeys.sln` — 93 passed.
6. [x] `npm run typecheck` и `npm run validate:prod` — зелёные.
7. [x] PR #32 merged в `main`.

## Приёмка (architect, 2026-06-28)
- PR #32 merged. Scope соблюдён: Core presets + Api endpoint + Settings dropdown; CommandCatalog не тронут.
- CHECKLIST: explicit degradation hint сохранён; VstoLegacy не тронут.

## Зависимости

- S03-001…003 Done — в main. Блокеров нет.

## Примечание для builder

- Ветка: `cursor/S05-001-consulting-profiles-31c7`
- Не менять `CommandCatalog` / `CommandIds`.
- Preset keys — reference для Office Desktop; Web hint про hotkeys сохранить.

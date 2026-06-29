# S06-003 — Import/export settings JSON

> Передача builder'у: `/builder выполни S06-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-003` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | Core + Api + AddIn |
| **Статус** | In Progress |
| **Issue** | #51 |
| **PR** | — |

## Цель

В Settings panel: **Export** скачивает `.json` с настройками; **Import** загружает файл, валидирует,
подставляет в editor → пользователь **Save** → API + live hotkeys (Desktop 2601+).
Формат совместим с `UserSettings.Serialize` (VSTO on-disk shape).

## Контекст (после S06-002)

| Компонент | Состояние |
|-----------|-----------|
| `UserSettings` | `{ profile, snapToGrid, shortcuts: [{ commandId, keys }] }` |
| Persistence | `PUT /api/settings` + `X-User-Id` |
| Hotkeys после Save | `syncKeyboardShortcuts()` → `replaceShortcuts` |
| Settings UI | Save / Reset / profile presets (McKinsey/BCG warning + Save) |
| **Gap** | Нет UI export/import файла; нельзя перенести профиль между машинами |

## Решения architect (зафиксировано)

### JSON format (export file)

```json
{
  "schemaVersion": 1,
  "profile": "McKinsey",
  "snapToGrid": true,
  "shortcuts": [
    { "commandId": "AlignLeft", "keys": "Alt+1" }
  ]
}
```

- `schemaVersion`: **1** — optional при import (если нет — treat as v1 legacy без version).
- Property names: **camelCase** как API/`types.ts` (`profile`, `snapToGrid`, `shortcuts`, `commandId`, `keys`).
- **Не включать:** `userId`, recent colors, catalog.

### Export source

Текущий **editor state** в Settings panel (включая unsaved edits).
Filename: `ppt-powerkeys-settings.json` или `ppt-powerkeys-settings-{profile}.json`.

### Import flow

1. User picks `.json` file (`<input type="file" accept=".json,application/json">`).
2. Parse + validate (Core).
3. On success → replace settings state in panel; MessageBar warning: «Imported — click Save to persist.»
4. On Save → existing `api.saveSettings` + `syncKeyboardShortcuts` + `onSettingsUpdated`.
5. On invalid → `outcomeError` / MessageBar с понятной причиной.

**Post-import hotkeys:** только после Save (не live на import preview).
`onSettingsUpdated` **не** вызывать до Save (как preset McKinsey pattern).

### Validation (Core — testable без PowerPoint)

Новый `UserSettingsImporter.cs` (или static methods on `UserSettings`):

| Проверка | Поведение |
|----------|-----------|
| Invalid JSON | `SettingsImportResult.Failed("Invalid JSON")` |
| Null / missing shortcuts | Empty list OK |
| Unknown `commandId` | **Skip** binding + collect warning (не fail entire import) |
| Empty `commandId` | Skip |
| Keys whitespace-only | Skip binding |
| Valid `CommandId` | Must exist in `CommandCatalog` |
| Duplicate keys in file | **Keep last**; align with Shortcut Manager |
| Profile string | Accept any string; normalize unknown → `"Custom"` optional |
| `snapToGrid` | `bool`, default `false` if missing |

Return type:

```csharp
public sealed record SettingsImportResult(
    UserSettings? Settings,
    IReadOnlyList<string> Warnings,
    string? Error = null);
```

- `Settings != null` + `Error == null` → success (possibly with warnings).
- `Settings == null` + `Error != null` → failed import.

### Api (thin layer)

- `POST /api/settings/import` — body: raw JSON string or `UserSettings`-shaped object → calls Core importer →
  returns `{ settings, warnings[] }` **without saving**.
- Export: **client-only** blob download достаточен; `GET /api/settings/export` — optional (не обязателен).

### AddIn UI (`SettingsPanel.tsx`)

- Кнопки **Export JSON** / **Import JSON** рядом с Save/Reset.
- Export: `JSON.stringify(settings, null, 2)` → Blob → download.
- Import: file reader → `api.importSettings(jsonText)` (POST validate).
- После import success: `setSettings(imported)`; show warnings if any.
- Hidden file input + ref trigger on Import button click.

## Scope builder

| Компонент | Файлы |
|-----------|-------|
| Core | `UserSettingsImporter.cs`, `UserSettingsImporterTests.cs` |
| Api | `Program.cs` — `POST /api/settings/import`; DTO in `SettingsContracts.cs` (or inline) |
| AddIn | `SettingsPanel.tsx`, `api.ts`, `types.ts` if needed |
| Docs | README roadmap ✓; `PRODUCT_CONTEXT.md` journal S06-003 (architect post-merge) |

## Анти-scope

- Import `CommandCatalog` / commands list
- Encrypt/sign settings file
- Auto-save on import without user Save
- Import recent colors (`localStorage`)
- New CommandIds
- `VstoLegacy*`
- Settings commands as hotkey targets (unchanged)

## Затрагиваемые файлы (ожидаемо)

| Область | Файлы |
|---------|-------|
| Core | `src/PptPowerKeys.Core/Settings/UserSettingsImporter.cs` |
| Tests | `src/PptPowerKeys.Tests/UserSettingsImporterTests.cs`, integration test for import endpoint |
| Api | `src/PptPowerKeys.Api/Program.cs`, `Contracts/SettingsContracts.cs` |
| AddIn | `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`, `src/services/api.ts`, `types.ts` |
| Docs | `README.md` (import/export line), `docs/PRODUCT_CONTEXT.md` (post-merge) |

## Критерии приёмки (Definition of Done)

### Core
- [ ] `UserSettingsImporter` + unit tests (valid, invalid JSON, unknown commandId, snapToGrid roundtrip, duplicate keys last wins).

### Api
- [ ] `POST /api/settings/import` — validate-only, integration test.

### AddIn
- [ ] Settings UI: Export downloads `.json`; Import loads + validates + editor update.
- [ ] Import → Save → settings persist via `GET /api/settings`; hotkeys sync on Save (Desktop).
- [ ] Import errors — понятные сообщения; no crash task pane.

### CI
- [ ] `dotnet test PptPowerKeys.sln` — зелёный.
- [ ] `npm run typecheck`, `validate:prod`, `build:prod` — зелёные.

### PR
- [ ] Ветка: `cursor/S06-003-import-export-settings-json-<suffix>`.
- [ ] `Closes #<issue>`; CHECKLIST; docs post-merge.

## Красные флаги (reject)

- Import auto-saves без Save
- Unknown `commandIds` silently accepted без warning
- Export wrong schema (missing fields / wrong casing)
- Regression hotkeys sync
- Api saves on import endpoint without explicit design
- `onSettingsUpdated` / `syncKeyboardShortcuts` called on import preview (до Save)

## Зависимости

- S03 Settings API — в main ✓
- S06-002 `syncKeyboardShortcuts` — в main ✓ (PR #49)

## Примечание для builder

- Export JSON should include `schemaVersion: 1` for forward compatibility.
- Profile slug in filename: sanitize (e.g. replace spaces/special chars).
- Reuse `ShortcutBindingValidator` semantics for duplicate keys (last wins).
- McKinsey/BCG import warning pattern: reuse `presetWarning`-style MessageBar for import.

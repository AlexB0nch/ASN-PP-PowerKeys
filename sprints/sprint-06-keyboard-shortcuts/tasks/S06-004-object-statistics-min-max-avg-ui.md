# S06-004 — Object Statistics MIN/MAX/AVG UI

> Передача builder'у: `/builder выполни S06-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-004` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | Core + Api + AddIn |
| **Статус** | Done |
| **Issue** | #54 (closed) |
| **PR** | #55 |

## Цель

UI выбора режима отображения Addup (`all` | `sum` | `min` | `max` | `average`) + persist в
`UserSettings.addupDisplayMode`. Команда `AddupTextFields` (и hotkey Alt+A) форматирует status bar по режиму;
default `all` = текущее поведение без регрессии.

## Контекст (после S06-003)

| Компонент | Состояние |
|-----------|-----------|
| `NumberAggregator` | `Stats(Count, Sum, Min, Max, Average)` ✓ |
| API | `POST /api/text/addup` ✓ |
| AddIn | `AddupTextFields` → все метрики одной строкой ✓ |
| Settings | snapToGrid persist; export/import JSON v1 ✓ |
| **Gap** | Нет выбора MIN/MAX/AVG-only display; нет persist режима |

## Решения architect (зафиксировано)

См. [`ARCHITECT-KICKOFF-S06-004.md`](../ARCHITECT-KICKOFF-S06-004.md).

### Status strings (exact)

| Mode | Template |
|------|----------|
| `all` | `Sum {sum} · avg {average} · min {min} · max {max} ({count} numbers).` |
| `sum` | `Sum {sum} ({count} numbers).` |
| `min` | `Min {min} ({count} numbers).` |
| `max` | `Max {max} ({count} numbers).` |
| `average` | `Avg {average} ({count} numbers).` |
| any, count=0 | `No numbers found in selection.` |

### UserSettings field

```json
{
  "profile": "Custom",
  "snapToGrid": false,
  "addupDisplayMode": "all",
  "shortcuts": [...]
}
```

- Property: `addupDisplayMode` (camelCase).
- Default: `"all"` if missing (GET, reset, import).
- Valid values: `all`, `sum`, `min`, `max`, `average` (case-insensitive on import → normalize lowercase).

### Import validation

| Проверка | Поведение |
|----------|-----------|
| Missing `addupDisplayMode` | `"all"` |
| Invalid value | `"all"` + warning `Unknown addupDisplayMode — using 'all'.` |

## Scope builder

| Компонент | Файлы |
|-----------|-------|
| Core | `AddupStatusFormatter.cs`, `UserSettings.cs`, `UserSettingsImporter.cs` |
| Tests | `AddupStatusFormatterTests.cs`, extend `UserSettingsImporterTests.cs`, settings roundtrip |
| Api | Contracts if needed (UserSettings DTO already maps Core) |
| AddIn | `types.ts`, `SettingsPanel.tsx`, `runCommand.ts`, `commandContext.ts`, `App.tsx`, optional `addupStatus.ts` |
| Docs | post-merge: `PRODUCT_CONTEXT.md`, README, `goals.md` (architect) |

## Анти-scope

- Новые CommandIds
- Запись stats обратно в фигуры / auto clipboard
- Изменение `NumberAggregator` math
- Eyedropper (S06-005)
- `VstoLegacy*`

## Затрагиваемые файлы (ожидаемо)

| Область | Файлы |
|---------|-------|
| Core | `Text/AddupStatusFormatter.cs`, `Settings/UserSettings.cs`, `Settings/UserSettingsImporter.cs` |
| Tests | `AddupStatusFormatterTests.cs`, `UserSettingsImporterTests.cs`, `ApiIntegrationTests.cs` |
| AddIn | `SettingsPanel.tsx`, `runCommand.ts`, `App.tsx`, `runtime/commandContext.ts`, `services/types.ts` |

## Приёмка (architect, 2026-06-29)

- PR #55 merged. Scope соблюдён: `AddupStatusFormatter`, `addupDisplayMode` persist, Settings dropdown, export/import, hotkey path.
- Режим `all` без регрессии status строки; invalid import → `all` + warning.
- `dotnet test` 143 passed; AddIn typecheck/validate:prod/build:prod green.

## Критерии приёмки (Definition of Done)

### Core
- [x] `AddupStatusFormatter.Format` + unit tests (all modes, count=0, `all` regression string).
- [x] `UserSettings.AddupDisplayMode` default `"all"`; round-trip JSON.
- [x] `UserSettingsImporter` — valid modes, invalid → `all` + warning.

### AddIn
- [x] Settings dropdown «Object statistics display»; Save → reload сохраняет режим.
- [x] `AddupTextFields` status соответствует режиму; `all` = текущая строка.
- [x] Export/import включает `addupDisplayMode`.
- [x] Hotkey path (`executeCommandById`) использует тот же режим.
- [x] (Optional) «Last addup result» в Text section — session only.

### CI
- [x] `dotnet test PptPowerKeys.sln` — зелёный (143).
- [x] `npm run typecheck`, `validate:prod`, `build:prod` — зелёные.

### PR
- [x] Ветка: `cursor/S06-004-object-statistics-min-max-avg-ui-28cd`.
- [x] `Closes #54`; CHECKLIST; docs post-merge.

## Красные флаги (reject)

- Регрессия status строки в режиме `all`
- `addupDisplayMode` не в export/import
- Invalid import silently accepted без warning
- Новые CommandIds
- Изменения `NumberAggregator` math
- Formatter только в AddIn без Core tests

## Зависимости

- S06-003 import/export — main ✓
- Addup API + `NumberAggregator` — main ✓

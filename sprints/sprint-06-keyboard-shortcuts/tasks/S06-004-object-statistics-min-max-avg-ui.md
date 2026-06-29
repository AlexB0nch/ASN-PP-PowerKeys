# S06-004 — Object Statistics MIN/MAX/AVG UI

> Передача builder'у: `/builder выполни S06-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-004` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | Core (formatter) + Api (UserSettings field) + AddIn (UI + status message) |
| **Статус** | Todo |
| **Issue** | — (architect создаёт) |
| **PR** | — |

## Цель

Команда **AddupTextFields** (`Alt+A` / McKinsey preset) уже считает **Sum, Min, Max, Average** в Core и показывает
**все** метрики одной строкой в status bar. Задача — дать пользователю **UI выбора режима отображения** и
**сохранять предпочтение** (как `snapToGrid`), чтобы status bar подчёркивал нужную метрику (SUM-only, MIN-only и т.д.).

**Не** добавлять новую математику и **не** писать статистику обратно в фигуры.

## Контекст (после S06-003)

| Компонент | Состояние |
|-----------|-----------|
| `NumberAggregator.Compute` | `Stats(Count, Sum, Min, Max, Average)` — готово |
| `POST /api/text/addup` | Возвращает все поля — готово |
| `runCommand` → `AddupTextFields` | Всегда формат `Sum X · avg Y · min Z · max W (N numbers)` |
| `UserSettings` | `{ profile, snapToGrid, shortcuts[] }` — export/import v1 |
| **Gap** | Нет UI выбора режима; нельзя «показать только MIN» в status bar |

## Решения architect (рекомендация — builder следует, architect уточняет при постановке Issue)

### Режим отображения (`addupDisplayMode`)

Enum / string union:

| Значение | Status bar (пример) |
|----------|---------------------|
| `all` (default) | `Sum 15 · avg 5 · min 2 · max 8 (3 numbers)` — текущее поведение |
| `sum` | `Sum: 15 (3 numbers)` |
| `min` | `Min: 2 (3 numbers)` |
| `max` | `Max: 8 (3 numbers)` |
| `average` | `Average: 5 (3 numbers)` |

При `count === 0` — единое сообщение «No numbers found in selection.» (все режимы).

### Persistence — **UserSettings** (не localStorage)

- Новое поле `AddupDisplayMode` в Core `UserSettings` (JSON: `addupDisplayMode`, camelCase).
- Default: `"all"`.
- `GET/PUT /api/settings` — round-trip поля.
- Export/import JSON (S06-003): включить `addupDisplayMode` в файл; `UserSettingsImporter` — unknown value → warning + fallback `all`.
- **schemaVersion** export: оставить **1** (новое optional поле backward-compatible).

### Core formatter (testable)

Новый статический helper, напр. `AddupStatusFormatter.Format(Stats stats, AddupDisplayMode mode) → string`.
Юнит-тесты в `PptPowerKeys.Tests` (без PowerPoint).

### AddIn UI

1. **Settings panel** — dropdown «Object statistics display» / «Addup display» с 5 опциями; сохраняется с **Save** (как snap-to-grid).
2. **Text category** (optional stretch в той же задаче, если просто) — read-only **Last addup result** под status bar после выполнения команды (session state в React; не persist).
3. `runCommand` / `commandContext` — читать `addupDisplayMode` из cached `UserSettings`; форматировать через shared TS helper **или** дублировать логику formatter (предпочтительно один источник: Core formatter + API не нужен для format; TS mirror допустим если architect выберет DRY через маленький shared module в AddIn only).

**Рекомендация architect:** Core formatter + thin TS mirror `formatAddupStatus(stats, mode)` с комментарием sync — проще CI, чем новый API endpoint.

### Empty / edge cases

- Одно число: min = max = average = sum.
- Отрицательные и locale numbers — уже в `NumberAggregatorTests`; formatter только форматирует готовые doubles (округление: 2 decimal places или `G` как сейчас — architect зафиксирует).

## Scope builder

| Компонент | Файлы (ожидаемо) |
|-----------|------------------|
| Core | `UserSettings.cs`, `AddupDisplayMode.cs`, `AddupStatusFormatter.cs`, tests |
| Api | `SettingsContracts.cs` / DTO sync if separate; `Program.cs` mapping |
| AddIn | `types.ts`, `SettingsPanel.tsx`, `runCommand.ts`, optional `App.tsx` last-result, `commandContext` |
| Import | `UserSettingsImporter.cs` + tests для нового поля |
| Docs | architect post-merge: `PRODUCT_CONTEXT.md`, README roadmap checkbox |

## Анти-scope

- Новые CommandIds (MIN/MAX/AVG как отдельные команды)
- Запись результата в текст фигур / clipboard auto-copy
- Новый API endpoint только для format (избыточно)
- Eyedropper / Color Picker (S06-005)
- `VstoLegacy*`
- Изменение `NumberAggregator` math (уже покрыто тестами)

## Критерии приёмки

- [ ] Dropdown в Settings; значение persist через Save → reload task pane сохраняет режим.
- [ ] `AddupTextFields` status message соответствует выбранному режиму; `all` = без регрессии текущей строки.
- [ ] Export JSON содержит `addupDisplayMode`; import с валидным/invalid значением — по spec importer.
- [ ] `dotnet test PptPowerKeys.sln` — зелёный (formatter + importer tests).
- [ ] AddIn: `npm run typecheck`, `npm run validate:prod`, `npm run build:prod` — зелёные.
- [ ] `.github/review/CHECKLIST.md` — architect post-merge.
- [ ] Manual note (вне CI): Addup на выделенных text shapes в PP Desktop/Web.

## Зависимости

- S06-003 Done (export/import settings) — PR #52
- S06-001/002 Done (hotkeys для AddupTextFields) — PR #46, #49

## Трассировка

- Issue `#N` → backlog In Progress → ветка `cursor/S06-004-object-statistics-min-max-avg-ui-*` → PR `Closes #N`

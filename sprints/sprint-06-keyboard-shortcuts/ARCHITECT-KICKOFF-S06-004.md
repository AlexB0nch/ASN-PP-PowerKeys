# Architect Kickoff — S06-004 Object Statistics MIN/MAX/AVG UI

> Дата: 2026-06-29. Зависимости: S06-003 (import/export) в main ✓.

## Контекст

- `NumberAggregator.Compute` → `Stats(Count, Sum, Min, Max, Average)` — готово в Core.
- `POST /api/text/addup` — готово.
- `AddupTextFields` (HostScript) вызывает API и показывает **все** метрики одной строкой в status bar.
- Hotkey Alt+A (McKinsey) через S06-001/002 — без изменений.

Текущая status-строка (режим `all`, **не менять**):

```
Sum {sum} · avg {average} · min {min} · max {max} ({count} numbers).
```

## Цель

UI выбора режима отображения Addup + persist в `UserSettings.addupDisplayMode`.

## Решения architect

### 1. Режимы (`addupDisplayMode`)

| Значение | Status bar (пример) |
|----------|---------------------|
| `all` (default) | `Sum 10 · avg 5 · min 2 · max 8 (4 numbers).` — **точно как сейчас** |
| `sum` | `Sum 10 (4 numbers).` |
| `min` | `Min 2 (4 numbers).` |
| `max` | `Max 8 (4 numbers).` |
| `average` | `Avg 5 (4 numbers).` |

При `count === 0`: `No numbers found in selection.` (все режимы).

Числа форматировать как сейчас (JS `stats.sum` etc. — без нового rounding в Core).

### 2. Core — `AddupStatusFormatter`

- Новый файл `src/PptPowerKeys.Core/Text/AddupStatusFormatter.cs`.
- `public enum AddupDisplayMode { All, Sum, Min, Max, Average }` (или string constants — сериализация **camelCase**: `all`, `sum`, `min`, `max`, `average`).
- `public static string Format(NumberAggregator.Stats stats, AddupDisplayMode mode)`.
- Юнит-тесты в `PptPowerKeys.Tests/AddupStatusFormatterTests.cs` — все режимы + count=0 + регрессия строки `all`.

**AddIn** может дублировать форматирование в TS **или** вызывать formatter через новый тонкий API endpoint. **Решение:** formatter в Core + **клиентский mirror** в `AddIn/src/text/addupStatus.ts` с теми же строками (избегаем лишнего HTTP round-trip на каждый Addup). Тесты — только в Core.

### 3. `UserSettings.addupDisplayMode`

- Core `UserSettings.AddupDisplayMode` — `string`, default `"all"`.
- Round-trip GET/PUT/reset (как `snapToGrid`).
- `UserSettingsImporter`: unknown/invalid value → fallback `"all"` + warning `"Unknown addupDisplayMode — using 'all'."`.
- Export JSON v1: включить `addupDisplayMode` рядом с `snapToGrid`.
- `types.ts` + Api contracts синхронизировать.

### 4. Settings UI

- Dropdown **«Object statistics display»** под snap-to-grid checkbox.
- Options: All metrics / Sum only / Min only / Max only / Average only.
- Save → persist; reload panel → значение сохранено.
- Profile preset apply (McKinsey/BCG) — **не трогать** `addupDisplayMode` (как snapToGrid).

### 5. `runCommand` / hotkeys

- Расширить `LayoutOptions` (или общий run-options объект): `addupDisplayMode?: string`.
- `App.tsx` + `commandContext.getLayoutOptions()` — проброс из `userSettings`.
- `AddupTextFields` case: форматировать status по режиму (TS mirror formatter).
- Hotkey Alt+A использует тот же путь через `executeCommandById` → `getLayoutOptions()`.

### 6. Optional — «Last addup result»

- В секции **Text** accordion: Caption1 с последним результатом (session state в `App.tsx`, **не persist**).
- Обновлять после успешного `AddupTextFields`.
- Если просто по scope — делать; иначе пропустить без блокировки приёмки.

## Анти-scope

- Новые CommandIds
- Запись stats в фигуры / clipboard
- Изменение `NumberAggregator` math
- Eyedropper (S06-005)
- `VstoLegacy*`

## Приёмка (кратко)

- Dropdown + Save/reload
- `all` без регрессии status строки
- Export/import `addupDisplayMode`; invalid → `all` + warning
- Formatter tests в `PptPowerKeys.Tests`
- CI green

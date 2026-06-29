# S05-005 — Smart Duplicate gap memory

> Передача builder'у: `/builder выполни S05-005`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S05-005` |
| **Спринт** | `sprint-05-advanced-features` |
| **Компонент** | Core + AddIn + Tests |
| **Статус** | Done |
| **Issue** | #41 |
| **PR** | #42 |

## Цель

Реализовать **Figma-like gap memory** для команд `DuplicateRight` / `DuplicateLeft` / `DuplicateDown` / `DuplicateUp` (README «Smart Duplicate»):

- Первый duplicate в направлении → `gap = 0` (touching, как сейчас).
- Повторный duplicate **того же** `CommandId` → использовать запомненный gap.
- После успешного duplicate → обновить память для этого `CommandId` значением gap, фактически использованным в запросе.

Api и Core уже поддерживают `gap` (`POST /api/objects/duplicate-offset` → `DuplicationEngine.ComputeDuplicate`); AddIn сегодня всегда передаёт `0`.

## Решения architect — семантика gap memory

| Аспект | Решение |
|--------|---------|
| Scope памяти | **Per CommandId** — `DuplicateRight`, `DuplicateLeft`, `DuplicateDown`, `DuplicateUp` независимы |
| Первый duplicate в направлении | `gap = 0` (нет записи в памяти) |
| Повторный duplicate | `getDuplicateGap(commandId)` → передать в `api.duplicateOffset(...)` |
| После success | `setDuplicateGap(commandId, gap)` — gap из запроса (минимум; уточнение через `InferGap` — optional stretch) |
| Persistence | **In-memory**, scope task pane (как `positionClipboard.ts`); **не** UserSettings / **не** localStorage |
| Смена направления | Своё gap для нового направления (`0` если первый раз) |
| CommandIds | **Новых нет** — те же 4 `Duplicate*` (каталог остаётся **79** команд) |

### UX (status bar, опционально)

При `gap > 0`: «Duplicated N shape(s) (gap X pt).» — на усмотрение builder; минимум — существующее «Duplicated N shape(s).»

## Scope

### 1. Core — `DuplicationEngine.InferGap` (новый, тестируемый)

Файл: `src/PptPowerKeys.Core/Layout/DuplicationEngine.cs`

```csharp
public static double? InferGap(CommandIds command, ShapeBounds source, ShapeBounds target)
```

Обратная операция к `ComputeDuplicate`: из фактических bounds source + clone вычислить gap.

| Command | Формула |
|---------|---------|
| `DuplicateRight` | `target.Left - (source.Left + source.Width)` |
| `DuplicateLeft` | `(source.Left - target.Left) - source.Width` |
| `DuplicateDown` | `target.Top - (source.Top + source.Height)` |
| `DuplicateUp` | `(source.Top - target.Top) - source.Height` |

Non-duplicate command → `null`.

Юнит-тесты в `DuplicationEngineTests.cs` (2–4 кейса, включая `gap=5` round-trip с `ComputeDuplicate`).

### 2. AddIn — `duplicateGapMemory.ts` (новый)

По образцу `positionClipboard.ts`:

- `getDuplicateGap(commandId: string): number` — `0` если нет памяти
- `setDuplicateGap(commandId: string, gap: number): void`
- `clearDuplicateGapMemory(): void` — для тестов (export если нужен)

In-memory `Map<string, number>`; `gap ≥ 0` (отрицательный gap — clamp to `0`; зафиксировать в тесте).

### 3. AddIn — `runCommand.ts` (`Duplicate*` cases)

Для `DuplicateRight` | `DuplicateLeft` | `DuplicateDown` | `DuplicateUp`:

```ts
const gap = getDuplicateGap(descriptor.id);
// ...
api.duplicateOffset(descriptor.id, source, gap)  // вместо 0
// после успешного duplicateShapesAtPositions:
setDuplicateGap(descriptor.id, gap);
```

**Optional stretch:** после clone прочитать bounds клонов и уточнить gap через `InferGap` (Api helper или доверять requested gap). **Минимум** — store requested gap.

### 4. Документация

- `docs/migration/01-vsto-to-officejs-mapping.md` — note про gap memory для `Duplicate*`
- Post-merge: `docs/PRODUCT_CONTEXT.md` — журнал S05-005

## Анти-scope

- Новые `CommandIds` / `CommandCatalog` entries (каталог остаётся **79** команд)
- `localStorage` / `UserSettings` persist gap
- «Repeat last offset vector» across different directions (только per-`CommandId` gap)
- Snap-to-grid interaction changes
- `VstoLegacy*`
- UI toggle «reset gap memory» (не нужен в MVP)

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/Layout/DuplicationEngine.cs`
- `src/PptPowerKeys.Tests/DuplicationEngineTests.cs`
- `src/PptPowerKeys.AddIn/src/office/duplicateGapMemory.ts` (новый)
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `docs/migration/01-vsto-to-officejs-mapping.md`
- (post-merge) `docs/PRODUCT_CONTEXT.md`

## Критерии приёмки (Definition of Done)

1. [x] `DuplicationEngine.InferGap` + unit-тесты (round-trip с `ComputeDuplicate`).
2. [x] `duplicateGapMemory.ts` — get/set per `commandId`, in-memory.
3. [x] `Duplicate*` в `runCommand.ts` передают remembered gap; после success обновляют память.
4. [x] Первый duplicate в направлении — gap `0` (поведение как до задачи).
5. [x] Второй duplicate того же направления — тот же gap (проверка через unit `InferGap` + manual).
6. [x] `dotnet test PptPowerKeys.sln` — зелёный (114 passed).
7. [x] `npm run typecheck`, `npm run validate:prod` — зелёные.
8. [x] PR: `cursor/S05-005-smart-duplicate-gap-c495`, `Closes #41`.
9. [x] `.github/review/CHECKLIST.md` пройден; `CommandCatalog` **не** менялся (79 команд).

## Приёмка (architect, 2026-06-29)
- PR #42 merged (`73742fa`). Scope соблюдён: Core `InferGap` + AddIn gap memory + runCommand wire; CommandCatalog/VstoLegacy/Api без изменений.
- CI зелёный. Локально: 114 dotnet tests, typecheck, validate:prod — OK.
- Gap memory per CommandId, in-memory task pane scope; negative gap clamped to 0.
- CHECKLIST: scope OK; 79 команд без изменений.
- Ручная проверка PowerPoint Online — post-merge (Pages + VDS).

## Зависимости

- S02-001 (`Duplicate` HostScript + `duplicateOffset` API) — в main.
- Блокеров нет.

## Примечание для builder

- Ветка: `cursor/S05-005-smart-duplicate-gap-<suffix>`
- Gap math — **только в Core** (`DuplicationEngine`); AddIn — state + wire.
- Api контракт `DuplicateApiRequest.Gap` — **без изменений**.
- Инвариант `ShapeBounds` boundary; anchor = last selected (не затрагивается).

# S02-001 — Objects: group/ungroup, duplicate, copy/paste position

> Передача builder'у: `/builder выполni S02-001`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-001` |
| **Спринт** | `sprint-02-functionality` |
| **Комponent** | AddIn (+ существующий Api/Core для duplicate-offset) |
| **Статус** | Done |

## Цель
Закрыть первый блок функционального паритета Sprint 02: команды **Objects** и связанные
**copy/paste position**, которые сейчас падают в «not wired up yet» в `runCommand.ts`.
Пользователь PowerPoint Online должен иметь рабочие group/ungroup, smart-duplicate в четырёх
направлениях и копирование/вставку позиции якорной фигуры.

## Контекст
- `DuplicationEngine` в Core и `POST /api/objects/duplicate-offset` **уже реализованы** и покрыты тестами.
- Панель должна: (1) клонировать фигуру через Office.js, (2) запросить целевые bounds у API,
  (3) применить позицию. Математика сдвига — **только в Core**, не на клиенте.
- `CopyObjectPosition` / `PasteObjectPosition` — HostScript, состояние хранится в task pane
  (in-memory), см. `docs/migration/01-vsto-to-officejs-mapping.md`.
- Anchor = **последняя** выделенная фигура (для copy position — копируем left/top якоря).

## Scope
### HostScript — реализовать исполнение
| Command ID | Подход |
|------------|--------|
| `Group` | `shapes.addGroup(...)` для выделенных фигур (≥2) |
| `Ungroup` | `group.ungroup()` на выделенной группе |
| `DuplicateRight` / `DuplicateLeft` / `DuplicateDown` / `DuplicateUp` | Office.js clone (`copyTo` или эквивалент) + `api.duplicateOffset` + установка left/top |
| `CopyObjectPosition` | Сохранить `{left, top}` последней выделенной фигуры в состоянии панели |
| `PasteObjectPosition` | Применить сохранённые left/top ко всем выделенным (width/height не менять) |
| `InsertTextbox` | `shapes.addTextBox()` — быстрый win в категории Objects |
| `Regroup` | **Явная деградация**: понятное сообщение «Not supported on PowerPoint Web» (support=`None` в каталоге), **не** «not wired up yet» |

### AddIn — новые Office.js helpers
- `duplicateSelectedShape(command)` — clone + offset через API
- `groupSelectedShapes()` / `ungroupSelectedShape()`
- `copyObjectPosition()` / `pasteObjectPosition()` — с модулем состояния (например `positionClipboard.ts`)
- Обработка «нет выделения» / «неверное выделение» с понятными сообщениями

### Тесты
- Core-тесты для duplicate-offset уже есть; **новых Core-тестов не требуется**, если математика не меняется.
- При добавлении чистой логики на клиенте — минимум: unit-тесты только если вынесена в тестируемый модуль (опционально, не блокер).

## Анти-scope
- `CopyAndAlignLeft/Right/Top/Bottom` (duplicate + align — отдельная задача).
- `AlignLeftToRight` / `AlignRightToLeft` / `AlignTopToBottom` / `AlignBottomToTop` (alignment HostScript).
- Format / Text / Slides команды.
- Smart Duplicate с **памятью шага** (gap) — отдельная задача; в S02-001 gap=0.
- Реализация `Regroup` (только UX-деградация).
- Изменения `VstoLegacy*`.
- Изменения Core/Api, кроме синхронизации контракта если потребуется.

settings gap позже.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/office/positionClipboard.ts` (новый, если нужен)
- `src/PptPowerKeys.AddIn/src/services/api.ts` (использовать существующий `duplicateOffset`)
- При необходимости: `src/PptPowerKeys.AddIn/src/services/types.ts` (только если меняется контракт)

## Office.js feasibility (ориентир для builder)
- **Group**: `PowerPoint.ShapeCollection.addGroup(shapeIds)` — requirement set проверить; Full в каталоге.
- **Ungroup**: `Shape.ungroup()` — Partial (новее API); graceful error если недоступно.
- **Duplicate**: `Shape.copyTo()` или `duplicate()` — Partial; fallback-сообщение если API недоступен на платформе.
- **Copy/Paste position**: только setters `left`/`top` — Full.

## Критерии приёмки (Definition of Done)
1. [x] Команды `Group`, `Ungroup`, `DuplicateRight/Left/Down/Up`, `CopyObjectPosition`, `PasteObjectPosition`, `InsertTextbox` **не** попадают в default «not wired up yet».
2. [x] `Regroup` возвращает явное сообщение о неподдержке на Web (не generic «not wired up yet»).
3. [x] Duplicate использует `POST /api/objects/duplicate-offset` для расчёта позиции (не дублирует математику на клиенте).
4. [x] Copy/Paste position: copy сохраняет позицию **последней** выделенной; paste применяет left/top к выделенным без изменения размеров.
5. [x] Понятные ошибки при пустом/некорректном выделении (group ≥2, ungroup — группа, duplicate — ≥1).
6. [x] `dotnet test PptPowerKeys.sln` — зелёный (47 passed).
7. [x] `npm run typecheck`, `npm run validate:prod` — зелёные.
8. [x] PR #16: ветка `cursor/S02-001-objects-group-duplicate-position-a065`, Task ID `S02-001`.

## Приёмка (architect, 2026-06-28)
- PR #16, ветка `cursor/S02-001-objects-group-duplicate-position-a065`, коммит `2574897`.
- Локально повторены `dotnet test` (47 passed), `npm run typecheck`, `npm run validate:prod` — зелёные.
- CHECKLIST: scope соблюдён, Core/Api не тронуты, duplicate-offset используется корректно, Regroup — явная деградация.
- Ручная проверка в PowerPoint Online — post-merge (deploy Pages + VDS).

## Зависимости
- Нет блокеров; API duplicate-offset и DuplicationEngineTests уже в main.

## Примечание для builder
- Сохрани паттерн `runCommand.ts`: `ServerLayout` → API, `HostScript` → `powerpoint.ts`.
- Не ломай уже работающие Insert/ZOrder/Addup команды.
- Dev-сценарий (`npm start` + localhost) должен остаться рабочим.

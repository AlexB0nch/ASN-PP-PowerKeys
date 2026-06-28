# S02-004 — Alignment: edge align + copy-and-align

> Передача builder'у: `/builder выполни S02-004`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-004` |
| **Спринт** | `sprint-02-functionality` |
| **Комponent** | Core + AddIn |
| **Статус** | In Progress |

## Цель
Реализовать исполнение **8 alignment-команд**, которые сейчас падают в «not wired up yet» в
`runCommand.ts`. Пользователь PowerPoint Online должен иметь рабочие «примыкание к ребру anchor»
(`AlignLeftToRight` и др.) и «duplicate + align» (`CopyAndAlignLeft` и др.).

## Контекст
- Команды `ServerLayout` (`AlignLeft`, `DistributeHorizontal`, …) **уже работают** через
  `LayoutEngine` → `api.applyLayout` → `applyShapeBounds`.
- `AlignLeftToRight` / `AlignRightToLeft` / `AlignTopToBottom` / `AlignBottomToTop` — примыкание
  **к ребру** anchor (не совпадает с `AlignLeft`/`AlignRight`/`AlignTop`/`AlignBottom`).
- Anchor = **последняя** выделенная фигура; минимум **2 фигуры** в выделении (как у `AlignLeft`).
- `CopyAndAlignLeft/Right/Top/Bottom` — HostScript: клонировать выделение, затем выровнять по
  соответствующей оси (`AlignLeft` / `AlignRight` / `AlignTop` / `AlignBottom`) через `LayoutEngine`.
- Математика layout — **только в Core** (`ShapeBounds`); Office.js только читает/пишет геометрию.

## Scope

### Core — `LayoutEngine` (новые layout-команды)
Добавить в `LayoutEngine.Apply` и `IsLayoutCommand`:

| Command ID | Формула (для каждой non-anchor фигуры `s`, anchor `a`) |
|------------|--------------------------------------------------------|
| `AlignLeftToRight` | `s.Left = a.Right` (левый край `s` у правого края anchor) |
| `AlignRightToLeft` | `s.Left = a.Left - s.Width` |
| `AlignTopToBottom` | `s.Top = a.Bottom` |
| `AlignBottomToTop` | `s.Top = a.Top - s.Height` |

Использовать существующий `AlignEach` (anchor не двигается). Поведение при `< 2` фигурах — как у
`AlignLeft` («Select at least two shapes…»).

### Core — `CommandCatalog`
Сменить execution с `Host` на `Layout` (→ `ServerLayout`) для четырёх edge-align команд, чтобы
сработал инвариант `Catalog_LayoutCommandsAreMarkedServerLayout` и маршрут `runServerLayout`.

### AddIn — `runCommand.ts`
| Command ID | Подход |
|------------|--------|
| `AlignLeftToRight` / `AlignRightToLeft` / `AlignTopToBottom` / `AlignBottomToTop` | Автоматически через `ServerLayout` после смены execution в каталоге (проверить, что default «not wired up yet» не срабатывает). |
| `CopyAndAlignLeft` | Клонировать каждую выделенную фигуру → собрать `ShapeBounds[]` (оригиналы + клоны, anchor = последняя исходная) → `api.applyLayout("AlignLeft", …)` → `applyShapeBounds`. |
| `CopyAndAlignRight` | То же + `AlignRight` |
| `CopyAndAlignTop` | То же + `AlignTop` |
| `CopyAndAlignBottom` | То же + `AlignBottom` |

Для CopyAndAlign: переиспользовать `cloneShapeOnSlide` / `duplicateShapesAtPositions` из S02-001.
Клон — на той же позиции, что источник (offset 0); затем layout выравнивает non-anchor относительно
anchor. Если Office.js `copyTo` смещает клон по умолчанию — после клона явно выставить left/top
источника перед align.

### Тесты (`PptPowerKeys.Tests`)
- Юнит-тесты для каждой из 4 edge-align команд в `LayoutEngineTests.cs` (anchor последний, 2+ фигур).
- `dotnet test PptPowerKeys.sln` — зелёный.
- `SettingsAndCatalogTests.Catalog_LayoutCommandsAreMarkedServerLayout` — без регрессии.

## Анти-scope
- Slides-команды (`CopySlide`, `ToggleZoom`, …) — S02-005.
- Единая деградация `support=None` + UX-бейджи — S02-006.
- Smart Duplicate с gap / памятью шага.
- Изменения `VstoLegacy*`, Api-контракта (если DTO не меняются).
- Client-side unit-тесты Office.js (не блокер).

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.Core/Layout/LayoutEngine.cs`
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs`
- `src/PptPowerKeys.Tests/LayoutEngineTests.cs`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` (если нужен helper для copy-and-align)

## Office.js feasibility
- `shape.left` / `shape.top` setters — Full (уже используются в `applyShapeBounds`).
- `copyTo` / `duplicate` — Partial (fallback из S02-001).
- CopyAndAlign — Partial (комбинация clone + ServerLayout).

## Критерии приёмки (Definition of Done)
1. [ ] `AlignLeftToRight`, `AlignRightToLeft`, `AlignTopToBottom`, `AlignBottomToTop` **не** попадают в default «not wired up yet»; выравнивают non-anchor фигуры к соответствующему ребру anchor.
2. [ ] `CopyAndAlignLeft/Right/Top/Bottom` **не** попадают в default «not wired up yet»; создают клон(ы) и выравнивают по соответствующей оси.
3. [ ] Математика edge-align — в `LayoutEngine`; покрыта юнит-тестами (≥1 тест на команду).
4. [ ] Edge-align команды помечены `ServerLayout` в `CommandCatalog`; `Catalog_LayoutCommandsAreMarkedServerLayout` проходит.
5. [ ] Понятные ошибки: нет выделения, < 2 фигур для align-команд.
6. [ ] `dotnet test PptPowerKeys.sln` — зелёный.
7. [ ] `npm run typecheck`, `npm run validate:prod` — зелёные.
8. [ ] PR с Task ID `S02-004`, ветка `cursor/S02-004-alignment-hostscript-51de`.

## Зависимости
- S02-001 (duplicate helpers) — в main.

## Примечание для builder
- Не дублируй layout-математику на клиенте — только Core + `api.applyLayout`.
- Не ломай существующие HostScript/ServerLayout команды.
- Ветка от актуального `main`; suffix `-51de` для cloud agent.

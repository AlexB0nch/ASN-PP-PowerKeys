# S05-004 — Multi-slide paste / remove (`PasteShapeToSelectedSlides`, `RemoveShapeFromSelectedSlides`)

> Передача builder'у: `/builder выполни S05-004`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S05-004` |
| **Спринт** | `sprint-05-advanced-features` |
| **Компонент** | Core + AddIn + Tests |
| **Статус** | In Progress |
| **Issue** | #37 |

## Цель

Две новые команды каталога **Objects** для README «Multi-slide Paste» (инновационные функции, **нет** parity в VSTO legacy):

1. **`PasteShapeToSelectedSlides`** — клонировать одну фигуру-шаблон с активного слайда на каждый выделенный слайд (кроме слайда-источника, если он в selection).
2. **`RemoveShapeFromSelectedSlides`** — удалить на всех выделенных слайдах фигуры с тем же `shape.name`, что у единственной выделенной фигуры на активном слайде.

Каталог: **77 → 79** команд. Обе — `OfficeJsSupport.Partial`, `ExecutionKind.HostScript`, **defaultShortcut: null** (README: User defined).

## Контекст

- S05-003 Done: `getSelectedSlides()` multi-select уже используется в `moveSelectedSlidesToBackup()`.
- S02-001: `cloneShapeOnSlide(context, source, slide)` в `powerpoint.ts` — clone + recreate fallback для textBox/line/geometricShape; ошибка неподдерживаемого типа уже есть.
- S02-004: `cloneSelectedShapesAtSourcePositions()` / `duplicateShapesAtPositions()` — образец clone на **текущем** слайде (`getSelectedSlides().getItemAt(0)`).
- Api `/api/commands` отдаёт каталог автоматически; `types.ts` не требует enum `CommandId`.
- **9 None-команд** в `SettingsAndCatalogTests.Catalog_NoneSupportCommands_AreExactlyNineKnownIds` — **не менять**.

### Office.js feasibility (зафиксировано architect)

| Аспект | Решение |
|--------|---------|
| Multi-slide selection | `presentation.getSelectedSlides()` — Full (S05-003) |
| Paste shape | **Partial** — `cloneShapeOnSlide()` per target slide; позиция как у source |
| Remove by name | **Partial** — iterate slides + `shape.name` match + `shape.delete()` |
| Cross-slide clone | **Partial** — `copyTo`/`duplicate` могут клонировать только на слайд source; fallback recreate на `targetSlide` обязателен |
| Execution | **HostScript** (категория **Objects**, не Slides) |

### Решения architect — `PasteShapeToSelectedSlides` (UX)

1. Пользователь выделяет **≥2 слайда** в deck (slide sorter / thumbnails).
2. На **активном** слайде выделена **ровно одна** фигура-шаблон (source).
3. Команда клонирует source на **каждый выделенный слайд, кроме слайда-источника** (если он тоже в selection).
4. Позиция клона: **те же `left` / `top` / `width` / `height`**, что у source (points).
5. Если source slide **не** в multi-selection — клонировать на **все** выделенные слайды.
6. Ошибки:
   - «Select two or more slides first.» — если `<2` slides в selection.
   - «Select exactly one shape on the active slide first.» — если `≠1` shape.
   - «Shape paste is not supported for this shape type on PowerPoint Web.» — при неподдерживаемом типе (можно обернуть/переформулировать текст из `cloneShapeOnSlide`, без «Try desktop…» в user-facing paste-сообщении).

### Решения architect — `RemoveShapeFromSelectedSlides` (UX)

1. Пользователь выделяет **≥1 слайд**.
2. На активном слайде выделена **ровно одна** фигура — берём **`shape.name`** как ключ удаления.
3. На каждом **выделенном** слайде удалить **все** shapes с тем же `name` (case-sensitive, как Office.js).
4. Если `name` пустой — ошибка: «Selected shape has no name. Name the shape first.»
5. Вернуть `{ slidesProcessed, shapesRemoved }` для status bar.
6. Если на каком-то слайде совпадений нет — **не ошибка**, просто 0 удалений на этом слайде.

## Scope

### 1. Core — две команды каталога

- `CommandIds.PasteShapeToSelectedSlides` (enum, секция **Objects**, после `SendBackward`)
- `CommandIds.RemoveShapeFromSelectedSlides` (enum, секция **Objects**, после paste)
- `CommandCatalog`:
  - `PasteShapeToSelectedSlides` — title «Paste shape to selected slides», Partial, HostScript, Objects
    - notes: multi-slide via `getSelectedSlides()`; clones source shape to each target slide
    - **defaultShortcut: null**
  - `RemoveShapeFromSelectedSlides` — title «Remove shape from selected slides (by name)», Partial, HostScript, Objects
    - notes: deletes all shapes matching `shape.name` on each selected slide
    - **defaultShortcut: null**
- Тесты `SettingsAndCatalogTests`:
  - `Catalog_CoversEveryCommandIdExceptNone` — покрывает новые ids автоматически
  - `Catalog_NoneSupportCommands_AreExactlyNineKnownIds` — **не менять** (9 None-команд)

### 2. AddIn — HostScript helpers (`powerpoint.ts`)

#### `pasteShapeToSelectedSlides(): Promise<number>`

- Один `PowerPoint.run` (или минимум runs) для эффективности.
- Алгоритм:
  1. `getSelectedSlides()` → `items.length >= 2`, иначе ошибка.
  2. `getSelectedShapes()` на active slide → ровно 1 shape (source).
  3. Загрузить source: `id, left, top, width, height, type` (+ text если textBox).
  4. Определить id слайда-источника: слайд, на котором лежит source (через `getSelectedSlides().getItemAt(0)` + shapes collection, или `source` parent slide id если API доступен).
  5. Для каждого target slide в selection:
     - если `targetSlide.id === sourceSlideId` — **skip**;
     - иначе `cloneShapeOnSlide(context, sourceShape, targetSlide)`;
     - установить `left`, `top` (width/height из clone / source).
  6. Вернуть count успешных вставок.

**Cross-slide clone:** `cloneShapeOnSlide` уже принимает `targetSlide`, но `copyTo`/`duplicate` могут создавать клон на слайде source. Для cross-slide:
- предпочесть recreate-path (`addTextBox` / `addLine` / `addGeometricShape` на `targetSlide`), **или**
- после `copyTo`/`duplicate` переместить клон на target slide, если API позволяет;
- **не дублировать** clone-логику — расширить существующий helper (export / wrapper `cloneShapeToSlide`).

#### `removeShapeFromSelectedSlides(): Promise<{ slidesProcessed: number; shapesRemoved: number }>`

- `getSelectedSlides()` → `>= 1` slide.
- Source shape name с active slide (ровно 1 selected shape).
- Пустой `name` → «Selected shape has no name. Name the shape first.»
- Для каждого selected slide:
  - `slide.shapes.load("items/name")` → filter by name (case-sensitive) → `shape.delete()`.
- `slidesProcessed` = число обработанных выделенных слайдов; `shapesRemoved` = сумма удалённых фигур.

**Рефакторинг (минимальный):** если `cloneShapeOnSlide` приватный — оставить приватным, но убедиться что cross-slide работает; при необходимости export или добавить тонкий wrapper; не копировать fallback recreate.

### 3. AddIn — `runCommand.ts`

```ts
case "PasteShapeToSelectedSlides": {
  const count = await pasteShapeToSelectedSlides();
  return outcomeSuccess(`Pasted shape to ${count} slide(s).`);
}
case "RemoveShapeFromSelectedSlides": {
  const { slidesProcessed, shapesRemoved } = await removeShapeFromSelectedSlides();
  return outcomeSuccess(`Removed ${shapesRemoved} shape(s) from ${slidesProcessed} slide(s).`);
}
```

- Ошибки helper → `outcomeError` с понятным текстом.
- Команды **не** попадают в default «not wired up yet».

### 4. Документация (минимум)

- `docs/migration/01-vsto-to-officejs-mapping.md` — две строки (README parity, Partial, HostScript, Objects).
- После merge — `docs/PRODUCT_CONTEXT.md` (79 команд, журнал S05-004).

## Анти-scope

- Slide sections / Backup hide-show (S05-003 anti-scope)
- Core layout math / новые Api endpoints
- Consulting profiles / snap-to-grid (S05-001/002)
- Smart Duplicate gap (S05-005)
- Default shortcut в catalog (User defined)
- Реестр `unsupportedWebCommands.ts` — **не менять** (новые команды Partial, не None)
- `VstoLegacy*`
- Удаление по типу фигуры / partial name match — только exact `shape.name`
- Paste на слайд-источник когда он в selection (explicit skip)

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/Commands/CommandIds.cs`
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `docs/migration/01-vsto-to-officejs-mapping.md`
- (post-merge) `docs/PRODUCT_CONTEXT.md`

## Критерии приёмки (Definition of Done)

1. [ ] `PasteShapeToSelectedSlides` и `RemoveShapeFromSelectedSlides` в `CommandIds` + `CommandCatalog` (Partial, HostScript, **Objects**, defaultShortcut null).
2. [ ] `pasteShapeToSelectedSlides()` — ≥2 slides, 1 source shape, skip source slide, same geometry; корректные ошибки UX.
3. [ ] `removeShapeFromSelectedSlides()` — ≥1 slide, remove by name, aggregates в status bar; пустое имя — ошибка; 0 matches на слайде — OK.
4. [ ] `cloneShapeOnSlide` (или wrapper) поддерживает **cross-slide** clone без дублирования логики.
5. [ ] `runCommand.ts` wired для обеих команд; нет fallback «not wired up yet».
6. [ ] `Catalog_NoneSupportCommands_AreExactlyNineKnownIds` — без изменений (9 None).
7. [ ] `dotnet test PptPowerKeys.sln` — зелёный.
8. [ ] `npm run typecheck`, `npm run validate:prod` — зелёные.
9. [ ] PR: ветка `cursor/S05-004-multi-slide-paste-remove-<suffix>`, Task ID в title/body, `Closes #<issue>`.
10. [ ] `.github/review/CHECKLIST.md` — scope, explicit degradation где Partial недоступен.
11. [ ] После merge: backlog S05-004 → **Done**; goals.md DoD P2 multi-slide отмечен; PRODUCT_CONTEXT → 79 команд.

## Зависимости

- S02-001 (`cloneShapeOnSlide`, duplicate/copy position) — в main.
- S05-003 (`getSelectedSlides` multi-select pattern) — в main.
- S05-001/002 — в main, **не блокеры**.

## Примечание для builder

- Ветка: `cursor/S05-004-multi-slide-paste-remove-<suffix>`
- Категория каталога: **Objects** (операции над фигурами на нескольких слайдах), не Slides.
- UX сообщений — в стиле S02-001 / S05-003 (конкретные ошибки, не generic).
- Partial: неподдерживаемый тип shape — явное сообщение paste, не красный generic.
- Один `PowerPoint.run` где возможно — меньше round-trips.
- Ручная проверка PowerPoint Online — post-merge (Pages + VDS).

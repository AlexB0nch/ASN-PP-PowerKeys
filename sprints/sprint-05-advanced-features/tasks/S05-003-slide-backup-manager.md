# S05-003 — Slide Backup Manager (`MoveSlidesToBackup`)

> Передача builder'у: `/builder выполни S05-003`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S05-003` |
| **Спринт** | `sprint-05-advanced-features` |
| **Компонент** | Core + AddIn + Tests |
| **Статус** | In Progress |
| **Issue** | #35 |

## Цель

Пользователь выделяет один или несколько слайдов → команда **перемещает их в конец презентации**
(«архив / Backup»). На PowerPoint Web **нет** slide sections API — реализуем только **move to end**,
без именованного раздела и без hide/show. Новая команда каталога `MoveSlidesToBackup` (77-я команда).

## Контекст

- README: «Переместить слайды в архив» — User defined shortcut; move to Backup section at end.
- VSTO legacy **не содержит** Backup — parity по README, не по COM-коду.
- S02-005: `duplicateSelectedSlide()` — образец Slides HostScript (`exportAsBase64` + `insertSlidesFromBase64`).
- `CopySlide` в `runCommand.ts` — образец wiring Slides-команды.
- Api `/api/commands` отдаёт каталог автоматически; `types.ts` не требует enum `CommandId`.

### Office.js feasibility (зафиксировано architect)

| Аспект | Решение |
|--------|---------|
| Move to end | **Partial** — `slide.moveTo` (PowerPointApi 1.8+) |
| Fallback | export → insert at end → delete source (как `CopySlide`, но insert **после последнего** слайда) |
| Named «Backup» section | **Anti-scope** |
| Hide/show Backup block | **Anti-scope** |
| Execution | **HostScript** |

### Решение architect по fallback

Если `slide.moveTo` недоступен — **не** ограничиваться одной ошибкой: реализовать fallback
`exportAsBase64` → `insertSlidesFromBase64` (target = last slide id) → `slide.delete()`.
Если и export/insert недоступны — ошибка: «Slide move is not supported on this PowerPoint version.»

## Scope

### 1. Core — новая команда каталога

- `CommandIds.MoveSlidesToBackup` (enum, секция Slides, **после** `CopySlide`, перед `PrintSlide`)
- `CommandCatalog`: Slides, `OfficeJsSupport.Partial`, `ExecutionKind.HostScript`
  - title: «Move slides to backup (end of deck)»
  - notes: move to end only; no slide sections on Web
  - **defaultShortcut: null** (README: User defined)
- Тесты `SettingsAndCatalogTests`:
  - `Catalog_CoversEveryCommandIdExceptNone` — покрывает новый id автоматически
  - при необходимости обновить счётчик/комментарии «76 commands» → **77** в docs после merge

### 2. AddIn — HostScript `moveSelectedSlidesToBackup()`

Файл: `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`

Поведение:

1. `getSelectedSlides()` → если 0: ошибка «Select one or more slides first.»
2. Загрузить все выделенные слайды (id + индекс в deck).
3. **Предпочтительно (`moveTo`, Api 1.8+):**
   - Загрузить `slides` collection, получить `lastIndex = slides.items.length - 1`.
   - Обрабатывать выделенные слайды **от большего индекса к меньшему** (descending), чтобы не сбивать индексы при перемещении.
   - Для каждого: `slide.moveTo({ startSlideIndex: lastIndex })` (или эквивалент API).
   - Относительный порядок группы выделенных слайдов сохраняется.
4. **Fallback** (если `moveTo` недоступен, но export/insert есть):
   - Для каждого выделенного слайда (тоже **descending по индексу**):
     - `exportAsBase64()` → `insertSlidesFromBase64(base64, { targetSlideId: <lastSlideId>, formatting: 'KeepSourceFormatting' })` → `slide.delete()`.
   - После insert «last slide» обновляется — перезагружать `slides` или вычислять id последнего слайда перед каждой итерацией.
5. Если ни `moveTo`, ни export/insert недоступны — «Slide move is not supported on this PowerPoint version.»
6. Вернуть **количество** перемещённых слайдов (`number`) для status bar.

### 3. AddIn — `runCommand.ts`

- `case "MoveSlidesToBackup"`: вызов `moveSelectedSlidesToBackup()`, success:
  «Moved N slide(s) to backup (end of deck).»
- Ошибки helper → `outcomeError` с понятным текстом
- Команда **не** попадает в default «not wired up yet»

### 4. Документация (минимум)

- `docs/migration/01-vsto-to-officejs-mapping.md` — строка `MoveSlidesToBackup`
- После merge — `docs/PRODUCT_CONTEXT.md` (77 команд, журнал S05-003)

## Анти-scope

- Slide sections (create/name «Backup»)
- Hide/show backup section
- Sort slides by selection order (отдельная README-команда — не в S05-003)
- Core layout math / Api endpoints
- Consulting profiles / snap-to-grid (S05-001/002)
- `VstoLegacy*`
- Default shortcut в catalog (User defined)
- Реестр `unsupportedWebCommands.ts` — не менять (новая команда Partial, не None)

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/Commands/CommandIds.cs`
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `docs/migration/01-vsto-to-officejs-mapping.md`
- (post-merge) `docs/PRODUCT_CONTEXT.md`

## Критерии приёмки (Definition of Done)

1. [ ] `MoveSlidesToBackup` в `CommandIds` + `CommandCatalog` (Partial, HostScript, Slides).
2. [ ] HostScript перемещает **все выделенные** слайды в **конец** deck (или graceful error).
3. [ ] `runCommand.ts` wired; нет fallback на generic «not wired up yet».
4. [ ] Unit-тесты каталога зелёные; при добавлении Core-логики — тесты без PowerPoint.
5. [ ] `dotnet test PptPowerKeys.sln` — зелёный.
6. [ ] `npm run typecheck`, `npm run validate:prod` — зелёные.
7. [ ] PR: ветка `cursor/S05-003-slide-backup-manager-<suffix>`, Task ID в title/body, `Closes #<issue>`.
8. [ ] `.github/review/CHECKLIST.md` — scope, explicit degradation где Partial недоступен.
9. [ ] После merge: backlog S05-003 → **Done**; goals.md DoD P2 (Backup) отмечен; PRODUCT_CONTEXT обновлён.

## Зависимости

- S02-005 (`CopySlide` HostScript) — в main.
- S05-001 — в main. S05-002 — в main, **не блокер**.

## Примечание для builder

- Ветка: `cursor/S05-003-slide-backup-manager-<suffix>`
- Slides = чистый HostScript; **не** добавлять layout в Core.
- UX сообщений — в стиле S02-005 (`CopySlide`, degradation для None-команд).
- Partial: если API недоступен — конкретная ошибка, не красный generic.
- Multi-select: алгоритм **descending by slide index** обязателен для обоих путей (moveTo и fallback).
- Ручная проверка PowerPoint Online — post-merge (Pages + VDS).

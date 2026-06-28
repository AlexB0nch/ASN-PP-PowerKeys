# S02-005 — Slides: CopySlide + деградация view/print

> Передача builder'у: `/builder выполни S02-005`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-005` |
| **Спринт** | `sprint-02-functionality` |
| **Компонент** | AddIn |
| **Статус** | Done |

## Цель
Закрыть последние 7 команд категории **Slides**, которые сейчас попадают в default
«not wired up yet» в `runCommand.ts`. Пользователь PowerPoint Online должен получить
рабочий **CopySlide** (Partial) и **конкретные** сообщения деградации для view/print-команд
(по образцу `Regroup` / `FormatPainter` / `PasteFormatted`).

## Контекст
После S02-001…004 default «not wired up yet» срабатывает **только** для Slides:

| Command ID | Support | Подход |
|------------|---------|--------|
| `CopySlide` | Partial | HostScript: дублировать текущий/выделенный слайд через Office.js |
| `ToggleZoom` | None | Явная деградация |
| `ToggleSlideSorter` | None | Явная деградация |
| `StartSlideShow` | None | Явная деградация |
| `ToggleGrid` | None | Явная деградация |
| `ToggleGuides` | None | Явная деградация |
| `PrintSlide` | None | Явная деградация (подсказка Ctrl+P / host print) |

Settings-команды (`OpenShortcutManager` и др.) — execution `Settings`, **не трогать**.

## Scope

### AddIn — `powerpoint.ts`
Добавить helper **`duplicateSelectedSlide()`** (или эквивалент):

1. Получить текущий слайд: `context.presentation.getSelectedSlides().getItemAt(0)`.
2. Если нет выделенного слайда — понятная ошибка («Select a slide first.»).
3. Дублирование (проверить requirement set / наличие API в runtime):
   - **Предпочтительно:** `slide.exportAsBase64()` → `presentation.insertSlidesFromBase64(base64, { targetSlideId, formatting: 'KeepSourceFormatting' })` — вставить копию **сразу после** текущего слайда.
   - Если `exportAsBase64` недоступен — явная ошибка: «Slide duplication is not supported on this PowerPoint version.»
4. Вернуть признак успеха (например `void` или номер слайда) для сообщения в `runCommand`.

См. `docs/migration/01-vsto-to-officejs-mapping.md` (Slides) и Microsoft docs:
`Slide.exportAsBase64`, `Presentation.insertSlidesFromBase64`.

### AddIn — `runCommand.ts`
Добавить `case` для всех 7 команд **до** `default`:

| Command ID | `ok` | Сообщение (ориентир, можно уточнить формулировку) |
|------------|------|---------------------------------------------------|
| `CopySlide` | true при успехе | «Slide duplicated.» |
| `ToggleZoom` | false | «Zoom is not available in PowerPoint Web. Use the host zoom controls.» |
| `ToggleSlideSorter` | false | «Slide sorter view is not available in PowerPoint Web.» |
| `StartSlideShow` | false | «Slide show cannot be started from the add-in on PowerPoint Web. Use Present / Slide Show in the host.» |
| `ToggleGrid` | false | «Grid toggle is not available in PowerPoint Web.» |
| `ToggleGuides` | false | «Guides toggle is not available in PowerPoint Web.» |
| `PrintSlide` | false | «Printing is not available from the add-in. Use Ctrl+P (Cmd+P) or the host Print command.» |

Для `CopySlide` — обработать ошибки helper'а (нет слайда, API недоступен).

### Проверка
После изменений **ни одна** из 7 Slides-команд не должна попадать в default
`'… is not wired up yet (Office.js support: …)'`.

## Анти-scope
- **S02-006** — единый UX бейджей для `support=None` (не менять UI каталога в этой задаче).
- **Core / Api** — не менять, если не требуется для Slides (математика layout не затрагивается).
- **VstoLegacy*** — не трогать.
- Client-side unit-тесты Office.js — не блокер.
- Settings-команды — не трогать.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`

## Office.js feasibility
- `getSelectedSlides()` — Full (уже используется).
- `Slide.exportAsBase64` + `insertSlidesFromBase64` — Partial / preview requirement set; graceful fallback с понятной ошибкой.
- View/zoom/sorter/slideshow/grid/guides/print — **None** (явная деградация).

## Критерии приёмки (Definition of Done)
1. [x] `CopySlide` дублирует текущий выделенный слайд через HostScript (или возвращает понятную ошибку, если API недоступен).
2. [x] `ToggleZoom`, `ToggleSlideSorter`, `StartSlideShow`, `ToggleGrid`, `ToggleGuides`, `PrintSlide` возвращают **конкретные** сообщения деградации (`ok: false`), не generic «not wired up yet».
3. [x] Ни одна из 7 Slides-команд не попадает в `default` в `runHostScript`.
4. [x] `dotnet test PptPowerKeys.sln` — зелёный (55 passed).
5. [x] `npm run typecheck`, `npm run validate:prod` — зелёные.
6. [x] PR #21: ветка `cursor/S02-005-slides-commands-1a4a`, Task ID `S02-005`, Closes #20.

## Приёмка (architect, 2026-06-28)
- PR #21, ветка `cursor/S02-005-slides-commands-1a4a`, merge commit `9487395` — **смержен в `main`**.
- Локально повторены `dotnet test` (55 passed), `npm run typecheck`, `npm run validate:prod` — зелёные.
- CHECKLIST: scope соблюдён, Slides — чистый HostScript, Core/Api/VstoLegacy не тронуты.
- `CopySlide`: `exportAsBase64` → `insertSlidesFromBase64` с `KeepSourceFormatting`; fallback при недоступном API.
- Ручная проверка в PowerPoint Online — post-merge (deploy Pages + VDS).

## Зависимости
- S02-001…004 — в main.

## Примечание для builder
- Slides — **чистый HostScript**; layout-математику в Core не добавлять.
- Сообщения деградации — в том же стиле, что `Regroup` / `FormatPainter` (коротко, по делу, без упоминания `descriptor.support` в тексте).
- Ветка от актуального `main`; suffix `-1a4a` для cloud agent.

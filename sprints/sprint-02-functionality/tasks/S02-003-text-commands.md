# S02-003 — Text: paste unformatted, ellipsis, superscript/subscript

> Передача builder'у: `/builder выполни S02-003`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-003` |
| **Спринт** | `sprint-02-functionality` |
| **Комponent** | AddIn |
| **Статус** | Done |

## Цель
Реализовать исполнение команд категории **Text**, которые сейчас падают в «not wired up yet».
Пользователь PowerPoint Online должен иметь рабочие paste plain text, замену текста на «…» и toggle
superscript/subscript; для `PasteFormatted` — явная деградация (support=`None`).

## Контекст
- `AddupTextFields` уже реализован (HostScript → `api.addup`).
- VSTO «ShortCut Tools» (`README.md`): Paste Unformatted (`Alt+V`), Replace with ellipsis (`Alt+.`),
  Superscript/Subscript (`Alt+J` / `Alt+Shift+J`).
- Text-команды — **HostScript** (`CommandCatalog`), математика layout не нужна.
- Office.js на Web: plain clipboard через `navigator.clipboard.readText()` (Partial, требует user gesture —
  нажатие кнопки в task pane подходит); rich clipboard недоступен.

## Scope
### HostScript — реализовать
| Command ID | Подход |
|------------|--------|
| `PasteUnformatted` | `navigator.clipboard.readText()` → вставить plain text в выделенные фигуры с текстом (`textFrame.textRange.text = ...`). Если выделена одна фигура без текста — попытаться вставить в textFrame; если несколько — применить ко всем с textFrame. Ошибки: пустой clipboard, нет выделения, permission denied. |
| `ReplaceWithEllipsis` | Для каждой выделенной фигуры с textFrame: `textRange.text = "..."` (три точки, как в VSTO/README). |
| `ToggleSuperscript` | Для выделенных фигур с текстом: toggle `textRange.font.superscript` (если true → false, иначе → true). При включении superscript — выключить subscript на той же фигуре (взаимоисключение, как в PowerPoint). |
| `ToggleSubscript` | Аналогично через `textRange.font.subscript`; при включении subscript — выключить superscript. |
| `PasteFormatted` | Явное сообщение: «Paste formatted is not supported on PowerPoint Web.» |

### Office.js helpers (`powerpoint.ts`)
- `pasteUnformattedText()` — clipboard read + apply to selection
- `replaceSelectedTextWithEllipsis()` — replace text with `"..."`
- `toggleSuperscript()` / `toggleSubscript()` — font property toggles
- Переиспользовать паттерн `getSelectedShapeTexts()` для обхода выделения; load `textFrame`, `textRange`, `font.superscript/subscript` перед sync.
- Понятные ошибки: нет выделения, фигуры без textFrame, clipboard API недоступен.

### `runCommand.ts`
- Добавить case'ы для всех пяти Command ID; не ломать существующие HostScript (Objects, Format, Addup).

## Анти-scope
- `PasteFormatted` эмуляция (rich clipboard).
- Alignment HostScript (`AlignLeftToRight` и др.) — отдельная задача S02-004 (или позже).
- Slides / Settings команды.
- Core / Api изменения (Text — чистый HostScript, кроме уже существующего Addup).
- `VstoLegacy*`.
- Unit-тесты на клиенте (опционально; не блокер — Office.js недоступен в CI).

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`

## Office.js feasibility
- `navigator.clipboard.readText()` — Partial (HTTPS + user gesture; PowerPoint Online OK).
- `textRange.text` — Full.
- `font.superscript` / `font.subscript` — Partial (новее requirement sets; graceful error если недоступно).
- `PasteFormatted` — None.

## Критерии приёмки (Definition of Done)
1. [x] `PasteUnformatted`, `ReplaceWithEllipsis`, `ToggleSuperscript`, `ToggleSubscript` **не** попадают в default «not wired up yet».
2. [x] `PasteFormatted` возвращает явное сообщение о неподдержке на Web.
3. [x] `ReplaceWithEllipsis` заменяет текст выделенных фигур на `"..."`.
4. [x] `PasteUnformatted` вставляет plain text из clipboard в выделение; понятная ошибка при пустом clipboard / denied permission.
5. [x] Superscript/subscript toggle работает на выделенных фигурах с текстом; взаимоисключение super/sub.
6. [x] `dotnet test PptPowerKeys.sln` — зелёный (47 passed).
7. [x] `npm run typecheck`, `npm run validate:prod` — зелёные.
8. [x] PR #18: ветка `cursor/S02-003-text-commands-7bcb`, Task ID `S02-003`.

## Приёмка (architect, 2026-06-28)
- PR #18, ветка `cursor/S02-003-text-commands-7bcb`, коммит `e0641fd` — **смержен в `main`** (`da97de4`).
- Локально повторены `dotnet test` (47 passed), `npm run typecheck`, `npm run validate:prod` — зелёные.
- CHECKLIST: scope соблюдён, Core/Api не тронуты, PasteFormatted — явная деградация, clipboard errors обработаны.
- Ручная проверка в PowerPoint Online — post-merge (deploy Pages + VDS).

## Зависимости
- Нет блокеров; S02-001/S02-002 в main.

## Примечание для builder
- Clipboard API вызывать только из обработчика команды (user gesture уже есть).
- Не ломай уже работающие HostScript-команды из main.
- Если `font.superscript` недоступен на платформе — вернуть понятное сообщение, не throw без обработки.
- Ветка от актуального `main`; suffix `-7bcb` для cloud agent.

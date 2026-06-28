# S02-006 — Единая деградация support=None + UX бейджи

> Передача builder'у: `/builder выполни S02-006`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S02-006` |
| **Спринт** | `sprint-02-functionality` |
| **Компонент** | AddIn (+ опционально Tests guard) |
| **Статус** | In Progress |

## Цель
После S02-001…005 **функциональный паритет закрыт** — все 76 каталоговых команд wired.
Остаётся **UX polish** для 9 команд с `support=None`: централизовать сообщения деградации,
не показывать красный «Error» для ожидаемого поведения на PowerPoint Web, добавить легенду бейджей.

## Контекст
9 команд `OfficeJsSupport.None` уже обрабатываются отдельными `case` в `runCommand.ts`:

| Command ID | Категория | Текущее сообщение (as-is в реестр) |
|------------|-----------|-------------------------------------|
| `Regroup` | Objects | Regroup is not supported on PowerPoint Web. |
| `FormatPainter` | Format | Format painter is not supported on PowerPoint Web. |
| `PasteFormatted` | Text | Paste formatted is not supported on PowerPoint Web. |
| `ToggleZoom` | Slides | Zoom is not available in PowerPoint Web. Use the host zoom controls. |
| `ToggleSlideSorter` | Slides | Slide sorter view is not available in PowerPoint Web. |
| `StartSlideShow` | Slides | Slide show cannot be started from the add-in on PowerPoint Web. Use Present / Slide Show in the host. |
| `ToggleGrid` | Slides | Grid toggle is not available in PowerPoint Web. |
| `ToggleGuides` | Slides | Guides toggle is not available in PowerPoint Web. |
| `PrintSlide` | Slides | Printing is not available from the add-in. Use Ctrl+P (Cmd+P) or the host Print command. |

`App.tsx` рисует бейджи (Full=green, Partial=yellow, None=red), но:
- нет легенды;
- None-команды кликабельны → status bar показывает красный badge «Error» (`ok: false`);
- сообщения дублируются в 9 case-блоках.

## Scope

### 1. Централизованный реестр — `unsupportedWebCommands.ts`
Создать `src/PptPowerKeys.AddIn/src/taskpane/unsupportedWebCommands.ts` (или рядом):

```ts
export const UNSUPPORTED_WEB_COMMANDS: Record<string, string> = {
  Regroup: "Regroup is not supported on PowerPoint Web.",
  // ... все 9 id
};

export function getUnsupportedWebMessage(commandId: string): string | undefined;
export function runUnsupportedWebCommand(commandId: string): CommandOutcome;
```

В `runCommand.ts` — **early return** в `runHostScript` (до `switch` или в начале `switch` через helper) вместо 9 отдельных `case`.
Удалить дублирующие `case` для этих 9 команд.

### 2. `CommandOutcome` — различать деградацию и ошибку
Расширить интерфейс, например:

```ts
export type CommandOutcomeKind = "success" | "unsupported" | "error";

export interface CommandOutcome {
  kind: CommandOutcomeKind;
  message: string;
  // ok?: deprecated — заменить на kind или оставить ok как derived (kind === "success")
}
```

- `unsupported` — ожидаемая деградация на Web (`support=None`);
- `error` — реальная ошибка (нет выделения, API недоступен, exception);
- `success` — команда выполнена.

Helper `runUnsupportedWebCommand` возвращает `{ kind: "unsupported", message }`.

Обновить все существующие return в `runCommand.ts` на новую форму (`kind` вместо/вместе с `ok`).

### 3. `App.tsx` UX
- **Легенда** под заголовком / caption: зелёный = Full, жёлтый = Partial, красный = Not on Web.
- Для `cmd.support === "None"`:
  - **Вариант A (предпочтительный):** кнопка остаётся кликабельной, но status bar показывает **warning** badge («Not on Web»), не красный «Error»;
  - **Вариант B:** кнопка `disabled` + tooltip.
- **Tooltip** для None: префикс «Not available on PowerPoint Web» + краткая причина из `cmd.notes` (если есть).
- Status bar: `kind === "success"` → green/OK; `kind === "unsupported"` → warning/«Not on Web»; `kind === "error"` → danger/Error.

### 4. Default «not wired up yet»
Оставить в `default` case `runHostScript`, добавить комментарий:
`// Safety-net for unknown ids — should never fire for catalog commands after S02-005.`
Возвращать `kind: "error"`.

### 5. Guard-тест (желательно)
В `PptPowerKeys.Tests/SettingsAndCatalogTests.cs` (или AddIn):
- Все команды каталога с `OfficeJsSupport.None` — ровно 9 id из списка выше (защита от новых None без реестра).
- Опционально: статический список всех `HostScript` + `ServerLayout` id и проверка, что реестр None покрывает подмножество.

Полная проверка покрытия `runCommand.ts` в CI без парсинга TS — опционально; достаточно None-guard + ручная сверка.

## Анти-scope
- Новые команды / Core layout-математика
- Settings UI (Shortcut Manager, Color Scheme) — отдельный спринт
- Smart Color Picker, VstoLegacy
- Изменение текстов деградации без необходимости (перенести as-is)
- Изменение `CommandCatalog` / `types.ts` (не требуется)

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/src/taskpane/unsupportedWebCommands.ts` (новый)
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx`
- `src/PptPowerKeys.Tests/SettingsAndCatalogTests.cs` (guard, опционально)

## Критерии приёмки (Definition of Done)
1. [ ] Все 9 None-команд обрабатываются через единый реестр/helper (нет отдельных case).
2. [ ] Легенда бейджей видна в UI (Full / Partial / Not on Web).
3. [ ] Клик по None-команде не показывает красный «Error» — warning «Not on Web» или disabled + tooltip.
4. [ ] Существующие сообщения деградации сохранены по смыслу (as-is из таблицы).
5. [ ] `dotnet test PptPowerKeys.sln` (55+), `npm run typecheck`, `npm run validate:prod` — зелёные.
6. [ ] PR: ветка `cursor/S02-006-unsupported-ux-3541`, Task ID `S02-006`, Closes #<issue>.

## Зависимости
- S02-001…005 — Done в `main`.

## Примечание для builder
- Ветка от актуального `main`; suffix `-3541` для cloud agent.
- `CommandCatalog` ↔ `types.ts` не менять.
- После PR — architect примет и закроет Sprint 02.

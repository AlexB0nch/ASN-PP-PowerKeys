# S06-005 — Color Picker eyedropper / HEX

> Передача builder'у: `/builder выполни S06-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-005` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | AddIn |
| **Статус** | In Progress |
| **Issue** | #56 |
| **PR** | TBD |

## Цель

Дополнить Smart Color Picker двумя возможностями из README / Sprint 04 stretch:

1. **HEX input** — поле ввода `#RRGGBB` (или `RRGGBB`), валидация, обновление preview/selectedColor, Apply как у swatches.
2. **Eyedropper** — pragmatic UX в рамках Office.js (см. feasibility ниже).

## Контекст (после S04 + S06-001…004)

| Компонент | Состояние |
|-----------|-----------|
| `ColorPickerPanel.tsx` | Theme swatches (≤10) + recent (≤5), preview HEX, Apply Fill/Line/Text ✓ |
| Theme read | `readPresentationThemeColors()` + fallback `DEFAULT_PALETTE` + MessageBar ✓ |
| Recent persist | `localStorage` (`ppt-powerkeys-recent-colors`), shared с Fill/Line/Text cycle ✓ |
| Core palette merge | `ColorPaletteBuilder` + `POST /api/colors/build-palette` ✓ |
| `normalizeHex()` | AddIn `formatColorState.ts` ✓ |
| `ThemeColor.IsValidHex/NormalizeHex` | Core ✓ |
| **Gap** | Нельзя ввести произвольный HEX; нет «пипетки» |

## Решения architect — Eyedropper feasibility (зафиксировано)

**Office.js НЕ даёт native eyedropper по слайду.** В Sprint 04 отложено из‑за Web limitations.

| Подход | Где работает | Решение |
|--------|--------------|---------|
| **A. Pick from selection** | Desktop + Web | **Обязательно (минимум DoD).** Кнопка «Pick from shape»: читать `fill.foregroundColor` / `lineFormat.color` / `textRange.font.color` у выделенной фигуры через Office.js; нормализовать → `selectedColor` + `recordRecentColor()`. |
| **B. Browser EyeDropper API** | Chromium WebView2 (Win Desktop), частично Web | **Optional bonus.** `window.EyeDropper` — screen pick; feature-detect; hide/disable + hint на unsupported. |
| **C. Native PP eyedropper UI** | — | **Out of scope** — недоступно add-in API |

**Минимальный DoD:** HEX input (обязательно) + хотя бы один eyedropper-путь (A предпочтительнее; B — optional bonus с graceful degradation).

### HEX validation

- Reuse `normalizeHex()` в AddIn; mirror `ThemeColor.IsValidHex` (`^#?[0-9a-fA-F]{6}$`).
- Вынести `isValidHex()` в `formatColorState.ts` (или отдельный helper) — **добавить unit tests** (Vitest/Jest если есть, иначе простой TS test file в AddIn или mirror в Core tests не нужен если логика только в TS).
- Invalid → inline error / MessageBar, **не crash**.
- Valid → обновить `selectedColor`, preview swatch; Enter или кнопка «Apply color» → тот же flow что swatch click.
- После pick/apply — `recordRecentColor()` как у swatches.

### Pick from selection (подход A)

Новая функция в `powerpoint.ts`, например `readColorFromSelection(source: 'fill' | 'line' | 'text')`:

- Требует ровно 1 выделенную фигуру (или первую из выделения — document: **первую**).
- `fill`: `shape.fill.foregroundColor` (уже используется в `toggleFillBlackWhite`).
- `line`: `shape.lineFormat.color`.
- `text`: `shape.textFrame.textRange.font.color` (catch no text frame).
- Нормализовать через `normalizeHex`; если не `isValidHex` после normalize — понятная ошибка.
- UI: одна кнопка «Pick from shape» с dropdown/radio Fill|Line|Text **или** три маленькие кнопки — на усмотрение builder (минимальный diff).

### EyeDropper API (подход B, optional)

```ts
interface EyeDropper {
  open(): Promise<{ sRGBHex: string }>;
}
```

- Feature-detect: `'EyeDropper' in window`.
- Кнопка «Screen pick» только когда supported; иначе disabled + Caption1 hint.
- `sRGBHex` уже `#RRGGBB` — normalize → selectedColor + recordRecentColor on success.

## Scope builder

| Компонент | Файлы |
|-----------|-------|
| AddIn | `ColorPickerPanel.tsx`, `formatColorState.ts` (`isValidHex`), `powerpoint.ts` (`readColorFromSelection`) |
| Tests | TS tests для `isValidHex` / `normalizeHex` edge cases (если test runner есть); иначе минимальный test file |
| Docs | post-merge: architect обновит `PRODUCT_CONTEXT.md`, README, `goals.md` |

## Анти-scope

- Новые CommandIds
- Изменение `ColorPaletteBuilder` merge logic (если не нужно для HEX)
- Persist custom colors в UserSettings/Api (recent остаётся localStorage)
- VSTO legacy / COM
- Canvas pixel sampling / screenshot hacks
- RGB sliders / HSL picker (только HEX достаточно для parity README)

## Затрагиваемые файлы (ожидаемо)

| Область | Файлы |
|---------|-------|
| AddIn UI | `src/PptPowerKeys.AddIn/src/taskpane/ColorPickerPanel.tsx` |
| AddIn office | `src/PptPowerKeys.AddIn/src/office/formatColorState.ts`, `powerpoint.ts` |
| Tests | `formatColorState.test.ts` или аналог |

## Критерии приёмки (Definition of Done)

### AddIn — HEX
- [ ] Text field HEX + Enter или кнопка → valid color в preview + можно Apply Fill/Line/Text
- [ ] Invalid HEX → inline error / MessageBar; panel не ломается
- [ ] Typed color после Apply → `recordRecentColor()` → появляется в Recent

### AddIn — Eyedropper
- [ ] «Pick from shape» (fill/line/text) работает на Desktop + Web при валидном выделении
- [ ] (Optional) EyeDropper API с feature gate; unsupported → disabled + hint
- [ ] Picked color → preview + recent list

### Регрессия
- [ ] Theme/recent swatches + Apply Fill/Line/Text без регрессии
- [ ] Fill/Line/Text cycle commands (hotkeys) без регрессии

### CI
- [ ] `dotnet test PptPowerKeys.sln` — зелёный (143+)
- [ ] `npm run typecheck`, `validate:prod`, `build:prod` — зелёные

### PR
- [ ] Ветка: `cursor/S06-005-color-picker-eyedropper-hex-6c25`
- [ ] `Closes #<issue>`; CHECKLIST; manual note: PP Desktop + Online (HEX обязательно; eyedropper — best effort)

## Красные флаги (reject)

- RGB sliders / HSL picker / новый API endpoint
- Новые CommandIds
- Persist custom colors в UserSettings
- Canvas/screenshot sampling
- Over-engineered color picker widget вместо Input + validation
- Регрессия theme/recent swatches или Apply buttons

## Зависимости

- S04-001…003 Color Picker foundation ✓
- `normalizeHex` / `ThemeColor.IsValidHex` ✓

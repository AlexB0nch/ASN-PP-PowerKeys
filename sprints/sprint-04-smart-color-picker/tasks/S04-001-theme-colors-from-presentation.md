# S04-001 — Theme colors from presentation (Office.js + Core palette merge)

> Передача builder'у: `/builder выполни S04-001`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S04-001` |
| **Спринт** | `sprint-04-smart-color-picker` |
| **Компонент** | Core + AddIn + Tests |
| **Статус** | In Progress |

## Цель
Заменить фиксированную `DEFAULT_PALETTE` в `formatColorState.ts` на **реальные theme colors
презентации** где Office.js позволяет читать их (Desktop; Partial на Web), с graceful fallback
на дефолтную палитру. Вынести merge theme + recent в **тестируемый Core helper** — фундамент для
Color Picker UI (S04-002) и интеграции Fill/Line/Text (S04-003).

## Контекст
- S02-002: `FillColor`/`LineColor`/`TextColor` циклят `getActivePalette()` = `DEFAULT_PALETTE` (10 hex) + 5 recent in-memory.
- `OpenColorScheme` — stub «planned (Sprint 04)»; UI — S04-002.
- Core: `ThemeColor.cs` — normalize/luminance, без palette engine.
- VSTO legacy `ColorSchemeReader.cs` — read-only reference; **не трогать**.
- Office.js (PowerPointApi 1.10): `slide.themeColorScheme.getThemeColor(ThemeColor.accentN)`;
  `presentation.slideMasters.items[0].themeColorScheme` — предпочтительно для Slide Master parity.
- **Web caveat:** `themeColorScheme` на PowerPoint Online может падать с `GeneralException` (issue #6582) —
  builder обязан обработать и вернуть fallback без краша.

## Scope

### Core — palette merge (pure logic)
- Новый класс/статический helper в `PptPowerKeys.Core/Colors/` (например `ColorPaletteBuilder`):
  - Вход: `IReadOnlyList<string> themeColors`, `IReadOnlyList<string> recentColors`, опционально `IReadOnlyList<string> fallbackTheme`.
  - Выход: упорядоченный список hex `#RRGGBB` (dedupe, normalize через `ThemeColor.NormalizeHex` / `IsValidHex`).
  - Правило merge (VSTO parity): **сначала theme (до 10), затем recent (до 5)**, без дубликатов.
  - Константы: `MaxThemeColors = 10`, `MaxRecentColors = 5` (или эквивалент).
- Юнит-тесты в `PptPowerKeys.Tests`: merge, dedupe, invalid hex skip, empty theme → fallback, recent ordering.

### AddIn — Office.js theme read
- Новый модуль `src/PptPowerKeys.AddIn/src/office/themeColors.ts`:
  - `readPresentationThemeColors(): Promise<ThemeColorsResult>` где result = `{ colors: string[]; source: 'slideMaster' | 'slide' | 'fallback'; warning?: string }`.
  - Проверка `Office.context.requirements.isSetSupported('PowerPointApi', '1.10')`.
  - Попытка чтения: `presentation.slideMasters` → первый master → `themeColorScheme.getThemeColor` для **accent1–accent6** + **dark1, dark2, light1, light2** (10 цветов как VSTO/README).
  - Fallback при ошибке/unsupported: `DEFAULT_PALETTE` из `formatColorState.ts`.
  - Нормализация hex (ensure `#` prefix, uppercase) — reuse `normalizeHex` из `formatColorState.ts` или Core-совместимая логика.
- Рефактор `formatColorState.ts`:
  - Экспорт `DEFAULT_PALETTE` (уже есть).
  - Добавить `setThemeColors(colors: string[] | null)` / `getThemeColorSource()` — in-memory theme override.
  - `getActivePalette()` вызывает Core merge (через тонкий TS wrapper **или** duplicate-free local call — предпочтительно HTTP к Api **не нужен**; можно портировать merge в TS **только если** Core недоступен из AddIn напрямую — **AddIn не ссылается на Core**; merge дублировать в TS **нельзя**).
  - **Решение architect:** expose merge через **новый Api endpoint** `POST /api/colors/merge-palette` **ИЛИ** duplicate minimal merge in TS matching Core tests. **Предпочтение: duplicate merge logic in TS is bad.** Лучше: **Api endpoint** `POST /api/colors/build-palette` body `{ themeColors, recentColors }` → `{ palette: string[] }` calling Core. Это тонкий слой, тестируется integration test.
  - **Упрощение (architect decision):** merge остаётся в **Core only**; AddIn вызывает **Api** `POST /api/colors/build-palette`. S04-001 добавляет endpoint + Core + AddIn client method. Альтернатива без сети: copy merge to TS with comment "must match Core" — **отклонено**, используем Api (уже есть HTTP client).

### AddIn — bootstrap theme load
- При `Office.onReady` или первом color command: вызвать `readPresentationThemeColors()`, передать в `setThemeColors`, опционально лог/warning в console.
- **Не** менять UX stub `OpenColorScheme` (S04-002).
- **Не** persist recent (S04-003).

### Api (тонкий слой)
- `POST /api/colors/build-palette` — DTO `{ themeColors?: string[], recentColors?: string[], fallbackTheme?: string[] }` → `{ palette: string[] }`.
- Контракт в `Api/Contracts`, типы в `AddIn/src/services/types.ts`.

## Анти-scope
- Color Picker UI / `OpenColorScheme` wiring — S04-002.
- Persist recent (localStorage / UserSettings) — S04-003.
- Изменение поведения `recordRecentColor` / cycle index — минимально, только если нужно для merge.
- Eyedropper, HEX input.
- `VstoLegacy*`, FormatPainter, SSO.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.Core/Colors/ColorPaletteBuilder.cs` (новый)
- `src/PptPowerKeys.Api/Contracts/ColorPaletteContracts.cs` (новый)
- `src/PptPowerKeys.Api/Program.cs` — endpoint
- `src/PptPowerKeys.Tests/ColorPaletteBuilderTests.cs` (новый)
- `src/PptPowerKeys.AddIn/src/office/themeColors.ts` (новый)
- `src/PptPowerKeys.AddIn/src/office/formatColorState.ts` — theme override + Api merge call
- `src/PptPowerKeys.AddIn/src/services/api.ts`, `types.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/App.tsx` или `index.tsx` — bootstrap theme read (минимально)

## Критерии приёмки (Definition of Done)
1. [ ] Core `ColorPaletteBuilder` merge: theme (≤10) + recent (≤5), dedupe, normalize; ≥5 unit-тестов.
2. [ ] Api `POST /api/colors/build-palette` вызывает Core; integration test зелёный.
3. [ ] AddIn `readPresentationThemeColors()` читает accent1–6 + dark1/2 + light1/2 с slide master при PowerPointApi 1.10; при ошибке — `DEFAULT_PALETTE` + `source: 'fallback'`.
4. [ ] `getActivePalette()` использует theme colors после bootstrap (не только hardcoded defaults).
5. [ ] `FillColor`/`LineColor`/`TextColor` **продолжают работать** (regression); cycle использует расширенную палитру если theme прочитан.
6. [ ] `dotnet test PptPowerKeys.sln` — зелёный.
7. [ ] `npm run typecheck` — зелёный.
8. [ ] PR: ветка `cursor/S04-001-theme-colors-from-presentation-6fba`, Task ID `S04-001`, `Closes #<issue>`.

## Зависимости
- S02-002 (`formatColorState.ts`, color HostScript) — в main.
- Нет блокеров.

## Примечание для builder
- Проверь requirement set 1.10 в `manifest.template.xml` / `package.json` office types если нужно.
- Web: не крашить task pane при `GeneralException` на theme read — silent fallback.
- Ветка: `cursor/S04-001-theme-colors-from-presentation-6fba`.
- `OpenColorScheme` stub **не убирать** в этой задаче.

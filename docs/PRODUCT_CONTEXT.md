# PRODUCT_CONTEXT — PPT PowerKeys

> Единый источник правды о продукте. Владелец — агент `architect`. Обновляется при каждом решении,
> меняющем продукт. Цель — чтобы любая новая сессия (Desktop или Cloud) за минуты восстановила контекст.

## 1. Что это за продукт
**PPT PowerKeys** — надстройка для Microsoft PowerPoint, воспроизводящая/расширяющая «ShortCut Tools»
(>100 команд для быстрой работы со слайдами: выравнивание относительно опорной фигуры, ресайз, операции
с объектами, форматирование, текст, слайды). Продуктовая спецификация функционала — в `README.md`.

Проект **мигрирует с Windows-only VSTO на кроссплатформенный Office Web Add-in**
(Windows / Mac / Web / iPad). Для **LTSC / perpetual Office** без sideload Web Add-in — **вторая официальная line**
`PptPowerKeys.Windows` (VSTO + Core in-process). См. [`docs/migration/04-powerpoint-ltsc-windows-native.md`](migration/04-powerpoint-ltsc-windows-native.md).

## 2. Product lines

| Line | Платформа | Доставка | Проекты |
|------|-----------|----------|---------|
| **A — Web Add-in (primary)** | M365, Online, Desktop, Mac | `manifest.prod.xml` | AddIn + Api + Core |
| **B — Windows Native (LTSC)** | Windows LTSC / perpetual | MSI / ClickOnce (S11) | `PptPowerKeys.Windows` + Core (S07+) |

**Track 0 (Web на LTSC без Upload UI):** [`docs/migration/05-ltsc-web-addin-central-deploy.md`](migration/05-ltsc-web-addin-central-deploy.md).

### Архитектура Line A (`docs/migration/00-architecture.md`)
```
PowerPoint (Desktop/Web/Mac/iPad)
  Task Pane (React + Fluent UI + Office.js)   → src/PptPowerKeys.AddIn
        │ HTTP REST (JSON)
  ASP.NET Core Minimal API (Swagger)          → src/PptPowerKeys.Api
        │ ссылка на проект
  PptPowerKeys.Core (.NET 8, чистый C#)        → src/PptPowerKeys.Core
```
| Проект | Назначение |
|--------|------------|
| `src/PptPowerKeys.Core` | .NET 8 class library, чистый C# без `Microsoft.Office.*`. Вся переносимая логика (`LayoutEngine`, `DuplicationEngine`, `NumberAggregator`, `CommandCatalog`, `ShapeBounds`). |
| `src/PptPowerKeys.Api` | ASP.NET Core Minimal API + Swagger. Тонкий слой поверх Core. |
| `src/PptPowerKeys.Tests` | xUnit: тесты Core + интеграционные тесты API. Не требуют PowerPoint. |
| `src/PptPowerKeys.AddIn` | Office Web Add-in: TypeScript + React + Fluent UI + Office.js. |
| `src/PptPowerKeys.VstoLegacy*` | **Заморожен** scaffold. **Не** развивать — новый Windows host: `PptPowerKeys.Windows` (S07+). |

Корневой `PptPowerKeys.sln` = Core + Api + Tests (кроссплатформенно).
`PptPowerKeys.Windows.sln` — S07+ (Windows + VSTO). `VstoLegacy.sln` — исторический reference only.

## 3. Инварианты / правила (нарушать нельзя)
- **Граница `ShapeBounds`** (`{id,left,top,width,height}` в points): host читает геометрию через Office.js →
  Core считает чистыми функциями → host применяет обратно по `id`. Никакой математики layout на Office-типах.
- **Anchor = последняя выделенная фигура** (`AnchorIndex` переопределяет). Сохранено из VSTO.
- `Core` свободен от Office/ASP.NET/UI; `Api` — тонкий; бизнес-логика покрыта тестами **без PowerPoint**.
- DTO в `Api/Contracts` синхронизированы с `AddIn/src/services/types.ts`; команды — строковые имена (`"AlignLeft"`).
- Новый код только в `Core`/`Api`/`AddIn`; `VstoLegacy*` — заморожен.

## 4. API (эндпоинты)
| Метод | Путь | Назначение |
|-------|------|------------|
| GET | `/health` | health-check |
| GET | `/api/commands` (`/{id}`) | каталог команд + Office.js feasibility |
| POST | `/api/layout/apply` | применить геометрическую команду к `ShapeBounds[]` |
| POST | `/api/objects/duplicate-offset` | позиция дубликата (smart-duplicate) |
| POST | `/api/text/addup` | сумма/мин/макс/среднее чисел из текста |
| GET/PUT | `/api/settings`, POST `/api/settings/reset`, POST `/api/settings/import` | профиль + шорткаты; file-backed JSON per user (`SETTINGS_DATA_PATH`, Docker volume `settings_data`); import — validate-only |

## 5. Окружение и команды
Подробности и нюансы Cloud — в `AGENTS.md`. Кратко:
- Backend: `dotnet test PptPowerKeys.sln`; запуск API `cd src/PptPowerKeys.Api && dotnet run` (http://localhost:5168, `/swagger`).
- Add-in: `cd src/PptPowerKeys.AddIn && npm ci`; dev `npm start` (https://localhost:3000); проверки `npm run typecheck|validate|build`.
- **Production (PowerPoint Online):** `manifest.dev.xml` (localhost) vs `manifest.prod.xml` (генерируется из `manifest.template.xml`);
  `npm run build:prod` / `validate:prod`; статика на GitHub Pages, API на Azure. Runbook: `docs/migration/02-powerpoint-web-deploy.md`.
- CI: `.github/workflows/ci.yml` (jobs: backend .NET 8 + add-in TypeScript на ubuntu); deploy add-in — `.github/workflows/deploy-addin-pages.yml`.
- Реальный sideload в PowerPoint требует Office (Desktop/Web); автотесты не покрывают загрузку iframe в браузере.

## 6. Дорожная карта / статус
Статусы — в `sprints/` и в `docs/migration/00-architecture.md` (Definition of Done эпика миграции).

**Текущее состояние (2026-06-29):** сквозной путь **подтверждён в живом PowerPoint Online** — надстройка
загружается (manifest на GitHub Pages, task pane «PptPowerKeys (Web)»), тянет каталог из **API на собственном
VDS** (`https://95.140.152.103.sslip.io`, HTTPS через Caddy + Let's Encrypt, деплой по SSH через GitHub Actions),
рендерит **79 команд** по категориям. **ServerLayout**, **HostScript** (Objects, Format, Text, Alignment, Slides)
и **Settings** исполняются; 9 команд `support=None` — через единый реестр `unsupportedWebCommands.ts` с warning-UX
(не красный Error). Default «not wired up yet» — safety-net для неизвестных id.

**Sprint 02 завершён (2026-06-28):** S02-001…006 Done (Objects, Format, Text, Alignment, Slides, unsupported UX).
**Sprint 03 завершён (2026-06-28):** S03-001…003 Done. Settings UI, persistent store, Shortcut Manager.
**Sprint 04 завершён (2026-06-28):** S04-001…003 Done (PR #29–#31). Smart Color Picker.
**Sprint 05 завершён (2026-06-29):** S05-001 Done (PR #32) Consulting profiles McKinsey/BCG;
S05-002 Done — snap-to-grid 0.1 cm (PR #34); S05-003 Done — `MoveSlidesToBackup` (PR #36);
S05-004 Done — multi-slide paste/remove (PR #39); S05-005 Done — Smart Duplicate gap memory (PR #42).
Anti-scope: snap-to-nearest-object, slide sections hide/show.

**Sprint 06 завершён (2026-06-29):** S06-001 Done (PR #46) — Shared Runtime + Tier 1 keyboard shortcuts;
S06-002 Done (PR #49) — `replaceShortcuts` sync с UserSettings (76 hotkey-eligible actions); Desktop Windows 2601+ target.
S06-003 Done — import/export settings JSON (PR #52). S06-004 Done — Object Statistics MIN/MAX/AVG UI (PR #55).
S06-005 Done — Color Picker HEX input + eyedropper (PR #57).

**Epic LTSC Windows Native (planned S07–S11):** ADR-001 — product line B `PptPowerKeys.Windows` (VSTO/COM +
Core in-process) для LTSC/perpetual Office. **Sprint 07 Done** (M1 prototype); **Sprint 08 in progress** (M2 layout parity).
См. `sprints/epic-ltsc-windows-native/ROADMAP.md`.

## 7. Журнал ключевых решений (анти-дрейф контекста)
- **S08-004:** Первая HostScript-волна Windows — **4** Copy-and-align команды (CopyAndAlignLeft/Right/Top/Bottom);
  pipeline: `ReadSelectedShapeBounds` → `CloneSelectedAtSourcePositions` (COM Duplicate at source Left/Top) →
  combined + `anchorIndex = originals.Count - 1` → `LayoutEngine.Apply` (mapped Align*) → `ApplyShapeBoundsOnSlide` (by id);
  `CommandExecutionResult` unified return; `HostScriptCommandMap` отдельно от layout-only `RibbonCommandMap`;
  ribbon group **Copy & Align** → `OnHostScriptCommand`; snap via S08-002 `LayoutOptions`; parity с Web `runCopyAndAlign`;
  `CopyAndAlignCommandsTests` (7 tests); manual QA matrix в `PptPowerKeys.Windows/README.md`. PR #69; 153 dotnet tests green.
- **S08-003:** Ribbon layout parity — **32** ServerLayout кнопки на вкладке **PowerKeys** в 6 группах
  (Alignment 8, Stack 4, Size 6, Stretch 4, Nudge Large 6, Nudge Small 4) + **Options** (snap checkbox из S08-002);
  единый `OnLayoutCommand` → `RibbonCommandMap.TryParse(btn{CommandIds})` → `CommandRouter.Execute`;
  Bootstrap «Test» удалён; `imageMso` по VstoLegacy где возможно (Nudge Small → `HappyFace` fallback);
  manual QA matrix per group в `PptPowerKeys.Windows/README.md`. PR #67; 146 dotnet tests green.
- **S08-002:** Windows snap-to-grid parity с Web S05-002 — `WindowsUserSettingsStore` persist
  `%AppData%/PptPowerKeys/UserSettings.json` (camelCase `snapToGrid`, Web export/import v1 compatible);
  `CommandRouter` передаёт `LayoutOptions { SnapToGrid }` во все 32 ServerLayout; ribbon checkbox
  «Snap to grid (0.1 cm)» toggle + immediate save; `GridStepCm` = default 0.1 (Core `GridSnap` unchanged);
  unit tests `WindowsUserSettingsStoreTests` (linked in `PptPowerKeys.Tests` for Linux CI); manual QA AlignLeft +
  SameWidth в `PptPowerKeys.Windows/README.md`. PR #65; 146 dotnet tests green.
- **S08-001:** `CommandRouter` — generic dispatch через `LayoutEngine.IsLayoutCommand()` для всех **32 ServerLayout**
  команд (Alignment 12 + Resize 20); один `ExecuteServerLayout` (COM → `ShapeBounds[]` → `LayoutEngine.Apply` → write back);
  `LayoutOptions` = null (snap-to-grid — S08-002); non-layout → `NotSupportedException`; ribbon по-прежнему только AlignLeft
  (полный layout ribbon — S08-003); manual QA note AlignLeft + SameWidth + DistributeHorizontal в `PptPowerKeys.Windows/README.md`.
  PR #63; 143 dotnet tests green.
- **ADR-001 / LTSC line:** Вторая официальная product line для PowerPoint LTSC/perpetual Windows — **Variant D**
  (VSTO host + in-process Core); не PPAM/VBA; не размораживать `VstoLegacy*`; новый проект `PptPowerKeys.Windows`;
  полный паритет 79 cmd + unlock 9 `OfficeJsSupport.None`; hotkeys via native hook (S11); Track 0 doc (S07-004).
  Docs: `docs/migration/04`, `docs/adr/ADR-001`.
- **S06-005:** Color Picker HEX + eyedropper — `isValidHex()` в AddIn (mirror `ThemeColor.IsValidHex`);
  `ColorPickerPanel` Custom HEX input (live preview, Enter/Set, inline error); **pick from shape** (path A) —
  `readColorFromSelection(fill|line|text)` читает цвет первой выделенной фигуры; **screen pick** (path B, bonus) —
  Browser `EyeDropper` API с feature-detect (WebView2/Chromium); unsupported → disabled + hint; native PP eyedropper
  out of scope; picked/typed → `recordRecentColor()`; без новых CommandIds / UserSettings persist. 143 dotnet tests.
- **S06-004:** Object Statistics display mode — `UserSettings.addupDisplayMode` (`all`|`sum`|`min`|`max`|`average`,
  default `all`); Core `AddupStatusFormatter` + TS mirror `addupStatus.ts`; Settings dropdown «Object statistics display»;
  `AddupTextFields` + hotkey Alt+A format status by mode; `all` preserves legacy string; export/import JSON v1;
  invalid mode → `all` + warning; optional «Last addup result» in Text section (session, not persist). 143 dotnet tests.
- **S06-003:** Import/export settings JSON — `UserSettingsImporter` (Core) validates file against `CommandCatalog`;
  unknown `commandId` → skip + warning; duplicate keys last wins; `POST /api/settings/import` validate-only (no persist);
  Settings Export (editor state + `schemaVersion: 1`) / Import → editor; MessageBar «Imported — click Save to persist»;
  hotkeys via `syncKeyboardShortcuts` only on Save (not import preview); VSTO on-disk JSON shape preserved.
- **S06-002:** `replaceShortcuts` ↔ UserSettings — 76 hotkey-eligible CommandIds (79 − 3 Settings);
  `shortcuts.json` declares 76 `actions[]`; Tier 1 defaults (14) in `shortcuts[]` unchanged; 62 без default key;
  `syncKeyboardShortcuts()` → `bindingsToOfficeMap` + `Office.actions.replaceShortcuts` (feature-gated 1.1);
  sync on bootstrap, Save, Reset, App settings update; McKinsey/BCG profile → Save → live hotkeys;
  optional `areShortcutsInUse` warning in Shortcut Manager; `toOfficeShortcutKey()` for Office format;
  empty keys → null; duplicate keys last wins. Web/Mac — no-op без crash.
- **S06-001:** Tier 1 global hotkeys — Shared Runtime (`lifetime long`, taskpane.html unified bootstrap);
  manifest `SharedRuntime 1.1` + `KeyboardShortcuts 1.1` + `ExtendedOverrides` → `shortcuts.json` (14 actions:
  catalog `DefaultShortcut` Alt+1…Alt+8, Alt+B/H/G, F1 + McKinsey Alt+D/A); `actionId === CommandId`;
  `registerCommandActions()` only when `KeyboardShortcuts 1.1` supported; `executeCommandById` → `runCommand`.
  Settings commands excluded; `replaceShortcuts` / all 79 ids — S06-002. Web/Mac — task pane unchanged.
- **S05-005:** Smart Duplicate gap memory — per-`CommandId` in-memory state в task pane (`duplicateGapMemory.ts`);
  первый duplicate в направлении gap=0, повторный — remembered gap; `DuplicationEngine.InferGap` в Core (inverse
  of `ComputeDuplicate`); status bar «(gap X pt)» при gap>0. Без localStorage/UserSettings; каталог 79 команд без изменений.
- **S05-004:** `PasteShapeToSelectedSlides` / `RemoveShapeFromSelectedSlides` (78–79-я команды) — HostScript:
  `pasteShapeToSelectedSlides()` — ≥2 slides, 1 source shape, skip source slide, same geometry;
  `cloneShapeOnSlide(..., crossSlide=true)` forces recreate path on target slide (no copyTo/duplicate cross-slide);
  `removeShapeFromSelectedSlides()` — delete by exact `shape.name` on each selected slide, aggregates
  `{ slidesProcessed, shapesRemoved }`. Catalog: Partial, HostScript, **Objects**, defaultShortcut null.
- **S05-003:** `MoveSlidesToBackup` (77-я команда) — HostScript `moveSelectedSlidesToBackup()`:
  `slide.moveTo` (Api 1.8+) preferred; fallback export → insert at end → delete; multi-select descending
  by index; no slide sections on Web. Catalog: Partial, HostScript, Slides, defaultShortcut null.
- **S05-002:** `GridSnap` (Core) — 0.1 cm grid in points; post-process in `LayoutEngine.Apply` when
  `LayoutOptions.SnapToGrid`; `UserSettings.snapToGrid` persist; AddIn checkbox + pass `options.snapToGrid`
  per layout request (stateless API). Без snap на клиенте; без новых CommandIds.
- **S05-001:** `ConsultingProfilePresets` (Core) — McKinsey/BCG shortcut presets; `GET /api/settings/profile-presets`;
  Settings dropdown: McKinsey/BCG заменяют shortcuts в editor (warning + Save); Custom только меняет label.
  Без новых CommandIds; 93 dotnet tests.
- **S04-003:** Recent colors (max 5) persist in browser `localStorage` key `ppt-powerkeys-recent-colors`
  (per device, no Api/UserSettings round-trip — VSTO parity for session reload). `loadPersistedRecentColors()`
  in `bootstrapThemeColors()`; `recordRecentColor` saves; `resetFormatColorState` clears for tests.
  Fill/Line/Text cycle commands and `ColorPickerPanel` share the same recent list. Picker `reload` refreshes
  theme via `readPresentationThemeColors()`; MessageBar when `source === 'fallback'` on Web.
- **S04-002:** `ColorPickerPanel.tsx` — theme/recent swatches, Apply Fill/Line/Text; `OpenColorScheme` scrolls to picker in Settings.
- **S04-001:** `ColorPaletteBuilder` (Core) merge theme≤10 + recent≤5; Api `POST /api/colors/build-palette`;
  AddIn `themeColors.ts` reads slide master `themeColorScheme` (PowerPointApi 1.10), silent fallback на
  `DEFAULT_PALETTE` на Web; `bootstrapThemeColors()` on `Office.onReady`.
- **S03-003:** `ShortcutManager.tsx` — editable bindings (title из каталога, edit keys, add/remove);
  duplicate-key warning non-blocking; `ShortcutBindingValidator` в Core + 6 unit-тестов; Save через существующий API.
- **S03-002:** Settings panel в AddIn (`SettingsPanel.tsx`); `getUserId()` → `localStorage` + header `X-User-Id`;
  `resetSettings()`; Settings-команды wired (`OpenShortcutManager` scroll, `ResetToDefaults` API reset,
  `OpenColorScheme` stub Sprint 04); UI hint — Office Web не перехватывает global hotkeys как VSTO.
- **S03-001:** `IUserSettingsStore` в Core; `FileUserSettingsStore` в Api — JSON per user под `SETTINGS_DATA_PATH`
  (default `/data/settings`), atomic write, Docker volume `settings_data` на VDS.
- **S02-006:** Единый реестр `unsupportedWebCommands.ts` для 9 None-команд; `CommandOutcome.kind`
  (`success` | `unsupported` | `error`); легенда бейджей и warning status bar вместо красного Error для ожидаемой деградации.
- **S02-005:** Slides HostScript — `CopySlide` через `exportAsBase64` + `insertSlidesFromBase64` (Partial);
  view/zoom/sorter/slideshow/grid/guides/print — явная деградация с конкретными сообщениями (None).
- **S02-004:** Edge-align (`AlignLeftToRight` и др.) — математика в `LayoutEngine`, execution `ServerLayout`;
  CopyAndAlign — HostScript: клон на позиции источника + `api.applyLayout` с явным `anchorIndex` (последняя исходная).
- **S02-003:** Text HostScript — `PasteUnformatted` через `navigator.clipboard.readText()` (Partial, user gesture);
  `ReplaceWithEllipsis` → `"..."`; superscript/subscript toggle с взаимоисключением; `PasteFormatted` — явная деградация (None).
- **S01-008:** dev/prod манифесты разделены; production URL подставляются при сборке (`ADDIN_BASE_URL`, `API_BASE_URL`).
  Для PowerPoint on the web достаточно `DesktopFormFactor` + публичные HTTPS URL (отдельный `WebFormFactor` в add-in only manifest не требуется).
- **S01-009:** prod-манифесту дан отдельный `<Id>` (`5b0ca36f-...`), отличный от dev (`92d7d44c-...`) — Office Online
  кэширует надстройку по `<Id>`, одинаковый GUID приводил к отдаче закэшированного localhost-SourceLocation.
  prod `DisplayName` = «PptPowerKeys (Web)».
- **S01-010:** prod-манифест `manifest.prod.xml` **закоммичен** в репозиторий (убран из `.gitignore`) — пользователь
  устанавливает надстройку через `git pull` + upload файла из клона, и prod-манифест должен быть доступен напрямую.
  CI-шаг (`ci.yml`) защищает закоммиченный файл от дрейфа с шаблоном.
- **Хостинг (2026-06-27):** статика add-in — **GitHub Pages** (`https://alexb0nch.github.io/ASN-PP-PowerKeys/`);
  **API — собственный VDS** `https://95.140.152.103.sslip.io` (Docker Compose: Caddy auto-HTTPS + Kestrel-контейнер),
  деплой по SSH через `.github/workflows/deploy-vds.yml` (секреты `VDS_*` в GitHub Actions). Хост в одной
  переменной `API_PUBLIC_HOST` (sslip.io → легко сменить на свой домен). Azure-путь (S01-011) остаётся как опция,
  но не используется. Реальный деплой подтверждён в S01-012.

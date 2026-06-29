# Sprint 06 — Retrospective

> **Статус:** завершён (2026-06-29). Все задачи Done (S06-001…005).

## Итоги

| ID | Issue | PR | Результат |
|----|-------|-----|-----------|
| S06-001 | #44 | #46 | Shared Runtime + Tier 1 keyboard shortcuts (`shortcuts.json`, 14 defaults) |
| S06-002 | #48 | #49 | `replaceShortcuts` ↔ UserSettings (76 hotkey-eligible actions) |
| S06-003 | #51 | #52 | Import/export settings JSON (`UserSettingsImporter`, validate-only API) |
| S06-004 | #54 | #55 | Object Statistics MIN/MAX/AVG UI (`addupDisplayMode`, `AddupStatusFormatter`) |
| S06-005 | #56 | #57 | Color Picker HEX input + eyedropper (pick from shape + optional Screen pick) |

## Definition of Done спринта — выполнено

- [x] **S06-001** — Shared Runtime + Tier 1 defaults
- [x] **S06-002** — Save/load Shortcut Manager → `Office.actions.replaceShortcuts`
- [x] **S06-003** — Import/export settings JSON
- [x] **S06-004** — Object Statistics MIN/MAX/AVG UI
- [x] **S06-005** — Color Picker eyedropper / HEX (stretch из Sprint 04)
- [x] Трассировка `S06-0YY` → Issue → PR → merge
- [x] `dotnet test PptPowerKeys.sln` — зелёный (143)
- [x] AddIn: `typecheck`, `validate:prod`, `build:prod` — зелёные
- [x] `docs/PRODUCT_CONTEXT.md` + `03-powerpoint-desktop-windows.md` обновлены

## Ключевые решения

- **Shared Runtime (S06-001):** `lifetime long`, unified `taskpane.html` bootstrap; manifest `SharedRuntime 1.1` +
  `KeyboardShortcuts 1.1` + `ExtendedOverrides` → `shortcuts.json`; `actionId === CommandId`; Settings commands excluded.
- **replaceShortcuts (S06-002):** 76 hotkey-eligible CommandIds; sync on bootstrap, Save, Reset; McKinsey/BCG profile →
  live hotkeys; Web/Mac — no-op без crash.
- **Import/export (S06-003):** `UserSettingsImporter` validates against `CommandCatalog`; import validate-only API;
  hotkeys sync only on Save.
- **Addup display mode (S06-004):** `UserSettings.addupDisplayMode`; Core `AddupStatusFormatter`; `all` preserves legacy string.
- **Color Picker eyedropper (S06-005):** Office.js не даёт native slide eyedropper — pragmatic scope:
  - **Path A (required):** `readColorFromSelection(fill|line|text)` — pick from first selected shape
  - **Path B (bonus):** Browser `EyeDropper` API с feature-detect (WebView2/Chromium)
  - **HEX input:** `isValidHex()` mirror Core; live preview; invalid → MessageBar
  - Без новых CommandIds; recent остаётся `localStorage`

## Метрики

- `dotnet test`: **143** passed (без изменений в S06-005)
- **79** команд в каталоге (без изменений)
- AddIn: `typecheck`, `validate:prod`, `build:prod` — зелёные

## Anti-scope (соблюдено)

- VSTO legacy / COM keyboard hook
- Новые CommandIds
- Snap-to-nearest-object, slide sections
- RGB sliders / HSL picker
- Persist custom colors в UserSettings/Api
- Canvas pixel sampling

## Следующий спринт

**TBD** — кандидаты из README backlog:

- Snap-to-nearest-object при drag
- Slide Backup: named section + hide/show (нет Office.js API)
- Разрезание объектов (VSTO parity)

См. `sprints/` — architect определит Sprint 07 при планировании.

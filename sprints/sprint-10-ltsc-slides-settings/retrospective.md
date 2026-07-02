# Sprint 10 — Retrospective

> **Статус:** завершён (2026-07-02). Все задачи Done (S10-001…005). **M3 Feature beta** достигнут.

## Итоги

| ID | Issue / PR | Результат |
|----|------------|-----------|
| S10-001 | #93 | Slide HostScript: CopySlide, MoveSlidesToBackup |
| S10-002 | #95 | View/print None unlocks (6 commands) |
| S10-003 | #97 | FormatPainter, PasteFormatted, Regroup |
| S10-004 | #99 / #100 | Settings WPF pane + UserSettings (3 Settings commands) |
| S10-005 | #103 / #104 | Color picker COM panel + `FormatColorPaletteProvider` |

## Definition of Done спринта — выполнено

- [x] S10-001…005 Done
- [x] **79/79** commands routed on Windows line
- [x] **9/9** OfficeJs None unlocks complete
- [x] Settings UI: profiles, shortcuts, import/export, addupDisplayMode
- [x] Color picker COM panel (theme + recent + apply + pick from shape + HEX)
- [x] Settings JSON compatible with Web export/import v1
- [x] `dotnet test PptPowerKeys.sln` — **319 passed** (Linux CI)
- [x] `retrospective.md` при закрытии

## Ключевые решения

- **Slides wave:** `CopySlide` (COM `Slide.Duplicate`), `MoveSlidesToBackup` (`Slide.MoveTo` high-index first).
- **None unlocks:** view/print (6) + format/objects/text (3) — все через COM без Office.js.
- **Settings pane:** WPF `SettingsPane` + `WindowsUserSettingsStore` Save/Reset/Import; ribbon `grpSettings`.
- **Color picker:** `ColorPickerPane` parity Web `ColorPickerPanel.tsx`; `FormatColorPaletteProvider` shared с cycle commands; `ReadColorFromSelection` на COM.
- **Anti-scope соблюдён:** screen eyedropper, global hotkeys → S11.

## Метрики

- `dotnet test`: **319** passed (+49 vs Sprint 09 close)
- Windows `CommandRouter` commands: **79** (32 ServerLayout + 47 HostScript)
- Manual QA: Windows + VS sideload required (outside Linux CI)

## Риски / долг

- Global hotkeys (`ShortcutManager`) → S11.
- MSI / code signing / QA matrix → S11.
- Color picker manual QA только на Windows LTSC.

## Следующий спринт

**S11 — LTSC Ship (Hotkeys · MSI · QA)** — production-ready `PptPowerKeys.Windows`.  
Kickoff: [`../sprint-11-ltsc-ship/ARCHITECT-KICKOFF.md`](../sprint-11-ltsc-ship/ARCHITECT-KICKOFF.md)  
Первая задача: **S11-001** Native ShortcutManager.

# Sprint 09 — Retrospective

> **Статус:** завершён (2026-07-01). Все задачи Done (S09-001…006). **M3 Objects/Format/Text beta** достигнут.

## Итоги

| ID | Issue / PR | Результат |
|----|------------|-----------|
| S09-001 | #73 / #74 | Insert shapes HostScript (6 commands) |
| S09-002 | #75 / #76 | Smart Duplicate + `DuplicationEngine` gap memory (4 commands) |
| S09-003 | #78 / #79 | Group / Ungroup / Z-order (6 commands) |
| S09-004 | #82 / #83 | Multi-slide paste/remove shapes (2 commands) |
| S09-005 | #86 / #87 | Format colors + palette + recent colors (4 commands) |
| S09-006 | #90 / #91 | Text + Addup (5 commands) |

## Definition of Done спринта — выполнено

- [x] S09-001…006 Done
- [x] **27** HostScript команд Objects/Format/Text routed (3 unlock → S10: Regroup, FormatPainter, PasteFormatted)
- [x] Color picker COM foundation: theme from Slide Master + `RecentColors` in `UserSettings.json` (S09-005)
- [x] `dotnet test PptPowerKeys.sln` — **270 passed** (Linux CI)
- [x] `retrospective.md` при закрытии

## Ключевые решения

- **HostScript wave 2:** COM read/write + Core helpers (`DuplicationEngine`, `ColorPaletteBuilder`, `NumberAggregator`, `AddupStatusFormatter`).
- **Ribbon groups:** Objects, Duplicate, Order, Multi-slide, Format, Text — все через `OnHostScriptCommand` + `HostScriptCommandMap`.
- **Format cycle:** `FormatColorCycleStore` fingerprint per selection; recent colors FIFO max 5.
- **Text Addup:** in-process Core (no HTTP); `AddupDisplayMode` from `WindowsUserSettingsStore`.
- **3 deferred unlocks:** Regroup, FormatPainter, PasteFormatted → Sprint 10.

## Метрики

- `dotnet test`: **270** passed (+101 vs Sprint 08 close)
- Windows `CommandRouter` commands: **65** (32 ServerLayout + 33 HostScript)
- Manual QA: Windows + VS sideload required (outside Linux CI)

## Риски / долг

- Clipboard paste (`PasteUnformatted`) requires STA + user gesture on Desktop.
- Full color picker panel + Settings UI → S10.
- Global hotkeys / MSI ship deferred to S11.

## Следующий спринт

**S10 — LTSC Slides · Settings · Web-None unlock** — Slides (8) + Settings UI + 9 None unlocks.  
Kickoff: [`../sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md`](../sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md)  
Первая задача: **S10-001** Slide HostScript (`CopySlide`, `MoveSlidesToBackup`).

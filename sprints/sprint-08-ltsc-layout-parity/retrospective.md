# Sprint 08 — Retrospective

> **Статус:** завершён (2026-06-30). Все задачи Done (S08-001…005). **M2 Layout beta** достигнут.

## Итоги

| ID | PR | Результат |
|----|-----|-----------|
| S08-001 | #63 | CommandRouter: all 32 ServerLayout commands via `LayoutEngine.IsLayoutCommand` |
| S08-002 | #65 | Snap-to-grid via `WindowsUserSettingsStore` + ribbon checkbox |
| S08-003 | #67 | Ribbon layout groups (32 ServerLayout buttons, 6 groups) |
| S08-004 | #69 | Copy-and-align HostScript (4 commands) |
| S08-005 | #71 | Position clipboard (Copy/Paste) + consolidated layout QA doc |

## Definition of Done спринта — выполнено

- [x] S08-001…005 Done
- [x] `dotnet test PptPowerKeys.sln` — **169 passed** (Linux CI)
- [x] Manual QA matrix: [`docs/migration/06-windows-layout-qa.md`](../../docs/migration/06-windows-layout-qa.md)
- [x] `retrospective.md` при закрытии

## Ключевые решения

- **ServerLayout pipeline:** COM → `ShapeBounds[]` → `LayoutEngine.Apply` → `ApplyShapeBounds`; anchor = last selected.
- **Snap-to-grid:** `%AppData%/PptPowerKeys/UserSettings.json`; Web export/import v1 compatible.
- **HostScript wave 1 (layout):** CopyAndAlign (duplicate + layout) and Position clipboard (in-memory Left/Top).
- **Ribbon architecture:** `OnLayoutCommand` (32) vs `OnHostScriptCommand` (6 layout extras); `HostScriptCommandMap` separate from `RibbonCommandMap`.
- **QA consolidation:** per-task README fragments trimmed; single M2 matrix in `06-windows-layout-qa.md`.

## Метрики

- `dotnet test`: **169** passed (+26 vs Sprint 07 close)
- Windows HostScript commands routed: **38** (32 ServerLayout + 4 CopyAndAlign + 2 position)
- Manual QA: Windows + VS sideload required (outside Linux CI)

## Риски / долг

- Windows/VSTO build and live PowerPoint QA remain Windows-only.
- Position clipboard is session-scoped (matches Web); no cross-session persist by design.
- Global hotkeys / MSI ship deferred to S11.

## Следующий спринт

**S09 — LTSC Objects · Format · Text** — HostScript wave 2 (30 commands).  
Kickoff: [`../sprint-09-ltsc-objects-format-text/goals.md`](../sprint-09-ltsc-objects-format-text/goals.md)

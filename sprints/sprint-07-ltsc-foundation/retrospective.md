# Sprint 07 — Retrospective

> **Статус:** завершён (2026-06-29). Все задачи Done (S07-001…004). **M1 Technical prototype** достигнут.

## Итоги

| ID | PR | Результат |
|----|-----|-----------|
| S07-001 | #59 | Core multitarget `net8.0` + `netstandard2.0`; polyfills; 143 tests green |
| S07-002 | #60 | `PptPowerKeys.Windows` VSTO shell + `PptPowerKeys.Windows.sln` |
| S07-003 | #61 | `ComHostAdapter` + `CommandRouter`; AlignLeft e2e in-process (no HTTP) |
| S07-004 | — | Track 0 LTSC deploy checklist (`05-ltsc-web-addin-central-deploy.md`) |

## Definition of Done спринта — выполнено

- [x] S07-001…004 Done
- [x] `dotnet test PptPowerKeys.sln` — **143 passed** (Linux CI)
- [x] `PptPowerKeys.Windows.sln` scaffolded; AlignLeft POC + manual runbook in README
- [x] `docs/PRODUCT_CONTEXT.md` — dual product line + Track 0 link
- [x] `VstoLegacy/FROZEN.md` — указывает на `PptPowerKeys.Windows`
- [x] ADR-001 cross-links в Core csproj

## Ключевые решения

- **Core multitarget:** `netstandard2.0` для VSTO .NET 4.8; `GeneratedRegex` заменён на `Regex.Compiled` для совместимости; polyfills для `init`/`required`.
- **Новый host:** `src/PptPowerKeys.Windows/` — **не** размораживание `VstoLegacy*`.
- **ShapeBounds boundary:** COM `Shape.Id` → string id; selection order preserved; anchor = last (Core convention).
- **CommandRouter:** ServerLayout path через `LayoutEngine.Apply` in-process; расширение в S08.
- **Track 0:** отдельный runbook для IT без Upload UI; decision tree Web vs native.

## Метрики

- `dotnet test`: **143** passed (без регрессии)
- Windows solution: **не в root CI** (by design)
- Команд в Core catalog: **79** (unchanged)

## Риски / долг

- Windows/VSTO сборка и AlignLeft manual QA — только на Windows + VS (вне Linux CI).
- `ComHostAdapter` — только AlignLeft в router; S08 добавит остальные ServerLayout.
- netstandard2.0 build warnings (nullable) — некритично, можно почистить позже.

## Следующий спринт

**S08 — LTSC Layout parity** — 32 ServerLayout + copy-and-align + position clipboard.  
Kickoff: [`../sprint-08-ltsc-layout-parity/ARCHITECT-KICKOFF.md`](../sprint-08-ltsc-layout-parity/ARCHITECT-KICKOFF.md)

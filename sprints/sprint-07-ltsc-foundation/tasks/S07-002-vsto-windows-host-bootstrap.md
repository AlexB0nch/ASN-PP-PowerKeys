# S07-002 — PptPowerKeys.Windows VSTO bootstrap

> Передача builder'u: `/builder выполни S07-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S07-002` |
| **Спринт** | `sprint-07-ltsc-foundation` |
| **Компонент** | Windows (new VSTO project) |
| **Статус** | Done |

## Цель

Создать **`src/PptPowerKeys.Windows/`** — новый VSTO add-in project (не `VstoLegacy*`), solution
`PptPowerKeys.Windows.sln`, минимальный Ribbon + ThisAddIn startup.

## Контекст

- `VstoLegacy` — frozen scaffold; RibbonTab.xml можно **копировать/адаптировать** как reference.
- S07-001: Core netstandard2.0 reference готов.

## Scope

| Item | Detail |
|------|--------|
| Project | VSTO PowerPoint Add-in .NET Framework 4.8 |
| Solution | `PptPowerKeys.Windows.sln` (отдельный от root sln) |
| Reference | `PptPowerKeys.Core` (netstandard2.0) |
| Ribbon | Minimal tab «PowerKeys» + 1 button stub → debug message |
| Startup | ThisAddIn loads without exception |
| Docs | README snippet in project or `docs/migration/04` § dev setup Windows |

## Анти-scope

- CommandRouter full implementation (S07-003)
- MSI installer (S11)
- Editing files under `VstoLegacy/` (read-only reference)
- Web Add-in changes

## Критерии приёмки

- [x] Solution builds on Windows + VS + VSTO workload (document in PR manual note)
- [x] Add-in loads in PowerPoint; ribbon button visible
- [x] Project references Core netstandard2.0
- [x] `dotnet test PptPowerKeys.sln` still green (unaffected)
- [x] FROZEN.md / ADR clarify new project vs legacy

## Зависимости

- S07-001 Done (Core multitarget)

## Трассировка

Issue → branch `cursor/S07-002-vsto-windows-host-bootstrap-*` → PR

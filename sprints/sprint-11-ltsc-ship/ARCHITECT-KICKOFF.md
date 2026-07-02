# Architect Kickoff — Sprint 11 LTSC Ship (Hotkeys · MSI · QA)

> **Статус:** Planned. Первая задача: **S11-001**.

## Контекст

- Sprint 10 **Done** — **M3 Feature beta**: 79/79 commands routed; Settings UI + color picker COM.
- Linux CI green (`dotnet test` 319 passed at S10 close).
- **M4 Production** — цель спринта: signed MSI, global hotkeys, QA matrix, LTSC runbook.

## Цель Sprint 11

Production-ready **PptPowerKeys.Windows** для IT deployment.

| Wave | Тема | Задачи |
|------|------|--------|
| Hotkeys | Native `ShortcutManager` (76 cmd) | S11-001 |
| Profiles | McKinsey/BCG → live bindings | S11-002 |
| Installer | MSI/ClickOnce + code signing | S11-003 |
| Companion | Optional updater/diagnostics | S11-004 |
| Release | QA matrix + LTSC runbook | S11-005 |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S11-001 | TBD | `/builder выполни S11-001` |
| S11-002 | TBD | — |
| S11-003 | TBD | — |
| S11-004 | TBD | — |
| S11-005 | TBD | — |

## Инварианты

- `dotnet test PptPowerKeys.sln` must stay green.
- Windows/VSTO build — Windows + VS (вне Linux CI).
- VstoLegacy — только ribbon reference.

## Процесс сессии

1. Issue S11-001 → backlog **In Progress**
2. `/builder выполни S11-001`
3. Приёмка PR → merge → Done
4. Повторить для S11-002…005
5. `retrospective.md` → goals DoD → `PRODUCT_CONTEXT` → epic close

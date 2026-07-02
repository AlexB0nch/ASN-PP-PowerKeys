# S11-005 — QA matrix + release (закрытие Sprint 11)

> Передача builder'у: `/builder выполни S11-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S11-005` |
| **Спринт** | `sprint-11-ltsc-ship` |
| **Компонент** | docs + QA |
| **Статус** | Todo |
| **Зависимость** | S11-001…003 Done; S11-004 optional |

## Цель

Consolidated **QA matrix** + **release runbook** для Product Line B v1.0: Office 2019 / 2021 LTSC / 2024 LTSC, hotkeys smoke, layout/objects/settings/color picker, MSI install path.

## Scope

| Deliverable | Описание |
|-------------|----------|
| `docs/migration/08-windows-ltsc-qa-matrix.md` | Command categories × Office versions × pass/fail |
| `src/PptPowerKeys.Windows/README.md` | Link matrix; release checklist |
| `docs/migration/07-windows-ltsc-deploy-msi.md` | Cross-link IT pack (from S11-003) |
| Release notes | `CHANGELOG` or sprint retrospective section |
| IT deployment pack | Single doc: MSI + Group Policy hints + Track 0 Web fallback |

## QA matrix minimum rows

| Area | Smoke tests |
|------|-------------|
| Install | MSI silent + manual |
| Layout | AlignLeft, SameWidth, snap |
| Objects | DuplicateRight, Group |
| None unlock | FormatPainter, ToggleZoom |
| Settings | Save profile, import JSON |
| Hotkeys | Alt+1, profile-specific key |
| Color picker | Apply fill, recent colors |

## Критерии приёмки

- [ ] Matrix doc exists with ≥3 Office version columns
- [ ] Release runbook: tag, build MSI, sign, smoke on VM
- [ ] `goals.md` DoD items addressable
- [ ] Architect post-merge: retrospective, PRODUCT_CONTEXT M4, epic ROADMAP complete

## Architect post-merge (не builder)

| # | Действие |
|---|----------|
| 1 | Backlog S11-005 → Done; Issues closed |
| 2 | `retrospective.md` |
| 3 | `goals.md` DoD `[x]` |
| 4 | `PRODUCT_CONTEXT.md` — Line B v1.0, M4 Production |
| 5 | `sprints/README.md` Sprint 11 Done |
| 6 | `epic-ltsc-windows-native/ROADMAP.md` — M4 complete |
| 7 | Git tag `windows-v1.0` (or agreed) |

## Reference

- `docs/migration/06-windows-layout-qa.md` (S08 matrix pattern)
- `FEATURE_PARITY.md` — 79 commands checklist

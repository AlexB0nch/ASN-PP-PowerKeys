# S11-004 — Companion updater / diagnostics (optional)

> Передача builder'у: `/builder выполни S11-004` (architect may **skip** if time-boxed)

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S11-004` |
| **Спринт** | `sprint-11-ltsc-ship` |
| **Компонент** | Windows (optional tray app) |
| **Статус** | Todo (optional) |
| **Зависимость** | S11-003 (version from MSI) |

## Цель

Optional **Companion** exe (ADR-001 Variant D): check for updates, show diagnostics (PP version, add-in version, settings path), optional license placeholder.

## Scope (minimal viable)

- Small WPF/WinForms tray or `--diagnostics` console flag on installer
- Read version from assembly; log file path `%AppData%/PptPowerKeys/`
- Update check URL placeholder (no server required for v1)

## Критерии приёмки

- [ ] Documented as optional in README
- [ ] If implemented: launches without admin; does not break add-in load
- [ ] Architect may mark **Cancelled** with rationale in backlog

## Анти-scope

- Full auto-update server; license enforcement

## Reference

- `docs/adr/ADR-001-ltsc-windows-native-line.md` — Companion optional

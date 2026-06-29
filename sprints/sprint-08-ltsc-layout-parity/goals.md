# Sprint 08 — LTSC Layout Parity

> Epic: [`../epic-ltsc-windows-native/ROADMAP.md`](../epic-ltsc-windows-native/ROADMAP.md)  
> Архитектура: [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)

## Цель спринта

**M2 Layout beta:** все **32 ServerLayout** команды через `CommandRouter` + in-process Core;
snap-to-grid; copy-and-align и position clipboard (по parity matrix).

## Scope (черновик backlog)

| ID | Задача | Компонент |
|----|--------|-----------|
| S08-001 | CommandRouter: all ServerLayout commands (32) | Windows + Core |
| S08-002 | Snap-to-grid via `LayoutOptions` / user settings | Windows + Core |
| S08-003 | Ribbon layout group (align / distribute / resize) | Windows UI |
| S08-004 | Copy-and-align HostScript wave (4 cmds) | Windows |
| S08-005 | Position clipboard + manual QA matrix note | Windows + docs |

## Anti-scope

- Objects / Format / Text HostScript (→ S09)
- Slides + 9 None unlock (→ S10)
- MSI / global hotkeys (→ S11)
- Web Add-in changes (кроме docs)

## Definition of Done спринта

- [ ] S08-001…005 Done
- [ ] `dotnet test PptPowerKeys.sln` зелёный
- [ ] Manual: representative ServerLayout commands on Windows PP
- [ ] `retrospective.md` при закрытии

## Зависимости

- Sprint 07 Done (M1: ComHost + AlignLeft POC)

## Предыдущий спринт

[`sprint-07-ltsc-foundation/retrospective.md`](../sprint-07-ltsc-foundation/retrospective.md)

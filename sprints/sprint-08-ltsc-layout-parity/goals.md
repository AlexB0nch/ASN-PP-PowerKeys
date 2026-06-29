# Sprint 08 — LTSC Layout Parity

> Epic: [`../epic-ltsc-windows-native/ROADMAP.md`](../epic-ltsc-windows-native/ROADMAP.md)  
> **Depends on:** Sprint 07 Done (ComHost + CommandRouter shell)

## Цель

Полный **layout parity**: 32 ServerLayout commands + copy-and-align (4) + copy/paste position (2) + snap-to-grid.

## Planned tasks (architect декомпозирует при kickoff)

| ID | Тема |
|----|------|
| S08-001 | CommandRouter ServerLayout pipeline (32 cmd) |
| S08-002 | CopyAndAlign* (4 Partial → COM clone + Core) |
| S08-003 | Copy/Paste object position |
| S08-004 | Snap-to-grid via LayoutOptions |
| S08-005 | Layout integration tests + manual matrix note |

## Definition of Done

- [ ] All Alignment (18) + Resize (20) commands work via COM host
- [ ] Parity with Web Add-in layout behavior (anchor = last selected)
- [ ] Core tests unchanged/green; Windows manual QA doc

## Anti-scope

- Objects/Format/Text (S09)
- Global hotkeys (S11)

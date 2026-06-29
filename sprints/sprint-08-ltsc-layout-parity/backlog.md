# Sprint 08 — Backlog

> Epic LTSC Windows Native · Architect kickoff: [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md)

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S08-001 | CommandRouter: all 32 ServerLayout commands | Windows + Core | **Todo** | — |
| S08-002 | Snap-to-grid (LayoutOptions + settings hook) | Windows + Core | **Todo** | — |
| S08-003 | Ribbon layout command group | Windows UI | **Todo** | — |
| S08-004 | Copy-and-align HostScript (4 commands) | Windows | **Todo** | — |
| S08-005 | Position clipboard + layout QA notes | Windows + docs | **Todo** | — |

## Порядок исполнения

1. **S08-001** — расширяет S07-003 `CommandRouter` на все ServerLayout
2. **S08-002** — snap flag из settings (совместимость с Web `UserSettings.SnapToGrid`)
3. **S08-003** — ribbon UX (может параллельно после 001)
4. **S08-004** — первая HostScript волна (copy-and-align)
5. **S08-005** — position clipboard + manual matrix

## Ссылка на ServerLayout список

См. `CommandCatalog` entries с `ExecutionKind.ServerLayout` (32 cmds) — parity matrix в
[`epic-ltsc-windows-native/FEATURE_PARITY.md`](../epic-ltsc-windows-native/FEATURE_PARITY.md).

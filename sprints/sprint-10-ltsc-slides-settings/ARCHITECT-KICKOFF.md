# Architect Kickoff — Sprint 10 LTSC Slides · Settings · Web-None unlock

> **Статус:** In Progress. Следующая задача: **S10-003**.

## Контекст

- Sprint 09 **Done** — M3 Objects/Format/Text beta: 27 HostScript + 32 ServerLayout = 65 commands routed.
- **S10-001 Done** (PR #93) — +2 Slide HostScript → **67 commands routed**.
- **S10-002 Done** (PR #95) — +6 view/print None unlocks → **73 commands routed**.
- `CommandRouter` + `ComHostAdapter` + `HostScriptCommandMap` готовы к wave 4.
- Linux CI green (`dotnet test` 289 passed at S10-002 close).

## Цель Sprint 10

**M3 Feature beta (продолжение):** Slides (8) + **9 OfficeJs None unlocks** + **Settings UI** + color picker panel COM.

| Wave | Команды | Задачи |
|------|---------|--------|
| Slides COM | CopySlide, MoveSlidesToBackup | S10-001 ✅ |
| None unlock view/print | Zoom, Sorter, SlideShow, Grid, Guides, Print | S10-002 ✅ |
| None unlock format/objects/text | FormatPainter, PasteFormatted, Regroup | S10-003 |
| Settings WPF pane | profiles, shortcuts, import/export, addupDisplayMode | S10-004 |
| Color picker + profiles | COM theme panel + consulting presets | S10-005 |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S10-001 | [`tasks/S10-001-slide-commands.md`](./tasks/S10-001-slide-commands.md) | Done (#93) |
| S10-002 | [`tasks/S10-002-view-print-none.md`](./tasks/S10-002-view-print-none.md) | Done (#95) |
| S10-003 | [`tasks/S10-003-format-regroup-none.md`](./tasks/S10-003-format-regroup-none.md) | `/builder выполни S10-003` |
| S10-004 | `tasks/S10-004-settings-pane.md` (создать) | после 003 |
| S10-005 | `tasks/S10-005-color-picker-profiles.md` (создать) | после 004 |

## Инварианты

- Web spec (`powerpoint.ts`, `runCommand.ts`) — эталон HostScript.
- `dotnet test PptPowerKeys.sln` must stay green.
- `UserSettings.json` camelCase — Web export/import v1 compatible.
- VstoLegacy — только ribbon reference.

## Процесс сессии

1. Issue S10-003 → backlog **In Progress**
2. `/builder выполни S10-003`
3. Приёмка PR → merge → Done
4. Повторить для S10-004…005
5. `retrospective.md` → goals DoD → `PRODUCT_CONTEXT` → kickoff S11

## Copy-paste промпт (S10-003)

```
/architect

Sprint 10 — S10-003 None unlock format/objects/text (3 COM commands).
S10-002 Done (#95 merged). Прочитай:
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-003-format-regroup-none.md
- sprints/epic-ltsc-windows-native/FEATURE_PARITY.md (Format/Text/Objects None unlocks)
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (btnFormatPainter)
- src/PptPowerKeys.Windows/Host/CommandRouter.cs

Issue S10-003 → backlog In Progress → /builder выполни S10-003 → приёмка → merge.
```

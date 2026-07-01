# Architect Kickoff — Sprint 10 LTSC Slides · Settings · Web-None unlock

> **Статус:** Ready to start. Первая задача: **S10-001**.

## Контекст

- Sprint 09 **Done** — M3 Objects/Format/Text beta: 27 HostScript + 32 ServerLayout = 65 commands routed.
- `CommandRouter` + `ComHostAdapter` + `HostScriptCommandMap` готовы к wave 3.
- Linux CI green (`dotnet test` 270 passed at S09 close).

## Цель Sprint 10

**M3 Feature beta (продолжение):** Slides (8) + **9 OfficeJs None unlocks** + **Settings UI** + color picker panel COM.

| Wave | Команды | Задачи |
|------|---------|--------|
| Slides COM | CopySlide, MoveSlidesToBackup | S10-001 |
| None unlock view/print | Zoom, Sorter, SlideShow, Grid, Guides, Print | S10-002 |
| None unlock format/objects/text | FormatPainter, PasteFormatted, Regroup | S10-003 |
| Settings WPF pane | profiles, shortcuts, import/export, addupDisplayMode | S10-004 |
| Color picker + profiles | COM theme panel + consulting presets | S10-005 |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S10-001 | [`tasks/S10-001-slide-commands.md`](./tasks/S10-001-slide-commands.md) | `/builder выполни S10-001` |
| S10-002 | `tasks/S10-002-view-print-none.md` (создать при старте) | после 001 |
| S10-003 | `tasks/S10-003-format-regroup-none.md` (создать) | после 002 |
| S10-004 | `tasks/S10-004-settings-pane.md` (создать) | после 003 |
| S10-005 | `tasks/S10-005-color-picker-profiles.md` (создать) | после 004 |

## Инварианты

- Web spec (`powerpoint.ts`, `runCommand.ts`) — эталон HostScript.
- `dotnet test PptPowerKeys.sln` must stay green.
- `UserSettings.json` camelCase — Web export/import v1 compatible.
- VstoLegacy — только ribbon reference.

## Процесс сессии

1. Issue S10-001 → backlog **In Progress**
2. `/builder выполни S10-001`
3. Приёмка PR → merge → Done
4. Повторить для S10-002…005
5. `retrospective.md` → goals DoD → `PRODUCT_CONTEXT` → kickoff S11

## Copy-paste промпт (S10-001)

```
/architect

Sprint 10 — S10-001 Slide HostScript (CopySlide + MoveSlidesToBackup).
Sprint 09 Done (#73–#91). S09-006 merged.
Прочитай:
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-001-slide-commands.md
- sprints/epic-ltsc-windows-native/FEATURE_PARITY.md (Slides)
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (duplicateSelectedSlide, moveSelectedSlidesToBackup)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (CopySlide, MoveSlidesToBackup)
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpSlides reference)

Issue S10-001 → backlog In Progress → /builder выполни S10-001 → приёмка → merge.
```

# Architect Kickoff — Sprint 10 LTSC Slides · Settings · Web-None unlock

> **Статус:** In Progress. Следующая задача: **S10-004**.

## Контекст

- Sprint 09 **Done** — M3 Objects/Format/Text: 27 HostScript + 32 ServerLayout = **65** commands.
- **S10-001 Done** (PR #93) — +2 Slide HostScript → **67 commands**.
- **S10-002 Done** (PR #95) — +6 view/print None unlocks → **73 commands**.
- **S10-003 Done** (PR #97) — +3 format/objects/text None unlocks → **76 commands**. **9/9 None unlocks complete.**
- **S10-004 Todo** — Settings task pane + 3 Settings commands → target **79/79** commands routed.
- Linux CI green (**298** dotnet tests at S10-003 close).

## Цель Sprint 10

**M3 Feature beta (finish):** Slides + None unlocks + **Settings UI** + color picker COM.

| Wave | Команды | Задачи |
|------|---------|--------|
| Slides COM | CopySlide, MoveSlidesToBackup | S10-001 ✅ |
| None unlock view/print | 6 | S10-002 ✅ |
| None unlock format/objects/text | FormatPainter, PasteFormatted, Regroup | S10-003 ✅ |
| **Settings WPF pane** | OpenShortcutManager, OpenColorScheme, ResetToDefaults | **S10-004** ← next |
| Color picker + profiles | COM theme panel + consulting presets | S10-005 |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S10-001 | [`tasks/S10-001-slide-commands.md`](./tasks/S10-001-slide-commands.md) | Done (#93) |
| S10-002 | [`tasks/S10-002-view-print-none.md`](./tasks/S10-002-view-print-none.md) | Done (#95) |
| S10-003 | [`tasks/S10-003-format-regroup-none.md`](./tasks/S10-003-format-regroup-none.md) | Done (#97) |
| S10-004 | [`tasks/S10-004-settings-pane.md`](./tasks/S10-004-settings-pane.md) | `/builder выполни S10-004` |
| S10-005 | `tasks/S10-005-color-picker-profiles.md` (создать) | после 004 |

## Инварианты

- Web spec (`SettingsPanel.tsx`, `runCommand.ts`) — эталон Settings UX.
- `UserSettings.json` camelCase — Web export/import v1 compatible.
- `dotnet test PptPowerKeys.sln` must stay green.
- VstoLegacy — только reference.

## Процесс сессии (S10-004)

1. Issue S10-004 → backlog **In Progress**
2. `/builder выполни S10-004`
3. Architect приёмка PR → merge → Done
4. PRODUCT_CONTEXT + kickoff S10-005
5. После S10-005: `retrospective.md` → Sprint 10 close → S11

## Copy-paste промпт (S10-004 — Settings pane)

```
/architect

Sprint 10 — S10-004 Settings task pane + UserSettings (3 Settings commands).
S10-001…003 Done (#93, #95, #97 merged). 76 commands routed; Settings → NotSupportedException.

Прочитай:
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-004-settings-pane.md
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/backlog.md
- .github/review/CHECKLIST.md
- docs/migration/04-powerpoint-ltsc-windows-native.md
- src/PptPowerKeys.Windows/Settings/WindowsUserSettingsStore.cs
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx
- src/PptPowerKeys.Core/Settings/UserSettingsImporter.cs
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpSettings)

Шаг 1: Issue S10-004 → backlog In Progress → /builder выполни S10-004
Шаг 2: Приёмка PR — WPF pane, 3 Settings cmds, 79/79 routed, dotnet test green, CHECKLIST
Шаг 3: После merge — backlog Done, PRODUCT_CONTEXT, kickoff S10-005

Color picker full UI — S10-005. Global hotkeys — S11.
```

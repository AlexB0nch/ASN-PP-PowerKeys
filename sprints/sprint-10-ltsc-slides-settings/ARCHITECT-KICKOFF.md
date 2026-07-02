# Architect Kickoff — Sprint 10 LTSC Slides · Settings · Web-None unlock

> **Статус:** In Progress. **Последняя задача:** **S10-005** → закрытие Sprint 10.

## Контекст

- Sprint 09 **Done** — M3 Objects/Format/Text: 65 commands routed.
- **S10-001 Done** (PR #93) — Slide HostScript (+2).
- **S10-002 Done** (PR #95) — View/print None unlocks (+6) → 73 commands.
- **S10-003 Done** (PR #97) — FormatPainter, PasteFormatted, Regroup (+3) → 76 commands. **9/9 None unlocks complete.**
- **S10-004 Done** (PR #100) — Settings WPF pane + 3 Settings cmds → **79/79 commands routed**.
- **S10-005 Todo** — Color picker COM panel (replace Colors placeholder) → **M3 Feature beta complete**.
- Linux CI green (**316** dotnet tests at S10-004 close).

## Цель Sprint 10

**M3 Feature beta:** Slides + None unlocks + Settings UI + **Color picker COM**.

| Wave | Задачи |
|------|--------|
| Slides COM | S10-001 ✅ |
| None unlock view/print | S10-002 ✅ |
| None unlock format/objects/text | S10-003 ✅ |
| Settings WPF pane | S10-004 ✅ |
| **Color picker + presets** | **S10-005** ← last |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S10-001 | [`tasks/S10-001-slide-commands.md`](./tasks/S10-001-slide-commands.md) | Done (#93) |
| S10-002 | [`tasks/S10-002-view-print-none.md`](./tasks/S10-002-view-print-none.md) | Done (#95) |
| S10-003 | [`tasks/S10-003-format-regroup-none.md`](./tasks/S10-003-format-regroup-none.md) | Done (#97) |
| S10-004 | [`tasks/S10-004-settings-pane.md`](./tasks/S10-004-settings-pane.md) | Done (#100) |
| S10-005 | [`tasks/S10-005-color-picker-profiles.md`](./tasks/S10-005-color-picker-profiles.md) | `/builder выполни S10-005` |

## Инварианты

- Web `ColorPickerPanel.tsx` — эталон UI/UX.
- Reuse `ColorPaletteBuilder`, `ComHostAdapter` apply/read — no duplicate palette math.
- `dotnet test PptPowerKeys.sln` must stay green.

## Процесс сессии (S10-005 — закрытие спринта)

1. Issue S10-005 → backlog **In Progress**
2. `/builder выполни S10-005`
3. Architect приёмка PR → merge → Done
4. **Sprint 10 close:** `retrospective.md`, goals DoD, `PRODUCT_CONTEXT`, kickoff **S11**

## Copy-paste промпт (S10-005 — Color picker, закрытие Sprint 10)

```
/architect

Sprint 10 — S10-005 Color picker COM panel (последняя задача спринта).
S10-001…004 Done (#93–#100). 79/79 routed. Colors tab = placeholder.

Прочитай:
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-005-color-picker-profiles.md
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/backlog.md
- .github/review/CHECKLIST.md
- src/PptPowerKeys.AddIn/src/taskpane/ColorPickerPanel.tsx
- src/PptPowerKeys.Windows/UI/SettingsPane.xaml
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs
- src/PptPowerKeys.Windows/Host/CommandRouter.cs

Шаг 1: Issue S10-005 → backlog In Progress → /builder выполни S10-005
Шаг 2: Приёмка PR — picker UI, OpenColorScheme, theme/recent/apply/pick/HEX, dotnet test, CHECKLIST
Шаг 3: После merge — retrospective.md, goals DoD, PRODUCT_CONTEXT (M3), kickoff S11

Screen eyedropper — anti-scope. Global hotkeys — S11.
```

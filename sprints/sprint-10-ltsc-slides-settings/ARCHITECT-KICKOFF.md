# Architect Kickoff — Sprint 10 LTSC Slides · Settings · Web-None unlock

> **Статус:** Done (2026-07-02). **M3 Feature beta** достигнут.

## Контекст

- Sprint 09 **Done** — M3 Objects/Format/Text beta: 65 commands routed.
- **S10-001 Done** (PR #93) — +2 Slide HostScript → **67 commands routed**.
- **S10-002 Done** (PR #95) — +6 view/print None unlocks → **73 commands routed**.
- **S10-003 Done** (PR #97) — +3 format/objects/text None unlocks → **76 commands routed**. **9/9 None unlocks complete.**
- **S10-004 Done** (PR #100) — Settings WPF pane + 3 Settings cmds → **79/79 commands routed**.
- **S10-005 Done** (PR #104) — Color picker COM panel + `FormatColorPaletteProvider`.
- Linux CI green (`dotnet test` 319 passed at S10 close).

## Итог Sprint 10

**M3 Feature beta:** Slides (8) + **9 OfficeJs None unlocks** + **Settings UI** + color picker panel COM.

| Wave | Команды | Задачи |
|------|---------|--------|
| Slides COM | CopySlide, MoveSlidesToBackup | S10-001 ✅ |
| None unlock view/print | Zoom, Sorter, SlideShow, Grid, Guides, Print | S10-002 ✅ |
| None unlock format/objects/text | FormatPainter, PasteFormatted, Regroup | S10-003 ✅ |
| Settings WPF pane | profiles, shortcuts, import/export, addupDisplayMode | S10-004 ✅ |
| Color picker + profiles | COM theme panel + consulting presets | S10-005 ✅ |

## Следующий спринт

**S11 — LTSC Ship (Hotkeys · MSI · QA)** — [`../sprint-11-ltsc-ship/ARCHITECT-KICKOFF.md`](../sprint-11-ltsc-ship/ARCHITECT-KICKOFF.md)  
Первая задача: **S11-001** Native ShortcutManager.

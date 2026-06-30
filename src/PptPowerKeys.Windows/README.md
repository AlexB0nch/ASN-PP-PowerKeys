# PptPowerKeys.Windows — LTSC / perpetual Office (Product Line B)

VSTO add-in for **Windows PowerPoint LTSC / perpetual Office**. Uses **in-process**
`PptPowerKeys.Core` (netstandard2.0). See [ADR-001](../../docs/adr/ADR-001-ltsc-windows-native-line.md).

> **Not** `PptPowerKeys.VstoLegacy*` — that tree is frozen historical scaffold.

## Build (Windows only)

Requirements:

- Windows 10/11
- Visual Studio 2022 with **Office/SharePoint development** workload (VSTO)
- PowerPoint 2019 / 2021 / LTSC 2024 (desktop)

Steps:

1. Open `src/PptPowerKeys.Windows.sln` in Visual Studio.
2. Restore/build — Core resolves as `netstandard2.0`.
3. Press F5 to debug-sideload into PowerPoint.
4. Ribbon tab **PowerKeys** → **Test** shows bootstrap message with Core catalog count.
5. Select 2+ shapes (anchor = last selected) → **Align Left** aligns to anchor left edge (in-process Core, no HTTP).

### AlignLeft manual check (S07-003)

1. Draw three rectangles at different horizontal positions.
2. Multi-select: click first shapes, **last** click = anchor (rightmost or any anchor shape).
3. **PowerKeys** → **Align Left** — all shapes' left edges match anchor's left edge.
4. Confirm no network calls (offline / air-gap safe for layout path).

## Solution layout

| Project | TFM | Role |
|---------|-----|------|
| `PptPowerKeys.Windows` | .NET Framework 4.8 VSTO | COM host, Ribbon, Task Pane (future) |
| `PptPowerKeys.Core` | netstandard2.0 (from this sln) | Shared business logic |

Root `PptPowerKeys.sln` (Linux CI) does **not** include this solution.

## Related docs

- [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)
- [`sprints/epic-ltsc-windows-native/ROADMAP.md`](../../sprints/epic-ltsc-windows-native/ROADMAP.md)

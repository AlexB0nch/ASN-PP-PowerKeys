# Epic: PptPowerKeys.Windows (LTSC / perpetual Office)

> **Product line B.** Primary Web Add-in (Sprint 01–06) продолжает развиваться параллельно.  
> Архитектура: [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)  
> ADR: [`docs/adr/ADR-001-ltsc-windows-native-line.md`](../../docs/adr/ADR-001-ltsc-windows-native-line.md)

## Рекомендация (зафиксировано)

**Variant D — гибрид:** VSTO/COM host (`PptPowerKeys.Windows`) + **in-process** `PptPowerKeys.Core` + optional Companion (updater/license).

**Не PPAM/VBA.** **Не companion-only.**

---

## Multi-sprint roadmap (S07–S11)

| Sprint | Папка | Статус | Цель | Задачи (ID range) |
|--------|-------|--------|------|-------------------|
| **S07** | [`sprint-07-ltsc-foundation/`](../sprint-07-ltsc-foundation/) | **Next** | Spikes + VSTO shell + ComHost POC | S07-001…004 |
| **S08** | [`sprint-08-ltsc-layout-parity/`](../sprint-08-ltsc-layout-parity/) | Planned | 32 ServerLayout + alignment extras | S08-001…005 |
| **S09** | [`sprint-09-ltsc-objects-format-text/`](../sprint-09-ltsc-objects-format-text/) | Planned | Objects, Format, Text HostScript | S09-001…006 |
| **S10** | [`sprint-10-ltsc-slides-settings/`](../sprint-10-ltsc-slides-settings/) | Planned | Slides + 9 None unlock + Settings UI | S10-001…005 |
| **S11** | [`sprint-11-ltsc-ship/`](../sprint-11-ltsc-ship/) | Planned | Global hotkeys, MSI, QA, release | S11-001…005 |

### Milestones

| Milestone | После спринта | Критерий |
|-----------|---------------|----------|
| **M1 Technical prototype** | S07 | Add-in loads in PP; 1 command e2e (e.g. AlignLeft) |
| **M2 Layout beta** | S08 | All 32 ServerLayout + snap-to-grid |
| **M3 Feature beta** | S10 | 79 commands routed; Settings + profiles |
| **M4 Production** | S11 | Signed MSI; QA matrix; LTSC runbook |

---

## Порядок запуска сессий architect → builder

```
1. /architect  → прочитать ARCHITECT-KICKOFF текущего спринта
2. architect   → Issue + backlog In Progress + task file (если нет)
3. /builder    → `/builder выполни S07-001` (одна задача за PR)
4. /architect  → приёмка PR, merge, backlog Done
5. Повторять 3–4 до завершения спринта → retrospective.md
6. /architect  → kickoff следующего спринта (S08…)
```

**Правило:** одна задача `S0X-0YY` = один PR. Windows/VSTO задачи **не блокируют** Web CI (`PptPowerKeys.sln`); отдельный `PptPowerKeys.Windows.sln` (создаётся в S07-002).

---

## Epic backlog (сквозной)

| Epic | Sprint | Тема |
|------|--------|------|
| E1 Platform | S07 | Core multitarget, VSTO bootstrap, ComHost spike |
| E2 Command engine | S07–S08 | CommandRouter, ServerLayout pipeline |
| E3 Layout | S08 | 32 + copy-and-align + position |
| E4 Objects/Format/Text | S09 | HostScript wave 1 |
| E5 Slides + None | S10 | 8 slide + FormatPainter etc. |
| E6 Settings | S10 | Profiles, import/export, color picker COM |
| E7 Hotkeys | S11 | ShortcutManager native hook |
| E8 Ship | S11 | MSI, diagnostics, QA matrix |

---

## Anti-scope (весь epic)

- Размораживание `VstoLegacy*` для новых фич (только reference)
- PPAM/VBA production line
- Замена Web Add-in как primary product
- Mac / Web / Online support в Windows line
- Snap-to-nearest-object (остаётся backlog Web line)

---

## Команда / CI (ожидание)

- **Builder Windows tasks:** сборка на Windows + Visual Studio + VSTO workload (вне Linux CI до отдельного workflow).
- **Core changes (S07-001):** должны проходить `dotnet test PptPowerKeys.sln` на Linux CI.
- **Integration tests:** manual / Windows VM matrix (S11).

---

## Следующий шаг

**Sprint 07** — [`sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md`](../sprint-07-ltsc-foundation/ARCHITECT-KICKOFF.md)  
Первая задача: **S07-001** Core multitarget netstandard2.0.

# PptPowerKeys.Windows — LTSC / perpetual Office (Windows)

> **Product line B** (официальная альтернатива Web Add-in).  
> Primary line: [Office Web Add-in](./00-architecture.md) · Desktop sideload: [03-powerpoint-desktop-windows.md](./03-powerpoint-desktop-windows.md)  
> ADR: [ADR-001](../adr/ADR-001-ltsc-windows-native-line.md) · Roadmap: [`sprints/epic-ltsc-windows-native/ROADMAP.md`](../../sprints/epic-ltsc-windows-native/ROADMAP.md)

## Executive summary

**Предпочтительный путь: гибрид D — VSTO/COM host + shared `PptPowerKeys.Core` in-process.**

| | Web Add-in (primary) | PptPowerKeys.Windows (LTSC line) |
|--|---------------------|----------------------------------|
| Платформа | M365, Online, Desktop, Mac | **Windows LTSC / perpetual only** |
| Установка | `manifest.prod.xml` sideload / Central Deployment | **Signed MSI / ClickOnce** |
| Host | Office.js task pane | **VSTO Ribbon + Custom Task Pane** |
| Логика | Core via HTTP Api | **Core in-process** (offline-capable) |
| Global hotkeys | `KeyboardShortcuts 1.1` (PP 2601+) | **Native keyboard hook** |
| 9 `support=None` cmd | Unsupported | **Full COM implementation** |

PPAM/VBA **не рекомендуется** — не достигает полного паритета без отказа от shared Core.

---

## Когда нужна эта ветка

1. **Sideload Web Add-in недоступен** — нет «Upload My Add-in», IT блокирует manifest.
2. **Air-gap / offline** — нельзя зависеть от GitHub Pages + публичного API.
3. **Полный паритет** — нужны FormatPainter, PrintSlide, view toggles, Regroup и т.д.
4. **Global hotkeys на LTSC 2019/2021/2024** без ожидания Keyboard Shortcuts API.

### Track 0 (проверить до native line)

Для части LTSC-организаций достаточно **Centralized Deployment** Web manifest (без Upload UI):

- M365 Admin Center → Integrated apps → upload `manifest.prod.xml`
- Trusted Catalog (shared folder + registry)

Track 0 **не заменяет** native line для offline и 9 None-команд. Чеклист: задача **S07-004**.

---

## Целевая архитектура

```
PowerPoint LTSC / perpetual (Windows)
┌─────────────────────────────────────────────────────────────┐
│  PptPowerKeys.Windows  (VSTO Add-in, NEW — not VstoLegacy*) │
│   ├─ Ribbon (PowerKeys tab — см. VstoLegacy/UI as reference)│
│   ├─ Custom Task Pane (WPF; optional WebView2 later)        │
│   ├─ ShortcutManager (global keyboard hook)                 │
│   └─ ComHostAdapter  ◄──►  ShapeBounds boundary             │
└───────────────────────────────┬─────────────────────────────┘
                                │ in-process
                                ▼
┌─────────────────────────────────────────────────────────────┐
│  PptPowerKeys.Core  (netstandard2.0 + net8.0)               │
│  LayoutEngine · DuplicationEngine · CommandCatalog · …        │
└───────────────────────────────┬─────────────────────────────┘
                                │ optional
                                ▼
┌─────────────────────────────────────────────────────────────┐
│  PptPowerKeys.Api  (telemetry / license — optional online)  │
└─────────────────────────────────────────────────────────────┘

Optional: PptPowerKeys.Companion — updater, diagnostics, license heartbeat
```

### Слои

| Слой | Проект | Ответственность |
|------|--------|-----------------|
| Host integration | `PptPowerKeys.Windows` | COM: selection, shapes, slides, format, clipboard |
| Business logic | `PptPowerKeys.Core` | Layout, duplication math, addup, palette, settings |
| Command routing | `PptPowerKeys.Windows` | `CommandId` → ServerLayout / HostScript / Settings |
| UI | WPF Task Pane + Ribbon | 79 cmd, Settings, Color Picker, Shortcut Manager |
| Updates | MSI + optional Companion | Signed releases |

### Инварианты (как Web Add-in)

- **ShapeBounds** — host читает COM → Core считает → host пишет по `id`.
- **Anchor = последняя выделенная** фигура.
- **CommandCatalog** — единый source of truth (79 команд).
- **`UserSettings` JSON shape** — совместим с Web (import/export v1).

### Command execution (аналог `runCommand.ts`)

```
CommandRouter.Execute(commandId)
  ├─ ServerLayout (32) → ComHost.ReadBounds() → Core.LayoutEngine → ComHost.WriteBounds()
  ├─ HostScript (44)   → ComHost handlers (spec: AddIn/src/office/powerpoint.ts)
  └─ Settings (3)      → Task Pane panels
```

---

## Сравнение архитектурных вариантов

| Вариант | Паритет 79 cmd | UX | Установка | Обновления | Производительность | Риски | Трудоёмкость |
|---------|----------------|-----|-----------|------------|-------------------|-------|--------------|
| **A PPAM/VBA** | ~40–55% | Слабый | `.ppam` | Ручная | Средняя | Macros blocked; no Core reuse | Высокая, низкое качество |
| **B VSTO/COM** | **95–100%** | Native Ribbon | MSI | ClickOnce/MSI | **Лучшая** (in-process) | Windows-only; net48 vs net8 | Высокая, предсказуемая |
| **C Companion** | ~70–85% | Плохой | `.exe` | Свой updater | COM latency | Хрупкость | Высокий support cost |
| **D Гибрид ★** | **95–100%** | Как B | MSI | + Companion opt. | Как B | Две product lines | **Рекомендован** |

---

## Feature parity (кратко)

Полная матрица: [`sprints/epic-ltsc-windows-native/FEATURE_PARITY.md`](../../sprints/epic-ltsc-windows-native/FEATURE_PARITY.md).

| Класс | Кол-во | LTSC |
|-------|--------|------|
| **parity direct** | 32 | ServerLayout — Core 1:1 |
| **parity via rewrite** | 44 | COM rewrite `powerpoint.ts` |
| **Web-blocked → LTSC unlock** | **9** | FormatPainter, PasteFormatted, Regroup, view/slide cmds, PrintSlide |
| **Settings** | 3 | Native WPF panels |

**LTSC превосходит Web** по: global hotkeys на старых билдах + 9 None-команд.

---

## Roadmap спринтов (S07–S11)

| Sprint | Фокус | Ключевой результат |
|--------|-------|-------------------|
| **S07** | Foundation & spikes | Core multitarget; `PptPowerKeys.Windows` shell; ComHost spike; Track 0 doc |
| **S08** | Layout parity | 32 ServerLayout + copy-and-align + position clipboard |
| **S09** | Objects · Format · Text | HostScript wave 1 (~30 cmd) |
| **S10** | Slides · Settings · None unlock | 8 slide + 9 None + Settings UI |
| **S11** | Hotkeys · Ship | ShortcutManager; MSI; QA matrix Office 2019–LTSC 2024 |

Детали: [`sprints/epic-ltsc-windows-native/ROADMAP.md`](../../sprints/epic-ltsc-windows-native/ROADMAP.md).

---

## Risk register (top)

| ID | Risk | Mitigation |
|----|------|------------|
| R1 | IT blocks VSTO | Signed MSI; deployment pack; Track 0 Web fallback doc |
| R2 | Core net8 vs VSTO net48 | Multitarget netstandard2.0 (S07-001) |
| R3 | Dual codebase drift | Shared CommandCatalog + Core tests |
| R4 | Keyboard hook conflicts | Configurable bindings; conflict UI |
| R5 | Support cost two lines | Clear docs Web vs Windows |

---

## Spikes (S07)

| ID | Тема |
|----|------|
| S07-001 | Core → netstandard2.0 |
| S07-002 | VSTO project bootstrap |
| S07-003 | COM ShapeBounds adapter |
| S07-004 | Track 0 LTSC deploy checklist |

---

## Связанные документы

- [`01-vsto-to-officejs-mapping.md`](./01-vsto-to-officejs-mapping.md) — карта VSTO → Office.js (reference для COM rewrite)
- [`src/PptPowerKeys.VstoLegacy/FROZEN.md`](../../src/PptPowerKeys.VstoLegacy/FROZEN.md) — legacy **не** размораживать; новый код в `PptPowerKeys.Windows`
- [`docs/PRODUCT_CONTEXT.md`](../PRODUCT_CONTEXT.md) — dual product line journal

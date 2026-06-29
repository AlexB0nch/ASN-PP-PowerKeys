# ADR-001: Вторая product line — PptPowerKeys.Windows (LTSC / perpetual Office)

| Поле | Значение |
|------|----------|
| **Status** | Accepted (2026-06-29) |
| **Deciders** | architect |
| **Supersedes** | — |
| **Related** | [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../migration/04-powerpoint-ltsc-windows-native.md) |

## Context

Основной продукт — **Office Web Add-in** (79 команд, Sprint 01–06 Done). На **PowerPoint LTSC /
perpetual Office** sideload через `Upload My Add-in` часто недоступен (корпоративные политики, путаница
с `.ppam`, отсутствие Store UI). Web Add-in также не покрывает **9 команд** `OfficeJsSupport.None` и
глобальные hotkeys на билдах без `KeyboardShortcuts 1.1`.

## Decision

Ввести **официальную вторую product line**: **`PptPowerKeys.Windows`** — VSTO/COM add-in на Windows с
**in-process** использованием `PptPowerKeys.Core` (гибрид **Variant D**).

- **Primary line** (Web Add-in) продолжает развиваться без изменений приоритета.
- **Новый код** — в `src/PptPowerKeys.Windows/` (новый проект). **`VstoLegacy*` остаётся frozen** как исторический scaffold; не размораживать ad-hoc.
- Core multitarget → `netstandard2.0` + `net8.0` для ссылок из .NET Framework 4.8 host.
- LTSC air-gap: layout/settings **без HTTP** (Core in-process); Api опционален (telemetry/license).

## Alternatives considered

| Вариант | Отвергнут потому что |
|---------|---------------------|
| A — PPAM/VBA | Нет reuse Core; unmaintainable at 79 commands; macro policies |
| B — VSTO alone | Принят как **host layer** внутри D |
| C — Desktop companion | Плохой UX; высокий support cost |
| Web-only + IT deploy | Track 0 полезен, но не закрывает None-команды и offline |

## Consequences

**Positive:** полный функциональный паритет; native global hotkeys на LTSC; 9 «Web-blocked» команд; offline layout.

**Negative:** два installable артефакта; Windows-only CI; code signing; dual support matrix (Web vs Windows).

**Follow-up:** Sprint 07–11 (`sprints/epic-ltsc-windows-native/ROADMAP.md`).

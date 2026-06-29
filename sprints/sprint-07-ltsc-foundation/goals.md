# Sprint 07 — LTSC Foundation (PptPowerKeys.Windows)

> Epic: [`../epic-ltsc-windows-native/ROADMAP.md`](../epic-ltsc-windows-native/ROADMAP.md)  
> Архитектура: [`docs/migration/04-powerpoint-ltsc-windows-native.md`](../../docs/migration/04-powerpoint-ltsc-windows-native.md)

## Цель спринта

Заложить **технический фундамент** product line B: shared Core на netstandard2.0, новый VSTO host
`PptPowerKeys.Windows`, spike COM↔ShapeBounds, документ Track 0 для org без sideload UI.

**Exit milestone M1:** PowerPoint загружает add-in; **одна команда** (AlignLeft) end-to-end через Core in-process.

## Scope

| ID | Задача | Компонент |
|----|--------|-----------|
| S07-001 | Core multitarget netstandard2.0 | Core + Tests |
| S07-002 | PptPowerKeys.Windows VSTO bootstrap | Windows (new sln) |
| S07-003 | ComHost ShapeBounds spike | Windows + Core |
| S07-004 | Track 0 LTSC deploy checklist (Web Add-in path) | docs |

## Anti-scope

- Полный parity layout/objects (→ S08–S10)
- MSI installer (→ S11)
- Размораживание `VstoLegacy*`
- Изменения Web Add-in / manifest (кроме docs cross-links)

## Definition of Done спринта

- [ ] S07-001…004 Done
- [ ] `dotnet test PptPowerKeys.sln` зелёный (Linux CI)
- [ ] Windows: `PptPowerKeys.Windows.sln` собирается; AlignLeft POC manual note
- [ ] `docs/PRODUCT_CONTEXT.md` journal Sprint 07 kickoff
- [ ] `retrospective.md` при закрытии спринта

## Зависимости

- Sprint 06 Done (Web line stable)
- ADR-001 accepted

## Следующий спринт

**S08** — Layout parity (32 ServerLayout + extras).

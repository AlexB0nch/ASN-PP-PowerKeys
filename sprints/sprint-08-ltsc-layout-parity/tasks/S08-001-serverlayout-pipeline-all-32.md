# S08-001 — CommandRouter: all 32 ServerLayout commands

> Передача builder'у: `/builder выполни S08-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S08-001` |
| **Спринт** | `sprint-08-ltsc-layout-parity` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (reference only) |
| **Статус** | In Progress |
| **Issue** | [#62](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/62) |
| **PR** | — |

## Цель

Расширить **`CommandRouter`** (S07-003 POC только `AlignLeft`) на **все 32 ServerLayout команды**
из `LayoutEngine.IsLayoutCommand` — единый in-process pipeline без HTTP:

```
COM selection → ShapeBounds[] → LayoutEngine.Apply → ComHostAdapter.ApplyShapeBounds
```

## Контекст (после Sprint 07)

| Компонент | Состояние |
|-----------|-----------|
| `ComHostAdapter` | Read/apply ShapeBounds; anchor = last in selection order |
| `CommandRouter` | Switch только `AlignLeft`; остальное `NotSupportedException` |
| `LayoutEngine` | Все 32 layout cmd реализованы и покрыты Core tests |
| Ribbon | Только кнопка Align Left (+ bootstrap) — **S08-003** |
| Snap-to-grid | Web: `UserSettings.SnapToGrid` + `LayoutOptions` — **S08-002** |

## Список 32 ServerLayout команд

**Alignment (12):** AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical,
AlignBottom, DistributeHorizontal, DistributeVertical, AlignLeftToRight, AlignRightToLeft,
AlignTopToBottom, AlignBottomToTop.

**Resize (20):** SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect,
WidthEqualsAnchorHeight, HeightEqualsAnchorWidth, StretchWidthToLeft, StretchWidthToRight,
StretchHeightToTop, StretchHeightToBottom, IncreaseWidthLarge, DecreaseWidthLarge,
IncreaseHeightLarge, DecreaseHeightLarge, IncreaseWidthSmall, DecreaseWidthSmall,
IncreaseHeightSmall, DecreaseHeightSmall, IncreaseSizeKeepAspect, DecreaseSizeKeepAspect.

Source of truth: `LayoutEngine.IsLayoutCommand()` in `src/PptPowerKeys.Core/Layout/LayoutEngine.cs`.

## Решения architect (зафиксировано)

### Generic dispatch (не 32-case switch)

```csharp
if (!LayoutEngine.IsLayoutCommand(command))
    throw new NotSupportedException(...);
return ExecuteServerLayout(command, layoutOptions: null); // snap in S08-002
```

`ExecuteServerLayout` остаётся один метод; `LayoutRequest.Command = command`.

### LayoutOptions / snap-to-grid

**S08-001:** `LayoutOptions` = `null` (как сейчас AlignLeft). Snap — отдельная задача **S08-002**.

### Ribbon

**S08-001:** не обязателен полный ribbon. Достаточно:
- programmatic/manual test path (существующая AlignLeft кнопка + **unit tests** с mock `IComHostAdapter`), **или**
- временный debug hook / расширение ribbon на 2–3 команды для smoke (optional, architect не блокирует приёмку).

Полный layout ribbon — **S08-003**.

### Error UX

- `< 2 shapes` для align/transform → `LayoutResult.NoChange` + message (Core уже возвращает); не crash.
- Empty selection → понятное сообщение (MessageBox или Debug — match AlignLeft pattern).

### Tests

- Новый `PptPowerKeys.Windows.Tests` **optional** в S08-001; минимум — mock `IComHostAdapter` tests если быстро.
- **Обязательно:** `dotnet test PptPowerKeys.sln` green (Linux CI, Core unchanged behavior).

## Scope builder

| Файл | Изменение |
|------|-----------|
| `src/PptPowerKeys.Windows/Host/CommandRouter.cs` | Generic ServerLayout dispatch для 32 cmd |
| `src/PptPowerKeys.Windows/Host/IComHostAdapter.cs` | Без изменений unless needed |
| `src/PptPowerKeys.Windows/README.md` | § manual QA: перечень 32 cmd testable via router API |
| Optional | `PptPowerKeys.Windows.Tests` + mock adapter tests |

## Анти-scope

- Snap-to-grid / UserSettings (S08-002)
- Ribbon layout group UI (S08-003)
- CopyAndAlign* HostScript (S08-004)
- Copy/Paste position (S08-005)
- HTTP / Api calls
- VstoLegacy*
- Web Add-in changes

## Критерии приёмки

- [ ] `CommandRouter.Execute(commandId)` работает для **всех 32** `LayoutEngine.IsLayoutCommand` ids
- [ ] Non-layout commands → `NotSupportedException` (или явный guard)
- [ ] In-process `LayoutEngine.Apply`; **no HTTP**
- [ ] Anchor semantics unchanged (last selected)
- [ ] `dotnet test PptPowerKeys.sln` — зелёный
- [ ] Manual QA note (Windows): минимум AlignLeft + SameWidth + DistributeHorizontal regression (в PR или README)
- [ ] `.github/review/CHECKLIST.md` — architect post-merge

## Зависимости

- Sprint 07 Done (PR #59–#61): ComHostAdapter, CommandRouter, AlignLeft POC

## Трассировка

Issue `#N` → `cursor/S08-001-serverlayout-pipeline-*` → PR `Closes #N`

## Следующие задачи спринта

- **S08-002** — Snap-to-grid (`LayoutOptions.SnapToGrid`)
- **S08-003** — Ribbon layout group
- **S08-004** — CopyAndAlign* (4 HostScript)
- **S08-005** — Position clipboard + QA matrix

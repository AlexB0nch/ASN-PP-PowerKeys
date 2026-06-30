# S08-003 — Ribbon layout command group (32 ServerLayout)

> Передача builder'у: `/builder выполни S08-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S08-003` |
| **Спринт** | `sprint-08-ltsc-layout-parity` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` (Ribbon UI) |
| **Статус** | Todo |
| **Issue** | — (architect создаёт) |
| **PR** | — |

## Цель

Добавить на вкладку **PowerKeys** ribbon-кнопки для **всех 32 ServerLayout команд**, вызывающих
существующий `CommandRouter.Execute(CommandIds)` — без дублирования layout-логики в UI.

## Контекст

| Компонент | Состояние |
|-----------|-----------|
| S08-001 | `CommandRouter` — generic dispatch 32 cmd ✓ |
| S08-002 | Snap checkbox + `LayoutOptions` (recommended **Done** before or merge-friendly) |
| Ribbon today | 1 button Align Left + Bootstrap «Test» |
| Reference | `VstoLegacy/UI/RibbonTab.xml` — группы Alignment / Resize (read-only) |

## Решения architect (зафиксировано)

### Generic ribbon handler (DRY)

Один callback вместо 32 методов:

```csharp
public void OnLayoutCommand(IRibbonControl control)
{
    var commandId = ParseCommandId(control); // from control.Id or tag
    ExecuteLayout(commandId);
}
```

- `ParseCommandId`: mapping `btnAlignLeft` → `CommandIds.AlignLeft` (dictionary или prefix `cmd_` + enum name).
- Shared `ExecuteLayout(CommandIds)`: router null-check, try/catch, MessageBox on error (как `OnAlignLeft` today).
- Удалить dedicated `OnAlignLeft` после миграции на generic path.

### Ribbon groups (32 buttons)

| Group | Commands (count) |
|-------|------------------|
| **Alignment** | AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical, AlignBottom, DistributeHorizontal, DistributeVertical **(8)** |
| **Stack / Position** | AlignLeftToRight, AlignRightToLeft, AlignTopToBottom, AlignBottomToTop **(4)** |
| **Size match** | SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect, WidthEqualsAnchorHeight, HeightEqualsAnchorWidth **(6)** |
| **Stretch** | StretchWidthToLeft, StretchWidthToRight, StretchHeightToTop, StretchHeightToBottom **(4)** |
| **Nudge large** | IncreaseWidthLarge, DecreaseWidthLarge, IncreaseHeightLarge, DecreaseHeightLarge, IncreaseSizeKeepAspect, DecreaseSizeKeepAspect **(6)** |
| **Nudge small** | IncreaseWidthSmall, DecreaseWidthSmall, IncreaseHeightSmall, DecreaseHeightSmall **(4)** |

**Total: 32.** Labels — English short (как VstoLegacy) или CommandCatalog `Title`; tooltips — из catalog title/notes где есть.

### imageMso

Reuse Office built-in icons per VstoLegacy mapping where possible (`ObjectsAlignLeft`, `ObjectsSameWidth`, …).
Fallback: `HappyFace` или generic shape icons — document in PR if missing.

### Bootstrap group

**Remove** `grpBootstrap` / Test button (M1 debug) **или** оставить скрытым — architect: **remove** для cleaner UX.

### Snap checkbox

Если **S08-002 Done** — checkbox уже в ribbon; **не дублировать**.  
Если S08-002 ещё Todo — оставить место в `grpLayout` / отдельный `grpOptions`; snap реализует S08-002.

### Box sizes

Ribbon overflow: использовать `size="normal"` для nudge buttons; `large` только для primary align (optional).

## Scope builder

| Файл | Изменение |
|------|-----------|
| `UI/RibbonTab.xml` | 5–6 groups, 32 buttons, `onAction="OnLayoutCommand"` |
| `UI/PowerKeysRibbon.cs` | Generic handler + id→CommandIds map |
| `UI/RibbonCommandMap.cs` (optional) | Static dictionary control.Id → CommandIds |
| `README.md` | Ribbon screenshot note / manual QA list |

## Анти-scope

- HostScript commands (S08-004 CopyAndAlign)
- Objects / Format / Text / Slides groups (S09–S10)
- ShortcutManager / global hotkeys (S11)
- New layout math in Core
- Web Add-in
- VstoLegacy code edits

## Критерии приёмки

- [ ] **32** ribbon buttons wired → `CommandRouter.Execute` for matching ServerLayout id
- [ ] Manual Windows QA: по 1 cmd из каждой group (≥5 total) + regression AlignLeft
- [ ] Invalid/disabled: empty selection → user-friendly message (no unhandled exception)
- [ ] Bootstrap Test button removed (or justified in PR)
- [ ] `dotnet test PptPowerKeys.sln` green (no Core changes expected)
- [ ] No duplicate snap toggle if S08-002 merged

## Зависимости

- **Required:** S08-001 Done (PR #63)
- **Recommended:** S08-002 Done (snap flows through router automatically once wired)

## Трассировка

Issue `#N` → `cursor/S08-003-ribbon-layout-group-*` → PR `Closes #N`

## CommandId checklist (32)

```
AlignLeft, AlignCenterHorizontal, AlignRight, AlignTop, AlignMiddleVertical, AlignBottom,
DistributeHorizontal, DistributeVertical,
AlignLeftToRight, AlignRightToLeft, AlignTopToBottom, AlignBottomToTop,
SameWidth, SameHeight, SameWidthKeepAspect, SameHeightKeepAspect,
WidthEqualsAnchorHeight, HeightEqualsAnchorWidth,
StretchWidthToLeft, StretchWidthToRight, StretchHeightToTop, StretchHeightToBottom,
IncreaseWidthLarge, DecreaseWidthLarge, IncreaseHeightLarge, DecreaseHeightLarge,
IncreaseWidthSmall, DecreaseWidthSmall, IncreaseHeightSmall, DecreaseHeightSmall,
IncreaseSizeKeepAspect, DecreaseSizeKeepAspect
```

Source: `LayoutEngine.IsLayoutCommand()`.

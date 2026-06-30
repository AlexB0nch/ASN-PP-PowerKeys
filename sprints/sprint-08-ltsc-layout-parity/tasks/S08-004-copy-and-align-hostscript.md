# S08-004 — Copy-and-align HostScript (4 commands)

> Передача builder'у: `/builder выполни S08-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S08-004` |
| **Спринт** | `sprint-08-ltsc-layout-parity` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | In Progress |
| **Issue** | [#68](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/68) |
| **PR** | — |

## Цель

Реализовать **первую HostScript-волну** на Windows line: 4 команды **Copy-and-align**
(duplicate selection at source position → align clones to anchor using Core layout).

| CommandId | Layout step |
|-----------|-------------|
| CopyAndAlignLeft | AlignLeft |
| CopyAndAlignRight | AlignRight |
| CopyAndAlignTop | AlignTop |
| CopyAndAlignBottom | AlignBottom |

Parity с Web Add-in `runCopyAndAlign()` in `runCommand.ts`.

## Контекст (после S08-001…003)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | Только `LayoutEngine.IsLayoutCommand` (32); CopyAndAlign → `NotSupportedException` |
| `ComHostAdapter` | Read/apply bounds **на текущем selection** |
| Web flow | `cloneSelectedShapesAtSourcePositions` → `combined = originals + clones` → `api.applyLayout` with `anchorIndex = originals.length - 1` → `applyShapeBoundsOnSlide` |
| Ribbon | 32 layout buttons; **нет** CopyAndAlign |

## Алгоритм (зафиксировано — match Web)

```
1. originals = ReadSelectedShapeBounds()  // empty → error
2. clones = CloneSelectedAtSourcePositions()  // COM Duplicate, same Left/Top
3. combined = originals ++ clones
4. anchorIndex = originals.Count - 1  // last originally selected
5. layoutCmd = map CopyAndAlign* → AlignLeft/Right/Top/Bottom
6. result = LayoutEngine.Apply(new LayoutRequest {
     Command = layoutCmd,
     Shapes = combined,
     AnchorIndex = anchorIndex,
     Options = GetLayoutOptions()  // snap from S08-002
   })
7. if result.Changed → ApplyShapeBoundsOnSlide(result.Shapes)  // by id, not selection-limited
8. return success message: "Duplicated and aligned N shape(s)."
```

## Решения architect

### CommandRouter API

Расширить `Execute(CommandIds)`:

```csharp
if (LayoutEngine.IsLayoutCommand(command)) → ExecuteServerLayout(...)
if (IsCopyAndAlign(command)) → ExecuteCopyAndAlign(...)
else → NotSupportedException
```

`IsCopyAndAlign` — static helper или switch на 4 ids.

**Return type:** ввести `CommandExecutionResult` (Changed, Message) **или** переиспользовать `LayoutResult` + optional wrapper для HostScript messages. Architect: minimal — `CommandExecutionResult` record в Windows project.

### ComHostAdapter extensions

| Method | Behavior |
|--------|----------|
| `CloneSelectedAtSourcePositions()` | COM `Shape.Duplicate()` per selected shape; reset Left/Top to source; return new `ShapeBounds[]` |
| `ApplyShapeBoundsOnSlide(IReadOnlyList<ShapeBounds>)` | Find shapes on active slide by id (not only selection); apply geometry |

Existing `ApplyShapeBounds` may remain selection-scoped; slide-scoped apply **required** for clones+originals after layout.

### Ribbon

- New group **Copy & Align** (4 buttons) on PowerKeys tab.
- Control ids: `btnCopyAndAlignLeft`, … (match enum names).
- Extend routing: `RibbonCommandMap` → split **Layout** vs **HostScript** maps, **or** single `TryParseHostScript`.
- `PowerKeysRibbon`: `OnHostScriptCommand` or unified `OnPowerKeysCommand` dispatching to router.

### Tests

- Unit test mapping CopyAndAlign → layout command (static).
- Optional: mock adapter test for combined array + anchor index logic (extract pure helper if useful).
- `dotnet test PptPowerKeys.sln` green.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/ComHostAdapter.cs` | Clone + ApplyShapeBoundsOnSlide |
| `Host/IComHostAdapter.cs` | New methods |
| `Host/CommandRouter.cs` | CopyAndAlign pipeline |
| `Host/CopyAndAlignCommands.cs` (optional) | Map + orchestration |
| `UI/RibbonTab.xml` | 4 buttons |
| `UI/RibbonCommandMap.cs` or `HostScriptCommandMap.cs` | Parse host ids |
| `UI/PowerKeysRibbon.cs` | Wire handlers |
| `README.md` | Manual QA CopyAndAlign |

## Анти-scope

- DuplicateRight/Left/Up/Down with offset (S09 — DuplicationEngine)
- Copy/Paste object position (S08-005)
- Other HostScript categories (S09)
- HTTP Api
- Core layout math changes
- VstoLegacy edits

## Критерии приёмки

- [ ] All 4 CopyAndAlign commands work manual QA (2+ shapes, anchor = last selected before duplicate)
- [ ] Snap-to-grid respected when enabled (S08-002)
- [ ] Empty selection → user-friendly error
- [ ] Clones created on active slide; layout applied to originals + clones
- [ ] Ribbon 4 buttons wired
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR manual QA note + `.github/review/CHECKLIST.md`

## Зависимости

- S08-001 Done (#63)
- S08-002 Done (#65) — LayoutOptions snap
- S08-003 Done (#67) — ribbon pattern reference

## Reference files (Web spec)

- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — `runCopyAndAlign`, `COPY_AND_ALIGN_LAYOUT`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `cloneSelectedShapesAtSourcePositions`, `applyShapeBoundsOnSlide`

## Трассировка

Issue `#N` → `cursor/S08-004-copy-and-align-*` → PR `Closes #N`

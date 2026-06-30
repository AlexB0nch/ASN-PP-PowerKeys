# S08-005 — Position clipboard + layout QA notes

> Передача builder'у: `/builder выполни S08-005`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S08-005` |
| **Спринт** | `sprint-08-ltsc-layout-parity` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + docs |
| **Статус** | Todo |
| **Issue** | — (architect создаёт) |
| **PR** | — |

## Цель

1. **Position clipboard** — `CopyObjectPosition` / `PasteObjectPosition` (parity с Web Add-in).
2. **Layout QA pack** — consolidated manual test matrix для всего Sprint 08 (M2 Layout beta).
3. Закрыть **Sprint 08** (последняя задача спринта).

## Контекст (после S08-001…004)

| Компонент | Состояние |
|-----------|-----------|
| ServerLayout | 32 cmd + ribbon ✓ |
| Snap-to-grid | UserSettings + checkbox ✓ |
| CopyAndAlign | 4 cmd + ribbon ✓ |
| Position clipboard | **Not implemented** on Windows |
| Web reference | `positionClipboard.ts` + `copyObjectPosition` / `pasteObjectPosition` in `powerpoint.ts` |

## Алгоритм position clipboard (match Web)

### CopyObjectPosition

```
1. shapes = ReadSelectedShapeBounds()
2. if empty → error "Select a shape first."
3. anchor = shapes[last]
4. snapshot = { Left: anchor.Left, Top: anchor.Top }
5. PositionClipboardStore.Set(snapshot)  // in-memory, session scope
6. message: "Copied position (X, Y)."
```

### PasteObjectPosition

```
1. snapshot = PositionClipboardStore.Get()
2. if null → error "Copy a position first (Copy object position)."
3. Apply Left/Top to all selected shapes (Width/Height unchanged)
4. message: "Pasted position to N shape(s)."
```

**Scope:** Left/Top only (не width/height) — как Web.

## Решения architect

### PositionClipboardStore

- New class `Host/PositionClipboardStore.cs` — static or singleton on `ThisAddIn` (session lifetime).
- **Не persist** to disk (match Web in-memory; not UserSettings).
- Optional: `Clear()` for tests.

### CommandRouter

Extend `Execute`:

```csharp
CopyObjectPosition → ExecuteCopyObjectPosition()
PasteObjectPosition → ExecutePasteObjectPosition()
```

Return `CommandExecutionResult` with user-facing `Message` (existing S08-004 pattern).

### ComHostAdapter

| Method | Behavior |
|--------|----------|
| `ApplyPositionToSelection(left, top)` | Set `Shape.Left`, `Shape.Top` for all selected; return count |

Copy reads via existing `ReadSelectedShapeBounds()`.

### Ribbon

- Add to **Stack** group (or new **Position** group): **Copy Position**, **Paste Position**.
- Ids: `btnCopyObjectPosition`, `btnPasteObjectPosition`.
- Extend `HostScriptCommandMap.TryParse` — support CopyAndAlign **+** position commands (shared prefix `btn` + enum name).

### Layout QA notes (docs)

Create **`docs/migration/06-windows-layout-qa.md`** (or `sprints/sprint-08-ltsc-layout-parity/LAYOUT_QA.md` + link from README):

| Section | Content |
|---------|---------|
| Prerequisites | Windows + VS sideload, PP version |
| M2 scope | 32 ServerLayout + snap + 4 CopyAndAlign + 2 position |
| Matrix table | Command / setup / expected (consolidate from README fragments) |
| Snap regression | ON vs OFF spot checks |
| Anchor rule | Last selected = anchor (all layout cmds) |

Update `PptPowerKeys.Windows/README.md` — link to QA doc; trim duplicate tables if consolidated.

### Sprint close (architect post-merge, not builder)

- `sprints/sprint-08-ltsc-layout-parity/retrospective.md`
- `goals.md` DoD checkboxes
- `PRODUCT_CONTEXT.md` journal S08-005 + Sprint 08 complete
- `sprints/README.md` if needed

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/PositionClipboardStore.cs` | In-memory snapshot |
| `Host/CommandRouter.cs` | Copy/Paste position handlers |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | ApplyPositionToSelection |
| `UI/HostScriptCommandMap.cs` | Include position command ids |
| `UI/RibbonTab.xml` | 2 buttons |
| `UI/PowerKeysRibbon.cs` | Already has OnHostScriptCommand — verify |
| `PptPowerKeys.Tests/PositionClipboardStoreTests.cs` (optional) | Store get/set |
| `docs/migration/06-windows-layout-qa.md` | QA matrix |
| `README.md` (Windows) | Link + position manual QA rows |

## Анти-scope

- Persist position to UserSettings / file
- Width/height paste
- New Core layout commands
- Sprint retrospective (architect writes post-merge)
- S09 Objects/Format

## Критерии приёмки

- [ ] CopyObjectPosition: stores anchor last-selected Left/Top; success message
- [ ] PasteObjectPosition: applies to all selected; error if no prior copy
- [ ] Width/Height unchanged on paste
- [ ] Ribbon 2 buttons wired
- [ ] QA doc published and linked from Windows README
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA note in PR (copy → paste flow)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S08-001…004 Done (#63–#69)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/office/positionClipboard.ts`
- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — `copyObjectPosition`, `pasteObjectPosition`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — cases CopyObjectPosition, PasteObjectPosition

## Трассировка

Issue `#N` → `cursor/S08-005-position-clipboard-*` → PR `Closes #N`

## Architect post-merge (Sprint 08 close)

- [ ] `retrospective.md` for sprint-08
- [ ] All S08-001…005 Done in backlog
- [ ] Kickoff pointer → Sprint 09 in epic ROADMAP

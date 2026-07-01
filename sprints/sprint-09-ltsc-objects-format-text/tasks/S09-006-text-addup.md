# S09-006 — Text commands + Addup (5 HostScript commands)

> Передача builder'у: `/builder выполни S09-006`  
> **Последняя задача Sprint 09** — после merge **architect** закрывает спринт (не builder).

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-006` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S09-001…005 Done |

## Цель

Реализовать **Text HostScript wave** на Windows line — 5 команд + Addup через Core in-process.

| CommandId | COM / behavior | Success message (match Web) |
|-----------|----------------|----------------------------|
| PasteUnformatted | `Clipboard.GetText()` → `TextFrame.TextRange.Text` | `Pasted plain text into N shape(s).` |
| ReplaceWithEllipsis | Set text to `"..."` on text frames | `Replaced text with "..." on N shape(s).` |
| ToggleSuperscript | `Font.Superscript` toggle; disable subscript when enabling | `Toggled superscript on N shape(s).` |
| ToggleSubscript | `Font.Subscript` toggle; disable superscript when enabling | `Toggled subscript on N shape(s).` |
| AddupTextFields | Core `NumberAggregator` + `AddupStatusFormatter` | dynamic (see below) |

**PasteFormatted — anti-scope** (Web `OfficeJsSupport.None` → S10 unlock).

Parity с Web Add-in `runCommand.ts` + `powerpoint.ts`.

## Контекст (после S09-001…005)

| Компонент | Ожидаемое состояние |
|-----------|---------------------|
| `CommandRouter` | Objects + Format HostScript через S09-001…005 |
| `HostScriptCommandMap` | Расширен по предыдущим S09 задачам |
| `WindowsUserSettingsStore` | `UserSettings.json` — snap; **`AddupDisplayMode` в модели Core, read в S09-006** |
| Core | `NumberAggregator.Compute`, `AddupStatusFormatter.Format` — покрыты тестами |
| Web Addup | Api `/api/text/addup` на Web; Windows — **in-process Core** (без HTTP) |
| Legacy ribbon | `VstoLegacy/UI/RibbonTab.xml` → `grpText` (4 кнопки; **нет** ReplaceWithEllipsis) |

## Алгоритм (match Web)

### PasteUnformatted

```
1. text = Clipboard.GetText(); empty → "Clipboard is empty."
2. shapes = selected; empty → "Select one or more shapes first."
3. for each shape with TextFrame:
     TextRange.Text = text; applied++
4. applied == 0 → "Selected shape(s) have no text frame to paste into."
5. message: "Pasted plain text into {applied} shape(s)."
```

Use `System.Windows.Forms.Clipboard` (already referenced via ribbon MessageBox).

### ReplaceWithEllipsis

```
1. shapes = selected; empty → "Select one or more shapes first."
2. for each shape with TextFrame: TextRange.Text = "..."; applied++
3. applied == 0 → "Selected shape(s) have no text to replace."
4. message: 'Replaced text with "..." on {applied} shape(s).'
```

Constant `ELLIPSIS = "..."` (match Web).

### ToggleSuperscript / ToggleSubscript

```
1. shapes = selected; empty → "Select one or more shapes first."
2. collect fonts from shapes with TextFrame; none → "Selected shape(s) have no text to format."
3. for each font:
     if Superscript command:
       enable = (Superscript != msoTrue); Superscript = enable; if enable Subscript = msoFalse
     if Subscript command:
       enable = (Subscript != msoTrue); Subscript = enable; if enable Superscript = msoFalse
4. message: "Toggled superscript|subscript on {count} shape(s)."
```

Mutual exclusivity — как Web `toggleScriptAttribute`.

### AddupTextFields

```
1. texts = ReadSelectedShapeTexts()  // one entry per selected shape, "" if no text frame
2. stats = NumberAggregator.Compute(texts)  // Core in-process
3. mode = WindowsUserSettingsStore.Current.AddupDisplayMode
4. message = AddupStatusFormatter.Format(stats, mode)
5. return CommandExecutionResult { Changed = stats.Count > 0, Message = message }
```

**Addup messages** (Core — must match Web `addupStatus.ts`):

| Condition | Message |
|-----------|---------|
| stats.Count == 0 | `No numbers found in selection.` |
| mode `all` | `Sum X · avg Y · min Z · max W (N numbers).` |
| mode `sum` | `Sum X (N numbers).` |
| mode `min` / `max` / `average` | `Min/Max/Avg X (N numbers).` |

Mode normalization: `AddupStatusFormatter.NormalizeMode` (unknown → `all`).

## Решения architect

### TextCommands helper

New file `Host/TextCommands.cs`:

```csharp
public static class TextCommands
{
    public static bool IsTextCommand(CommandIds command) => command switch
    {
        CommandIds.PasteUnformatted or CommandIds.ReplaceWithEllipsis
            or CommandIds.ToggleSuperscript or CommandIds.ToggleSubscript
            or CommandIds.AddupTextFields => true,
        _ => false,
    };
}
```

### CommandRouter

```csharp
if (TextCommands.IsTextCommand(command))
    return ExecuteTextCommand(command);
```

Handlers delegate to `ComHostAdapter` + Core for Addup.

### ComHostAdapter extensions

| Method | Behavior |
|--------|----------|
| `IReadOnlyList<string> ReadSelectedShapeTexts()` | Selection order; `""` if no TextFrame |
| `int PasteUnformattedText(string text)` | Apply to all text frames |
| `int ReplaceTextWithEllipsis()` | Set `"..."` |
| `int ToggleSuperscript()` / `int ToggleSubscript()` | Font script toggles |

Private: `TryGetTextRange(Shape)` — handle shapes without text frame gracefully.

### WindowsUserSettingsStore

Read `Current.AddupDisplayMode` for Addup (field already on `UserSettings` in Core).  
**Не** требуется UI для смены mode в S09 — значение из JSON / default `"all"`.  
Optional: `SetAddupDisplayMode` stub for S10 Settings pane — only if needed for tests.

### HostScriptCommandMap + Ribbon

Extend `TryParse` for `TextCommands.IsTextCommand`.

Add **`grpText`**:

| Control id | Label | imageMso | Note |
|------------|-------|----------|------|
| `btnPasteUnformatted` | Paste Plain | PasteTextOnly | |
| `btnAddupTextFields` | Addup | CalculateNow | **Not** legacy `btnAddup` — map uses `btn{CommandIds}` |
| `btnReplaceWithEllipsis` | Ellipsis | … | Web-only in legacy; add for parity |
| `btnToggleSuperscript` | Superscript | FontSuperscript | |
| `btnToggleSubscript` | Subscript | FontSubscript | |

All `onAction="OnHostScriptCommand"`.

Update `PptPowerKeys.Windows/README.md` — Text section + manual QA.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/TextCommands.cs` | Command id guard |
| `Host/CommandRouter.cs` | Text + Addup handlers |
| `Host/IComHostAdapter.cs` + `ComHostAdapter.cs` | Text read/write, script toggles |
| `UI/HostScriptCommandMap.cs` | Include 5 commands |
| `UI/RibbonTab.xml` | `grpText` 5 buttons |
| `PptPowerKeys.Windows/README.md` | Docs + QA rows |
| `PptPowerKeys.Tests/TextCommandsTests.cs` (optional) | `IsTextCommand` guard |

**Core changes:** не требуются (NumberAggregator / AddupStatusFormatter уже в main).

## Анти-scope

- **PasteFormatted** (S10 unlock)
- Settings UI для Addup display mode (S10); только read JSON
- Sprint retrospective / goals / PRODUCT_CONTEXT (architect post-merge)
- Api HTTP calls from Windows line
- Rich clipboard / formatted paste

## Критерии приёмки (builder PR)

- [ ] All 5 text commands routed via `CommandRouter.Execute`
- [ ] PasteUnformatted: clipboard empty / no text frame errors match Web
- [ ] ReplaceWithEllipsis: exact `"..."` replacement
- [ ] Superscript/subscript mutual exclusivity
- [ ] Addup uses `NumberAggregator.Compute` + `AddupStatusFormatter.Format` in-process
- [ ] Addup respects `UserSettings.AddupDisplayMode` from store
- [ ] Ribbon 5 buttons wired (`btnAddupTextFields` not `btnAddup`)
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA note in PR (paste, ellipsis, super/sub, addup on `100 + 200`)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S09-001…005 Done
- Core Text tests in `PptPowerKeys.Tests` (NumberAggregator, AddupStatusFormatter)

## Reference (Web)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts` — pasteUnformattedText, replaceSelectedTextWithEllipsis, toggleSuperscript/Subscript, getSelectedShapeTexts
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Text cases + messages
- `src/PptPowerKeys.AddIn/src/text/addupStatus.ts` — message strings
- `src/PptPowerKeys.Core/Text/NumberAggregator.cs`
- `src/PptPowerKeys.Core/Text/AddupStatusFormatter.cs`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpText`

## Трассировка

Issue `#N` → `cursor/S09-006-text-addup-*` → PR `Closes #N`

---

## Architect post-merge (Sprint 09 close — **не builder**)

После merge PR S09-006 architect выполняет:

| # | Действие |
|---|----------|
| 1 | Backlog: S09-006 → **Done**, Issue закрыть |
| 2 | `sprints/sprint-09-ltsc-objects-format-text/retrospective.md` — итоги S09-001…006, метрики HostScript count |
| 3 | `goals.md` — DoD checkboxes `[x]` (30 HostScript wave; note 3 deferred S10) |
| 4 | `docs/PRODUCT_CONTEXT.md` — journal S09-006 + Sprint 09 complete |
| 5 | `sprints/README.md` — Sprint 09 **Done**, Sprint 10 **Next** |
| 6 | `sprints/epic-ltsc-windows-native/ROADMAP.md` — next step → S10 kickoff |

### Retrospective template (outline)

- **Milestone:** M3 Feature beta wave 1 (Objects/Format/Text HostScript on Windows)
- **Delivered:** S09-001…006 PRs, ~27 routable HostScript (+ 3 deferred: Regroup, FormatPainter, PasteFormatted)
- **Total Windows routed (after S09):** 38 layout (S08) + ~27 S09 ≈ **65** CommandRouter paths
- **Key decisions:** HostScript map pattern, COM adapters per domain, Core in-process (no Api on Windows line)
- **CI:** `dotnet test PptPowerKeys.sln` green; manual QA Windows sideload
- **Next:** Sprint 10 — Slides + None unlock + Settings UI

---

## Copy-paste промпт (новая сессия `/architect` — полный цикл)

```
/architect

Sprint 09 — S09-006 Text + Addup (последняя задача спринта, закрытие Sprint 09).
Прочитай:
- sprints/sprint-09-ltsc-objects-format-text/tasks/S09-006-text-addup.md
- sprints/sprint-09-ltsc-objects-format-text/goals.md
- sprints/sprint-09-ltsc-objects-format-text/backlog.md
- sprints/sprint-09-ltsc-objects-format-text/ARCHITECT-KICKOFF.md
- .github/review/CHECKLIST.md
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/Host/ComHostAdapter.cs
- src/PptPowerKeys.Windows/UI/HostScriptCommandMap.cs
- src/PptPowerKeys.Windows/Settings/WindowsUserSettingsStore.cs
- src/PptPowerKeys.Windows/Host/FormatColorCommands.cs (pattern)
- src/PptPowerKeys.Core/Text/NumberAggregator.cs
- src/PptPowerKeys.Core/Text/AddupStatusFormatter.cs
- src/PptPowerKeys.AddIn/src/office/powerpoint.ts (pasteUnformattedText, replaceSelectedTextWithEllipsis, toggleSuperscript/Subscript, getSelectedShapeTexts)
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (Text + Addup cases)
- src/PptPowerKeys.AddIn/src/text/addupStatus.ts
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpText)

S09-001…005 Done.

Шаг 1 — постановка builder:
Issue S09-006 → backlog In Progress → передай builder: /builder выполни S09-006

Шаг 2 — приёмка PR builder:
- Scope только S09-006 (5 text commands + ribbon grpText)
- dotnet test PptPowerKeys.sln green
- CHECKLIST.md
- Success/error messages match Web
- btnAddupTextFields (не btnAddup)

Шаг 3 — после merge (architect, не builder):
- backlog S09-006 Done
- retrospective.md
- goals.md DoD
- PRODUCT_CONTEXT journal (Sprint 09 complete)
- sprints/README + epic ROADMAP → Sprint 10 Next

PasteFormatted — anti-scope (S10).
```

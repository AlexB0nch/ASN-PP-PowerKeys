# S09-006 — Text commands + Addup (5 COM HostScript commands)

> Передача builder'у: `/builder выполни S09-006`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-006` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (`NumberAggregator`, `AddupStatusFormatter`) |
| **Статус** | In Progress |
| **Issue** | [#90](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/90) |
| **PR** | TBD |

## Цель

Реализовать **5 Text HostScript** на Windows line (parity с Web Add-in `powerpoint.ts`,
`runCommand.ts`, `addupStatus.ts`):

| CommandId | Web behavior | COM target |
|-----------|--------------|------------|
| PasteUnformatted | clipboard plain text → shapes | `Clipboard` + `TextFrame.TextRange.Text` |
| ReplaceWithEllipsis | text = `"..."` | `TextFrame.TextRange.Text` |
| ToggleSuperscript | toggle + disable subscript | `Font.Superscript` / `Font.Subscript` |
| ToggleSubscript | toggle + disable superscript | `Font.Subscript` / `Font.Superscript` |
| AddupTextFields | `NumberAggregator` + display mode | read texts + Core format |

**Закрывает Sprint 09:** 30 HostScript команд (19 Objects + 5 Format + 6 Text, из них 5 в scope; `PasteFormatted` → S10).

## Контекст (после S09-005)

| Компонент | Состояние |
|-----------|-----------|
| `NumberAggregator` | Core Done (S02) |
| `AddupStatusFormatter` | Core Done (S06-004) |
| `UserSettings.AddupDisplayMode` | Core + `WindowsUserSettingsStore` load/save ✓ |
| `CommandRouter` | Format Done; Text → `NotSupportedException` |
| Web spec | `powerpoint.ts` + `runCommand.ts` — эталон |

## Алгоритм (зафиксировано — match Web)

### PasteUnformatted

```
1. text = Clipboard.GetText() (plain); empty → error "Clipboard is empty."
2. shapes = selection; empty → error "Select one or more shapes first."
3. for each shape: try TextFrame.TextRange.Text = text; count successes
4. count == 0 → error "Selected shape(s) have no text frame to paste into."
5. return "Pasted plain text into {count} shape(s)."
```

### ReplaceWithEllipsis

```
1. shapes = selection; empty → error "Select one or more shapes first."
2. for each shape: try TextFrame.TextRange.Text = "..."
3. count == 0 → error "Selected shape(s) have no text to replace."
4. return "Replaced text with \"...\" on {count} shape(s)."
```

### ToggleSuperscript / ToggleSubscript

```
1. shapes = selection; empty → error "Select one or more shapes first."
2. for each shape with text: read Font.Superscript/Subscript (MsoTriState)
3. toggle target; when enabling one script, disable the other (mutual exclusion)
4. no text shapes → error "Selected shape(s) have no text to format."
5. return "Toggled superscript|subscript on {count} shape(s)."
```

### AddupTextFields

```
1. texts = ReadSelectedShapeTexts()  // empty string for shapes without text frame
2. stats = NumberAggregator.Compute(texts)
3. mode = settingsStore.Current.AddupDisplayMode
4. message = AddupStatusFormatter.Format(stats, mode)
5. return message (Changed=true always; "No numbers found in selection." is success path)
```

## Решения architect

### CommandRouter API

```csharp
if (TextCommands.IsTextCommand(command)) → ExecuteText(command)
```

### ComHostAdapter extensions

| Method | Returns | Behavior |
|--------|---------|----------|
| `ReadSelectedShapeTexts()` | `IReadOnlyList<string>` | Text per selected shape; `""` if no text frame |
| `PasteUnformattedText()` | count | Clipboard → selection text frames |
| `ReplaceSelectedTextWithEllipsis()` | count | Set `"..."` on text frames |
| `ToggleSuperscript()` | count | COM font superscript toggle |
| `ToggleSubscript()` | count | COM font subscript toggle |

### HostScriptCommandMap

Extend `TryParse` for Text commands. Ribbon control ids (convention `btn{CommandIds}`):

| Control id | CommandIds |
|------------|------------|
| `btnPasteUnformatted` | PasteUnformatted |
| `btnAddupTextFields` | AddupTextFields |
| `btnToggleSuperscript` | ToggleSuperscript |
| `btnToggleSubscript` | ToggleSubscript |

`ReplaceWithEllipsis` — CommandRouter / shortcuts only (no ribbon button; VSTO has no btn).

Optional alias: `btnAddup` → `AddupTextFields` (VSTO parity reference only).

### Ribbon grpText

Новая группа **Text** (`grpText`) после `grpFormat` — 4 кнопки (VSTO parity minus ellipsis):

- `btnPasteUnformatted` — Paste Plain
- `btnAddupTextFields` — Addup
- `btnToggleSuperscript` — Superscript
- `btnToggleSubscript` — Subscript

Reference: `PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (`grpText`).

### Tests (Linux CI)

- `TextCommandsTests` — command set helper
- `HostScriptCommandMapTests` — 4 new `btn*` cases
- Link new `.cs` files in `PptPowerKeys.Tests.csproj`
- Reuse existing `NumberAggregator` / `AddupStatusFormatter` tests (no Core changes expected)

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/TextCommands.cs` | Command set helper |
| `Host/IComHostAdapter.cs` | Text methods |
| `Host/ComHostAdapter.cs` | COM implementation |
| `Host/CommandRouter.cs` | Route 5 commands |
| `UI/HostScriptCommandMap.cs` | Parse 4 btn ids (+ optional btnAddup alias) |
| `UI/RibbonTab.xml` | grpText (4 buttons) |
| `PptPowerKeys.Tests/` | Tests + csproj link |
| `README.md` | Manual QA Text commands |

## Анти-scope

- **PasteFormatted** (S10)
- Api / AddIn changes (Web spec already Done)
- VstoLegacy edits (ribbon reference only)
- Core `NumberAggregator` / `AddupStatusFormatter` logic changes
- Settings UI for addup display mode (Web only; Windows reads persisted `AddupDisplayMode`)

## Критерии приёмки

- [ ] 5 commands routed via `CommandRouter.Execute`
- [ ] Messages match Web `runCommand.ts` / `AddupStatusFormatter`
- [ ] Addup respects `UserSettings.AddupDisplayMode` from `WindowsUserSettingsStore`
- [ ] Superscript/subscript mutual exclusion
- [ ] Clipboard empty / no selection / no text frame — понятные ошибки (match Web strings)
- [ ] Ribbon grpText (4 buttons) wired via `OnHostScriptCommand`; `btnAddupTextFields` maps to Addup
- [ ] Unit tests for `TextCommands` + map tests
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] PR: `cursor/s09-006-text-addup-c04a`, Task ID, `Closes #<issue>`

## Зависимости

- S02-003 Web Text commands (spec)
- S06-004 `AddupStatusFormatter` + `addupDisplayMode`
- S09-005 Done

## Reference files (Web spec)

- `src/PptPowerKeys.AddIn/src/office/powerpoint.ts`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`
- `src/PptPowerKeys.AddIn/src/text/addupStatus.ts`
- `src/PptPowerKeys.Core/Text/NumberAggregator.cs`
- `src/PptPowerKeys.Core/Text/AddupStatusFormatter.cs`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (grpText)

## Трассировка

Issue TBD → `cursor/s09-006-text-addup-c04a` → PR `Closes #<issue>`

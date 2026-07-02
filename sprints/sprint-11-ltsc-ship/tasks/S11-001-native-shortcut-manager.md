# S11-001 — Native ShortcutManager (keyboard hook, 76 cmd)

> Передача builder'у: `/builder выполни S11-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S11-001` |
| **Спринт** | `sprint-11-ltsc-ship` |
| **Epic** | LTSC Windows Native (Product Line B) — E7 Hotkeys |
| **Компонент** | `PptPowerKeys.Windows` + `PptPowerKeys.Core` (helpers) + Tests |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |

## Цель

Реализовать **native global keyboard hook** на Windows line: перехват chord'ов из `UserSettings.shortcuts` и исполнение через `CommandRouter.Execute` для **76 hotkey-eligible** команд (parity scope с Web `hotkeyEligibleCommandIds.ts`).

Settings pane (S10-004) уже редактирует JSON — этот task добавляет **runtime**, которого нет в `ThisAddIn`.

## Контекст (после Sprint 10)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 79/79 routed |
| `WindowsUserSettingsStore` | Load/Save `%AppData%/PptPowerKeys/UserSettings.json` |
| `SettingsPane` | Shortcuts grid + Save; **не** вызывает hotkey reload |
| Web line S06-001/002 | `Office.actions.associate` + `replaceShortcuts` — эталон поведения |
| `VstoLegacy/Core/ShortcutManager.cs` | Stub only (`Initialize`/`ReloadBindings` debug) — **не копировать** |

## Решения architect (зафиксировано)

### Hotkey-eligible scope

**76 CommandIds** — все из `CommandCatalog` кроме `ExecutionKind.Settings` (3):

- Exclude: `OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults`
- Include: все ServerLayout + HostScript (в т.ч. 9 None-unlock)

Добавить в Core (single source):

```csharp
// PptPowerKeys.Core/Commands/HotkeyEligibleCommandIds.cs
public static class HotkeyEligibleCommandIds
{
    public static IReadOnlyList<string> All =>
        CommandCatalog.All
            .Where(c => c.Value.Execution != ExecutionKind.Settings)
            .Select(c => c.Key.ToString())
            .ToList();
}
```

Mirror-test или unit test с count **76** (guard against catalog drift).

### Hook strategy

**When active:** только если foreground window принадлежит PowerPoint (`Application.HWND` / `GetForegroundWindow` check).

**Mechanism (builder выбирает, document in PR):**

- Preferred: `IMessageFilter` на UI thread **или** `WH_KEYBOARD` / `WH_KEYBOARD_LL` hook с marshal на UI thread
- **Не** `RegisterHotKey` per binding (не масштабируется на 76)

**On match:**

1. Parse chord via `ShortcutBindingValidator.NormalizeKeys` + new `ShortcutChordParser` (Core or Windows)
2. Lookup binding in current settings snapshot (case-insensitive command id)
3. `CommandRouter.Execute(commandId)` — swallow key if handled
4. Ignore if focus in modal dialog / text edit inside PP where native typing expected (document heuristics)

### ShortcutManager API

```csharp
public interface IShortcutManager : IDisposable
{
    void Initialize();
    void ReloadBindings();
}
```

Location: `src/PptPowerKeys.Windows/Host/ShortcutManager.cs`

Wire in `ThisAddIn_Startup` after `CommandRouter` created; `Dispose` on shutdown.

### Bindings source

At `Initialize` / `ReloadBindings`:

```csharp
var settings = _settingsStore.Current;
foreach (var binding in settings.Shortcuts)
{
    if (!HotkeyEligibleCommandIds.IsEligible(binding.CommandId)) continue;
    // register in internal dictionary keyed by normalized chord
}
```

Empty/invalid keys → skip (no throw).

Default bindings: `UserSettings.CreateDefaults()` (catalog `DefaultShortcut`) when file missing — already handled by store.

### Execution

- Reuse existing `CommandRouter` — **no duplicate** command switch
- Swallow only on successful route; unknown chord → pass through to PowerPoint
- Log `Debug.WriteLine` on execute (command id + keys) for manual QA

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Core/Commands/HotkeyEligibleCommandIds.cs` | New — eligible list + `IsEligible()` |
| `Core/Settings/ShortcutChordParser.cs` (or Windows/) | Parse `"Alt+1"`, `"Ctrl+Alt+B"` → key state |
| `Windows/Host/IShortcutManager.cs` | Interface |
| `Windows/Host/ShortcutManager.cs` | Hook + binding map + execute |
| `Windows/ThisAddIn.cs` | Init/dispose ShortcutManager |
| `Windows/PptPowerKeys.Windows.csproj` | New files |
| `Windows/README.md` | § Global hotkeys — behavior, QA steps |
| `Tests/HotkeyEligibleCommandIdsTests.cs` | Count 76, excludes Settings |
| `Tests/ShortcutChordParserTests.cs` | Parse/normalize cases |

## Анти-scope

- Settings pane Save → `ReloadBindings` wiring (**S11-002**)
- McKinsey/BCG profile switch live UX / conflict UI (**S11-002**)
- MSI / ClickOnce / signing (**S11-003**)
- Companion updater (**S11-004**)
- Full Office version QA matrix (**S11-005**)
- Api / AddIn changes
- VstoLegacy* edits
- Hotkeys when PowerPoint not foreground (global OS-wide — anti-scope unless architect revises)

## Критерии приёмки (builder PR)

- [ ] `ShortcutManager` initialized in `ThisAddIn`; disposed on shutdown
- [ ] 76 hotkey-eligible ids defined in Core; unit test guards count
- [ ] At least catalog default bindings work in manual QA (e.g. Alt+1 → AlignLeft when PP focused, 2+ shapes)
- [ ] Custom binding from existing JSON executes via `CommandRouter`
- [ ] Settings commands **not** registered as hotkeys
- [ ] Keys pass through when no binding matches
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] `PptPowerKeys.Windows/README.md` updated with hotkeys section
- [ ] PR notes: Windows + VS + VSTO manual QA matrix (PP version, steps)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- Sprint 10 Done — Settings pane + UserSettings JSON (S10-004)
- S10-005 optional for color OpenColorScheme binding — not required for S11-001

## Reference

- `src/PptPowerKeys.AddIn/src/runtime/hotkeyEligibleCommandIds.ts`
- `src/PptPowerKeys.AddIn/src/runtime/syncKeyboardShortcuts.ts`
- `sprints/sprint-06-keyboard-shortcuts/tasks/S06-001-shared-runtime-keyboard-shortcuts.md`
- `sprints/sprint-06-keyboard-shortcuts/tasks/S06-002-replace-shortcuts-user-settings.md`
- `src/PptPowerKeys.VstoLegacy/Core/ShortcutManager.cs` (stub)
- `docs/migration/04-powerpoint-ltsc-windows-native.md` — risk R4 keyboard conflicts

## Трассировка

Issue `#N` → `cursor/S11-001-native-shortcut-manager-*` → PR `Closes #N`

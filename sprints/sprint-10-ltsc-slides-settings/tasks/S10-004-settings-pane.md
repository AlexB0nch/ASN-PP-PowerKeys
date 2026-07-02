# S10-004 — Settings task pane + UserSettings (3 Settings commands)

> Передача builder'у: `/builder выполни S10-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-004` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |
| **Зависимости** | S10-001…003 Done |

## Цель

Реализовать **native Settings UI** на Windows line — WPF task pane + routing **3 Settings commands**.
После merge: **79/79** команд каталога маршрутизируются (`CommandRouter` или dedicated handler).

| CommandId | Поведение (parity Web `runCommand.ts`) |
|-----------|----------------------------------------|
| OpenShortcutManager | Показать task pane → scroll к секции Shortcuts |
| OpenColorScheme | Показать task pane → scroll к секции Color (placeholder до S10-005) |
| ResetToDefaults | `UserSettings.CreateDefaults()` → persist → refresh pane + ribbon |

## Контекст (после S10-003)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | **76** commands (32 layout + 44 host); Settings → `NotSupportedException` |
| `WindowsUserSettingsStore` | `SetSnapToGrid`, `GetRecentColors`, `RecordRecentColor`; **нет** full Save/Reset/Import |
| Web эталон | `SettingsPanel.tsx` + `UserSettingsImporter` (Core) |
| Core | `UserSettings`, `ConsultingProfilePresets`, `ShortcutBindingValidator`, `AddupStatusFormatter` |
| JSON path | `%AppData%/PptPowerKeys/UserSettings.json` (camelCase, Web export/import v1) |
| Ribbon | **Нет** `grpSettings`; snap checkbox только в Options (S08-002) |
| VSTO legacy | `grpSettings` reference (Shortcuts, Colors, Reset) |

## UX parity (match Web `SettingsPanel.tsx`)

### Task pane sections

| Section | Controls |
|---------|----------|
| **Profile** | Combo: Custom, McKinsey, BCG → `ConsultingProfilePresets.GetShortcuts(profile)` в editor (warning «Save to persist») |
| **General** | Snap to grid checkbox; Addup display mode (`all`/`sum`/`min`/`max`/`average`) |
| **Shortcuts** | Editable list: command title (from `CommandCatalog`) + keys; Add / Remove; duplicate-key warning (`ShortcutBindingValidator.FindDuplicateKeys`) |
| **Color** | Placeholder panel: «Smart Color Picker — S10-005» (scroll target for `OpenColorScheme`) |
| **Actions** | Save, Reset to defaults, Export JSON, Import JSON |

### Export / Import JSON

**Export** (match Web `buildSettingsExportPayload`):

```json
{
  "schemaVersion": 1,
  "profile": "Custom",
  "snapToGrid": false,
  "addupDisplayMode": "all",
  "shortcuts": [ { "commandId": "AlignLeft", "keys": "Alt+1" } ]
}
```

**Import:** `UserSettingsImporter.Import(json)` in-process (Core) → load into editor + show warnings → **Save** persists (как Web «Imported — click Save to persist»).

**Note:** `recentColors` остаётся в `UserSettings.json` on disk; export v1 **не обязан** включать recent (Web тоже не экспортирует) — не ломать import v1.

### Messages (Settings commands)

| Command | Success message |
|---------|-----------------|
| OpenShortcutManager | «Shortcut manager opened.» (или silent show pane) |
| OpenColorScheme | «Color scheme opened.» |
| ResetToDefaults | «Settings reset to defaults.» |

## Решения architect

### WPF Custom Task Pane

```
ThisAddIn.Startup
  → new SettingsTaskPaneHost (ElementHost + WPF SettingsPaneView)
  → CustomTaskPanes.Add(host, "PowerKeys Settings")
  → default Visible = false
```

- Folder: `UI/Settings/` — `SettingsPaneView.xaml` + code-behind (или MVVM light).
- **ElementHost** in WinForms `UserControl` — standard VSTO + WPF interop on .NET 4.8.
- Add refs: `PresentationFramework`, `PresentationCore`, `WindowsBase`, `WindowsFormsIntegration`.

### SettingsCommands + routing

New `Host/SettingsCommands.cs`:

```csharp
public static bool IsSettingsCommand(CommandIds command) =>
    command is CommandIds.OpenShortcutManager
        or CommandIds.OpenColorScheme
        or CommandIds.ResetToDefaults;
```

Extend `CommandRouter.Execute` **или** `PowerKeysRibbon.OnSettingsCommand` → `SettingsCommandHandler.Execute(command)`:

| Command | Handler |
|---------|---------|
| OpenShortcutManager | `SettingsTaskPane.Show(); ScrollToShortcuts()` |
| OpenColorScheme | `SettingsTaskPane.Show(); ScrollToColorSection()` |
| ResetToDefaults | `SettingsStore.ResetToDefaults(); Pane.Reload(); InvalidateRibbonSnap()` |

Return `CommandExecutionResult` with message (existing pattern).

### WindowsUserSettingsStore extensions

| Method | Behavior |
|--------|----------|
| `Save(UserSettings settings)` | Validate shortcuts via catalog; atomic write JSON |
| `ResetToDefaults()` | `UserSettings.CreateDefaults()` + persist |
| `ReplaceFromImport(UserSettings imported)` | Used after Import preview (optional — Save uses full settings from pane) |

Persist fields: `profile`, `snapToGrid`, `addupDisplayMode`, `shortcuts`, `recentColors` (preserve on Save if not edited).

**Sync:** Save snap → invalidate ribbon checkbox (`PowerKeysRibbon.InvalidateSnapControl`).

### Ribbon

Add **`grpSettings`** (VSTO parity):

| Control id | Label | imageMso | onAction |
|------------|-------|----------|----------|
| `btnOpenShortcutManager` | Shortcuts | KeyboardCustomization | OnSettingsCommand |
| `btnOpenColorScheme` | Colors | ThemeColorsGallery | OnSettingsCommand |
| `btnResetToDefaults` | Reset | Undo | OnSettingsCommand |

New `UI/SettingsCommandMap.cs` — parse `btn{CommandIds}` for 3 Settings ids.

### Tests (Linux CI)

| Test | Scope |
|------|-------|
| `SettingsCommandsTests` | `IsSettingsCommand` |
| `SettingsCommandMapTests` | 3 ribbon ids |
| Extend `WindowsUserSettingsStoreTests` | Save full settings, ResetToDefaults, round-trip JSON |
| Existing `UserSettingsImporterTests` | unchanged |

**Не** требуется WPF UI automation in CI.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `UI/Settings/SettingsPaneView.xaml(.cs)` | WPF settings UI |
| `UI/Settings/SettingsTaskPaneHost.cs` | ElementHost wrapper |
| `UI/Settings/SettingsTaskPaneController.cs` | Show/scroll/reload |
| `Host/SettingsCommands.cs` | Command guard |
| `Host/SettingsCommandHandler.cs` | Execute 3 settings cmds |
| `UI/SettingsCommandMap.cs` | Ribbon id parse |
| `Settings/WindowsUserSettingsStore.cs` | Save, Reset, full persist |
| `Host/CommandRouter.cs` | Settings branch **or** ribbon-only handler |
| `UI/PowerKeysRibbon.cs` | `OnSettingsCommand` |
| `UI/RibbonTab.xml` | `grpSettings` 3 buttons |
| `ThisAddIn.cs` | Register CustomTaskPane |
| `PptPowerKeys.Windows.csproj` | WPF refs + new files |
| `PptPowerKeys.Windows/README.md` | Settings pane QA |
| `PptPowerKeys.Tests/SettingsCommandsTests.cs` | Guard tests |

## Анти-scope

- **Full Color Picker panel** COM UI (S10-005) — только placeholder + scroll target
- **Global hotkey hook** / `replaceShortcuts` native (S11) — hint in pane: «Hotkeys saved; global hook in S11»
- WebView2 embed Web task pane
- Api / AddIn changes
- Consulting profile **editor** beyond preset apply (same as Web)
- `UserSettingsImporter` schema v2

## Критерии приёмки (builder PR)

- [ ] Custom task pane loads; Save/Reset/Export/Import work
- [ ] Profile presets McKinsey/BCG apply shortcuts in editor
- [ ] Snap + Addup mode persist; Addup command reads updated mode
- [ ] 3 Settings commands routed; ribbon `grpSettings` wired
- [ ] Import uses Core `UserSettingsImporter`; warnings shown
- [ ] Export JSON matches Web v1 shape (`schemaVersion: 1`)
- [ ] **79/79** catalog commands executable (no `NotSupportedException` for Settings)
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA in PR (VS sideload: open shortcuts, reset, import Web export file)
- [ ] `.github/review/CHECKLIST.md`

## Зависимости

- S10-001…003 Done (PR #93, #95, #97)
- Web Sprint 03–06 settings work (`SettingsPanel.tsx`, `UserSettingsImporter`)

## Reference

- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Settings cases
- `src/PptPowerKeys.Core/Settings/UserSettingsImporter.cs`
- `src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs`
- `src/PptPowerKeys.Core/Settings/ShortcutBindingValidator.cs`
- `docs/migration/04-powerpoint-ltsc-windows-native.md` (WPF task pane)
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` — `grpSettings`

## Трассировка

Issue `#N` → `cursor/S10-004-settings-pane-*` → PR `Closes #N`

## Architect post-merge (не builder)

- [ ] Backlog S10-004 → **Done**
- [ ] `docs/PRODUCT_CONTEXT.md` journal (79 commands routed milestone)
- [ ] `ARCHITECT-KICKOFF.md` → next S10-005
- [ ] Kickoff pointer in epic ROADMAP

---

## Copy-paste промпт (новая сессия `/architect` — полный цикл)

```
/architect

Sprint 10 — S10-004 Settings task pane + UserSettings (3 Settings commands).
S10-001…003 Done (#93, #95, #97 merged). 76 commands routed; Settings → NotSupportedException.

Прочитай:
- sprints/sprint-10-ltsc-slides-settings/tasks/S10-004-settings-pane.md
- sprints/sprint-10-ltsc-slides-settings/ARCHITECT-KICKOFF.md
- sprints/sprint-10-ltsc-slides-settings/backlog.md
- .github/review/CHECKLIST.md
- docs/migration/04-powerpoint-ltsc-windows-native.md (WPF task pane)
- src/PptPowerKeys.Windows/ThisAddIn.cs
- src/PptPowerKeys.Windows/Settings/WindowsUserSettingsStore.cs
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/UI/PowerKeysRibbon.cs
- src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx
- src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts (Settings cases)
- src/PptPowerKeys.Core/Settings/UserSettingsImporter.cs
- src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs
- src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml (grpSettings)

Шаг 1 — постановка builder:
Issue S10-004 → backlog In Progress → /builder выполни S10-004

Шаг 2 — приёмка PR builder:
- WPF CustomTaskPane: profile, shortcuts editor, snap, addup mode, Save/Reset/Export/Import
- OpenShortcutManager / OpenColorScheme / ResetToDefaults wired + ribbon grpSettings
- UserSettings.json camelCase; import via Core UserSettingsImporter
- Color section = placeholder (S10-005 anti-scope)
- dotnet test PptPowerKeys.sln green; CHECKLIST.md
- Milestone: 79/79 commands routed

Шаг 3 — после merge (architect):
- backlog Done, PRODUCT_CONTEXT journal, kickoff S10-005

Global hotkeys native hook — anti-scope (S11).
```

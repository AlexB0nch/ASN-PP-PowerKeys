# S10-004 — Settings task pane + UserSettings (3 Settings commands)

> Передача builder'у: `/builder выполни S10-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-004` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` + Core (reference) |
| **Статус** | In Progress |
| **Issue** | [#99](https://github.com/AlexB0nch/ASN-PP-PowerKeys/issues/99) |
| **PR** | — |

## Цель

Реализовать **WPF Settings task pane** с полным parity `UserSettings` (как Web `SettingsPanel.tsx`) и
маршрутизировать **3 Settings-команды** — последняя волна Sprint 10 до **79/79 routed**.

| CommandId | VSTO ribbon | Поведение |
|-----------|-------------|-----------|
| OpenShortcutManager | `btnShortcutManager` | Показать task pane, прокрутить к shortcuts |
| OpenColorScheme | `btnColorScheme` | Показать task pane, вкладка Colors (placeholder → S10-005) |
| ResetToDefaults | `btnResetDefaults` | `UserSettings.CreateDefaults()` + persist + reload UI |

## Контекст (после S10-003)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 76 commands routed; Settings → `NotSupportedException` |
| `WindowsUserSettingsStore` | `SnapToGrid`, `RecentColors` only; нет Save/Reset/Import |
| Ribbon | `grpOptions` snap checkbox; **нет** `grpSettings` |
| Task pane | **не реализован** |
| Web reference | `SettingsPanel.tsx`, `ShortcutManager.tsx`, `UserSettingsImporter` |

## Решения architect

### CommandRouter

- Новый `SettingsCommands.IsSettingsCommand` → `ExecuteSettings(command)`
- Делегирует в `ITaskPaneService` (show pane + panel-specific action)
- **Не** бросать `NotSupportedException` для 3 Settings ids

### WindowsUserSettingsStore (расширить)

| Метод | Назначение |
|-------|------------|
| `Save(UserSettings)` | Полная persist (profile, shortcuts, snapToGrid, addupDisplayMode) |
| `Reset()` | `UserSettings.CreateDefaults()` + write |
| `Import(string json)` | `UserSettingsImporter.Import` → return result (не persist до Save) |

JSON: camelCase, schema v1 compatible с Web export/import.

### WPF Custom Task Pane

- `Microsoft.Office.Tools.CustomTaskPane` + WPF `UserControl` через `ElementHost`
- Добавить refs: `PresentationFramework`, `PresentationCore`, `WindowsBase`, `WindowsFormsIntegration`
- Панель **Settings**: profile dropdown (`ConsultingProfilePresets`), snap checkbox, addup mode,
  shortcuts grid (command + keys + delete), Save / Reset / Export JSON / Import JSON
- Вкладка/секция **Colors**: placeholder «Color picker — S10-005» (без COM theme panel)
- `ITaskPaneService`: `ShowSettings()`, `ShowSettingsScrollToShortcuts()`, `ShowColorsPlaceholder()`, `ReloadFromStore()`

### Ribbon (VSTO parity)

Добавить `grpSettings` в `RibbonTab.xml` (после `grpOptions`):

```xml
<group id="grpSettings" label="Settings">
  <button id="btnShortcutManager" ... onAction="OnSettingsCommand" />
  <button id="btnColorScheme" ... onAction="OnSettingsCommand" />
  <button id="btnResetDefaults" ... onAction="OnSettingsCommand" />
</group>
```

`SettingsCommandMap.TryParse(controlId, out CommandIds)` — аналог `HostScriptCommandMap`.

### Snap-to-grid sync

Ribbon checkbox `chkSnapToGrid` и Settings pane checkbox читают/пишут один `WindowsUserSettingsStore`.
После Save в pane — `InvalidateControl("chkSnapToGrid")`.

### Global hotkeys

**Anti-scope** — ShortcutManager hook = S11. Pane только редактирует bindings в JSON.

## Scope builder

| Файл | Изменение |
|------|-----------|
| `Host/SettingsCommands.cs` (new) | `IsSettingsCommand` |
| `Host/CommandRouter.cs` | Route 3 Settings via `ITaskPaneService` |
| `Settings/WindowsUserSettingsStore.cs` | Save, Reset, Import |
| `Settings/ITaskPaneService.cs` (new) | Task pane contract |
| `UI/TaskPaneService.cs` (new) | CustomTaskPane lifecycle |
| `UI/SettingsPane.xaml` + `.xaml.cs` (new) | WPF settings UI |
| `UI/SettingsCommandMap.cs` (new) | Ribbon id → CommandIds |
| `UI/RibbonTab.xml` | `grpSettings` (3 buttons) |
| `UI/PowerKeysRibbon.cs` | `OnSettingsCommand` |
| `ThisAddIn.cs` | Wire store + task pane + router |
| `PptPowerKeys.Windows.csproj` | WPF refs + new files |
| `README.md` | Settings QA steps |
| `PptPowerKeys.Tests/*` | SettingsCommandsTests, SettingsCommandMapTests, store Save/Reset/Import tests |

## Анти-scope

- Color picker COM theme panel + consulting presets (**S10-005**)
- Global keyboard hook / ShortcutManager runtime (**S11**)
- Api / AddIn changes
- VstoLegacy*

## Критерии приёмки

- [ ] **79/79** commands routed — `CommandRouter.Execute` не бросает для Settings ids
- [ ] WPF task pane: profile, snap, addup mode, shortcuts editor, Save/Reset/Export/Import
- [ ] Import uses `UserSettingsImporter`; Export shape matches Web v1 (`schemaVersion`, camelCase)
- [ ] Ribbon `grpSettings`: Shortcuts, Colors, Reset → 3 Settings commands
- [ ] `OpenShortcutManager` scrolls to shortcuts; `ResetToDefaults` resets + reloads
- [ ] `OpenColorScheme` shows pane with Colors placeholder (full UI — S10-005)
- [ ] Unit tests + `dotnet test PptPowerKeys.sln` green
- [ ] PR with Task ID S10-004, `Closes #<issue>`

## Зависимости

- S10-003 Done (PR #97)

## Reference files

- `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx`
- `src/PptPowerKeys.AddIn/src/taskpane/ShortcutManager.tsx`
- `src/PptPowerKeys.Core/Settings/UserSettingsImporter.cs`
- `src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (`grpSettings`)
- `docs/migration/04-powerpoint-ltsc-windows-native.md`

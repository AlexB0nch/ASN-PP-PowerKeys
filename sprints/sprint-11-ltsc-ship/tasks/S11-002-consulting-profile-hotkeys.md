# S11-002 — Consulting profile → live hotkey bindings

> Передача builder'у: `/builder выполни S11-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S11-002` |
| **Спринт** | `sprint-11-ltsc-ship` |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Зависимость** | S11-001 merged |

## Цель

После **Save / Reset / profile change** в Settings pane — **live reload** hotkeys через `ShortcutManager.ReloadBindings()`. Parity с Web S06-002: McKinsey/BCG preset keys работают сразу после Save без перезапуска PowerPoint.

## Scope

| Файл | Изменение |
|------|-----------|
| `UI/SettingsPane.xaml.cs` | After Save, Reset, profile apply → `ReloadBindings()` |
| `Settings/WindowsUserSettingsStore.cs` | Optional callback/event `SettingsChanged` |
| `ThisAddIn.cs` | Wire store → ShortcutManager |
| `Windows/README.md` | Profile hotkeys QA (McKinsey Alt+D, BCG Alt+Shift+D) |
| Tests | Settings save triggers reload (mock `IShortcutManager`) |

## Критерии приёмки

- [ ] Save shortcuts → new binding active without PP restart
- [ ] Profile McKinsey → Save → Alt+D = DuplicateRight (if bound)
- [ ] Profile BCG → Save → Ctrl+Alt+B = SameWidth
- [ ] Reset to defaults → hotkeys match catalog defaults
- [ ] Basic conflict note in UI or debug log when duplicate chords (stretch: MessageBox warning)
- [ ] `dotnet test` green; manual QA in PR

## Анти-scope

- MSI (S11-003); full conflict resolution UI (defer beyond warning)

## Reference

- `ConsultingProfilePresets.cs`
- Web `syncKeyboardShortcuts.ts`
- S06-002 task file

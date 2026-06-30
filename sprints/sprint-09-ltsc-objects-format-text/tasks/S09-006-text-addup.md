# S09-006 — Text commands + Addup (6 HostScript commands)

> Передача builder'u: `/builder выполни S09-006`  
> **Последняя задача Sprint 09** — после merge architect закрывает спринт.

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S09-006` |
| **Спринт** | `sprint-09-ltsc-objects-format-text` |
| **Компонент** | `PptPowerKeys.Windows` + Core |
| **Статус** | Todo |
| **Зависимости** | S09-005 Done |

## Цель

| CommandId | Behavior |
|-----------|----------|
| PasteUnformatted | Clipboard paste text only into selection text frames |
| ReplaceWithEllipsis | Truncate text with ellipsis (match Web rules) |
| ToggleSuperscript | COM Font.Superscript toggle |
| ToggleSubscript | COM Font.Subscript toggle |
| AddupTextFields | Core `NumberAggregator` on selected text → status message |

**PasteFormatted — anti-scope** (Web None → S10).

## Алгоритм Addup (match Web)

```
1. texts = ReadSelectedShapeTexts()
2. stats = NumberAggregator.Aggregate(texts)
3. mode = UserSettings.AddupDisplayMode (or default)
4. message = AddupStatusFormatter.Format(stats, mode)
```

Display mode from `WindowsUserSettingsStore` (compatible with Web export JSON).

## Решения architect

- `TextCommands.cs` + `ComHostAdapter.ReadSelectedShapeTexts()`.
- Clipboard: `Clipboard.GetText()` for PasteUnformatted.
- Ribbon text group buttons.
- Sprint close (architect post-merge): `retrospective.md`, goals DoD, PRODUCT_CONTEXT journal.

## Критерии приёмки

- [ ] 5 text commands (+ Addup) routed
- [ ] NumberAggregator used in-process; message matches Web formatter
- [ ] Addup respects settings display mode
- [ ] Ribbon wired
- [ ] `dotnet test PptPowerKeys.sln` green
- [ ] Manual QA note in PR

## Reference

- `src/PptPowerKeys.Core/Text/NumberAggregator.cs`
- `src/PptPowerKeys.Core/Text/AddupStatusFormatter.cs`
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — Text cases

## Architect post-merge (Sprint 09 close)

- [ ] `retrospective.md` for sprint-09
- [ ] All S09-001…006 Done in backlog
- [ ] Kickoff pointer → Sprint 10 in epic ROADMAP
- [ ] PRODUCT_CONTEXT journal

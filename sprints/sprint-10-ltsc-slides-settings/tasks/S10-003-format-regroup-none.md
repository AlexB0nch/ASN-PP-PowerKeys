# S10-003 — None unlock: FormatPainter, PasteFormatted, Regroup (3 COM commands)

> Передача builder'у: `/builder выполни S10-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S10-003` |
| **Спринт** | `sprint-10-ltsc-slides-settings` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | `PptPowerKeys.Windows` |
| **Статус** | Todo |
| **Issue** | — |
| **PR** | — |

## Цель

Реализовать **3 OfficeJs None unlocks** на Windows line — команды, которые на Web деградируют
(`unsupportedWebCommands.ts`), но доступны через COM на Desktop:

| CommandId | VSTO ribbon | COM target |
|-----------|-------------|------------|
| FormatPainter | `btnFormatPainter` | `Shape.PickUp` / `Shape.Apply` |
| PasteFormatted | — | Rich clipboard paste into text frame |
| Regroup | — | `ShapeRange.Regroup()` |

## Контекст (после S10-002)

| Компонент | Состояние |
|-----------|-----------|
| `CommandRouter` | 73 commands routed |
| Web | None → explicit degradation messages |
| Windows | Format group has Fill/Line/Text only; extend for FormatPainter |

## Решения architect

### CommandRouter

- Новый `FormatRegroupCommands.IsFormatRegroupCommand` → `ExecuteFormatRegroup(command)`
- Каждая команда — прямой COM host script (без Core layout)

### Ribbon

- Расширить **Format** (`grpFormat`): `btnFormatPainter` (VSTO parity, `imageMso="FormatPainter"`)
- PasteFormatted, Regroup — CommandRouter + shortcuts (no VSTO ribbon btn)

### Tests

- `FormatRegroupCommandsTests` + `HostScriptCommandMapTests` для `btnFormatPainter`

## Анти-scope

- Settings UI (S10-004)
- Color picker panel (S10-005)
- Api / AddIn changes

## Критерии приёмки

- [ ] 3 commands routed via `CommandRouter.Execute`
- [ ] COM behavior matches VSTO / PowerPoint Desktop expectations
- [ ] Ribbon button `btnFormatPainter` in `grpFormat`
- [ ] Unit tests + `dotnet test PptPowerKeys.sln` green
- [ ] PR with Task ID S10-003

## Reference files

- `src/PptPowerKeys.AddIn/src/taskpane/unsupportedWebCommands.ts`
- `src/PptPowerKeys.VstoLegacy/UI/RibbonTab.xml` (btnFormatPainter)
- `sprints/epic-ltsc-windows-native/FEATURE_PARITY.md`
- `docs/migration/01-vsto-to-officejs-mapping.md` (FormatPainter, PasteFormatted, Regroup)

# S07-001 — Core multitarget netstandard2.0

> Передача builder'у: `/builder выполни S07-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S07-001` |
| **Спринт** | `sprint-07-ltsc-foundation` |
| **Epic** | LTSC Windows Native (Product Line B) |
| **Компонент** | Core + Tests |
| **Статус** | Done |

## Цель

`PptPowerKeys.Core` должен собираться как **`netstandard2.0`** (для VSTO .NET Framework 4.8 host) **и**
**`net8.0`** (текущий Api/Tests) без регрессии CI.

## Контекст

- ADR-001: `PptPowerKeys.Windows` ссылается на Core in-process.
- VSTO host = .NET Framework 4.8 → нужен netstandard2.0 (или net472) build Core.
- Api/Tests остаются на net8.0.

## Scope

| Item | Detail |
|------|--------|
| csproj | `<TargetFrameworks>net8.0;netstandard2.0</TargetFrameworks>` |
| APIs | Условная компиляция если нужно (#if NET8_0); netstandard2.0 без net8-only deps |
| Tests | `PptPowerKeys.Tests` остаётся net8.0; все **143** tests pass |
| Docs | Комментарий в csproj / ADR cross-link |

## Анти-scope

- PptPowerKeys.Windows project (S07-002)
- COM code
- Изменение публичного API CommandCatalog без need
- VstoLegacy

## Критерии приёмки

- [x] `dotnet test PptPowerKeys.sln` зелёный
- [x] `dotnet build src/PptPowerKeys.Core/PptPowerKeys.Core.csproj -f netstandard2.0` успешен
- [x] `dotnet build src/PptPowerKeys.Core/PptPowerKeys.Core.csproj -f net8.0` успешен
- [x] Api project still references net8.0 Core build
- [x] No breaking changes to Web Add-in

## Зависимости

- ADR-001 merged / accepted

## Затрагиваемые файлы (ожидаемо)

- `src/PptPowerKeys.Core/PptPowerKeys.Core.csproj`
- Possibly conditional code in Core if net8-only APIs used

## Трассировка

Issue `#N` → `cursor/S07-001-core-multitarget-netstandard-*` → PR `Closes #N`

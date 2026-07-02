# S11-003 — MSI / ClickOnce + code signing pipeline

> Передача builder'u: `/builder выполни S11-003`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S11-003` |
| **Спринт** | `sprint-11-ltsc-ship` |
| **Компонент** | setup + docs + CI notes |
| **Статус** | Todo |
| **Зависимость** | S11-002 merged (or parallel if no hotkey coupling) |

## Цель

**IT-deployable installer** для `PptPowerKeys.Windows`: MSI (preferred) or ClickOnce, code signing story, silent install flags, uninstall clean.

## Scope (architect may refine in Issue)

| Deliverable | Описание |
|-------------|----------|
| Setup project | WiX / VS Setup Project under `src/PptPowerKeys.Windows.Setup/` or documented VS Publish |
| Signing | Authenticode cert instructions; CI placeholder (secrets not in repo) |
| Output | `.msi` installs VSTO add-in + registry for PP load |
| Docs | `docs/migration/07-windows-ltsc-deploy-msi.md` — IT admin runbook |
| Prerequisites | VSTO Runtime, .NET Framework 4.8 checklist |

## Критерии приёмки

- [ ] Clean Windows VM: install MSI → PowerPoint shows PowerKeys ribbon
- [ ] Uninstall removes add-in
- [ ] Signing documented (self-sign dev + prod cert process)
- [ ] Version bump / product code strategy documented
- [ ] No regression to `dotnet test PptPowerKeys.sln`

## Анти-scope

- Azure/signing service automation (optional follow-up)
- Mac/Web deploy

## Reference

- `docs/migration/04-powerpoint-ltsc-windows-native.md` — delivery MSI
- `docs/migration/05-ltsc-web-addin-central-deploy.md` — Track 0 fallback
- ADR-001 risk R1 IT blocks VSTO

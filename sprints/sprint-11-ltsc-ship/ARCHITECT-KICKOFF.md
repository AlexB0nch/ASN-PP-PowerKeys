# Architect Kickoff — Sprint 11 LTSC Ship (Hotkeys · MSI · QA)

> **Статус:** Planned → **Next** после закрытия Sprint 10 (S10-005 merged, M3 Feature beta).
> **Milestone:** **M4 Production** — signed MSI, global hotkeys, QA matrix, LTSC runbook, Product Line B v1.0.

## Контекст

- Sprint 01–06 (Web Add-in) **Done** — 79 cmd, Shared Runtime + `replaceShortcuts` (76 hotkey-eligible).
- Epic LTSC S07–S10: **79/79 commands routed** на `PptPowerKeys.Windows`; Settings WPF pane + shortcuts editor (S10-004); color picker COM (S10-005).
- **Gap:** Settings pane сохраняет bindings в `%AppData%/PptPowerKeys/UserSettings.json`, но **глобальный runtime hotkeys отсутствует** (`ThisAddIn` не инициализирует ShortcutManager).
- **Gap:** нет signed MSI / IT deployment pack; manual QA только по фрагментам в README.
- Linux CI: `dotnet test PptPowerKeys.sln` green; Windows/VSTO сборка — VS2022 + VSTO workload (вне Linux CI).

## Цель Sprint 11

**Production-ready Product Line B:** native keyboard hook для 76 hotkey-eligible команд, live sync с UserSettings, signed installer, QA matrix Office 2019 / 2021 LTSC / 2024 LTSC, release + IT runbook.

| Wave | Задачи |
|------|--------|
| Hotkeys infra | S11-001 ✅ first |
| Profile → live bindings | S11-002 |
| Installer + signing | S11-003 |
| Companion (optional) | S11-004 |
| QA + release | S11-005 ← last (закрытие спринта + epic M4) |

## Задачи спринта

| ID | Файл | Builder |
|----|------|---------|
| S11-001 | [`tasks/S11-001-native-shortcut-manager.md`](./tasks/S11-001-native-shortcut-manager.md) | `/builder выполни S11-001` |
| S11-002 | [`tasks/S11-002-consulting-profile-hotkeys.md`](./tasks/S11-002-consulting-profile-hotkeys.md) | после merge 001 |
| S11-003 | [`tasks/S11-003-msi-signing-pipeline.md`](./tasks/S11-003-msi-signing-pipeline.md) | после merge 002 |
| S11-004 | [`tasks/S11-004-companion-updater.md`](./tasks/S11-004-companion-updater.md) | optional / параллельно 003 |
| S11-005 | [`tasks/S11-005-qa-matrix-release.md`](./tasks/S11-005-qa-matrix-release.md) | `/builder выполни S11-005` (last) |

## Инварианты

- Web AddIn `hotkeyEligibleCommandIds.ts` + S06-002 — **эталон scope** (76 = 79 − 3 Settings).
- Settings-команды (`OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults`) — **не** hotkey-eligible.
- `CommandRouter.Execute` — единая точка исполнения (как ribbon / task pane).
- `ConsultingProfilePresets` (Core) — McKinsey/BCG keys; Custom — user bindings only.
- **Не** размораживать `VstoLegacy*` (ShortcutManager stub — reference only).
- Core changes must keep `dotnet test PptPowerKeys.sln` green.

## Процесс сессии (S11 — закрытие epic M4)

1. Убедиться Sprint 10 **Done** (S10-005 merged, retrospective exists).
2. Issue **S11-001** → backlog **In Progress**
3. `/builder выполни S11-001` → architect приёмка → merge → Done
4. Повторить **S11-002…005** (S11-004 optional — architect решает skip/defer в Issue)
5. **Sprint 11 close:** `retrospective.md`, goals DoD, `PRODUCT_CONTEXT` (M4 + Line B v1.0), epic ROADMAP Done, tag `windows-v1.0` (или agreed semver)

## Copy-paste промпт (новая сессия — kickoff S11-001)

```
/architect

Sprint 11 — LTSC Ship (Hotkeys · MSI · QA). Первая задача S11-001.
Зависимость: Sprint 10 Done (S10-005 merged, M3 Feature beta, 79/79 routed).

Прочитай:
- sprints/sprint-11-ltsc-ship/ARCHITECT-KICKOFF.md
- sprints/sprint-11-ltsc-ship/tasks/S11-001-native-shortcut-manager.md
- sprints/sprint-11-ltsc-ship/goals.md
- sprints/sprint-11-ltsc-ship/backlog.md
- sprints/epic-ltsc-windows-native/ROADMAP.md
- sprints/epic-ltsc-windows-native/FEATURE_PARITY.md (S11 row)
- .github/review/CHECKLIST.md
- docs/migration/04-powerpoint-ltsc-windows-native.md (§ S11, risks R4)
- docs/migration/03-powerpoint-desktop-windows.md (Web hotkeys reference)
- src/PptPowerKeys.AddIn/src/runtime/hotkeyEligibleCommandIds.ts
- src/PptPowerKeys.AddIn/src/runtime/syncKeyboardShortcuts.ts
- src/PptPowerKeys.Windows/ThisAddIn.cs
- src/PptPowerKeys.Windows/Host/CommandRouter.cs
- src/PptPowerKeys.Windows/UI/SettingsPane.xaml.cs
- src/PptPowerKeys.Core/Settings/ConsultingProfilePresets.cs
- src/PptPowerKeys.Core/Settings/ShortcutBindingValidator.cs
- src/PptPowerKeys.VstoLegacy/Core/ShortcutManager.cs (stub reference only)

Шаг 1 — постановка builder:
- Проверь Sprint 10 closed; если нет — остановись, kickoff S11 после S10-005
- Issue S11-001 → backlog In Progress → /builder выполни S11-001

Шаг 2 — приёмка PR builder (S11-001):
- Native ShortcutManager: hook/filter, 76 eligible bindings, CommandRouter.Execute
- ReloadBindings API; only when PowerPoint foreground
- dotnet test green; Windows README hotkeys section; CHECKLIST.md
- Manual QA note (Alt+1 align, custom binding after Save — может потребовать S11-002)

Шаг 3 — после S11-001 merge:
- backlog Done → Issue S11-002 → /builder выполни S11-002
- Далее S11-003…005 по backlog; S11-004 optional
- После S11-005 merge: retrospective.md, goals DoD, PRODUCT_CONTEXT (M4), epic close, release tag

Anti-scope S11-001: MSI (S11-003), Companion (S11-004), full QA matrix (S11-005).
```

## Copy-paste промпт (S11-005 — закрытие Sprint 11 + M4)

```
/architect

Sprint 11 — S11-005 QA matrix + release (последняя задача спринта, M4 Production).
S11-001…004 Done (или S11-004 skipped).

Прочитай:
- sprints/sprint-11-ltsc-ship/tasks/S11-005-qa-matrix-release.md
- sprints/sprint-11-ltsc-ship/goals.md
- src/PptPowerKeys.Windows/README.md

Issue S11-005 → backlog In Progress → /builder выполни S11-005 → приёмка → merge.

После merge (architect):
- retrospective.md, goals DoD [x], PRODUCT_CONTEXT (Line B v1.0, M4)
- sprints/README Sprint 11 Done
- sprints/epic-ltsc-windows-native/ROADMAP.md epic M4 complete
- Release tag + IT deployment pack verified
```

# Кикофф для architect — Sprint 06 (глобальные шорткаты)

> **Роль этой сессии:** планирование Sprint 06, декомпозиция `S06-0YY`, постановка **S06-001** builder'у.
> Sprint 05 Done — `sprint-05-advanced-features/retrospective.md`. PR #43 (desktop runbook) merged.

## 1. Где мы (2026-06-29)

| Метрика | Значение |
|---------|----------|
| Команды | **79** в `CommandCatalog` |
| Тесты | **114** `dotnet test` |
| Исполнение | `runCommand(descriptor)` — ServerLayout / HostScript / Settings |
| Shortcut Manager | Bindings в `UserSettings` через API; **не** регистрирует клавиши в PowerPoint |
| UX сегодня | Клик в task pane; Settings hint: hotkeys not active yet (PR #43) |
| Манифест | `VersionOverrides V1_0`, **без** Shared Runtime; taskpane + `commands.html` — раздельные entry |

**Целевая платформа hotkeys:** PowerPoint Desktop **Windows 2601+** (`KeyboardShortcuts 1.1` + `SharedRuntime 1.1`).

**Web / Mac / старый PP:** graceful degradation — task pane как сейчас.

## 2. Feasibility (зафиксировано architect)

| Тема | PP Desktop Win 2601+ | PowerPoint Web | Mac Desktop |
|------|----------------------|----------------|-------------|
| Shared Runtime | Required | Supported (task pane load) | Supported |
| Keyboard Shortcuts API | **Full** (PP build ≥ 19628.20150) | Limited / MS tables vary | PP 16.105+ per MS docs |
| `replaceShortcuts` (UserSettings sync) | Full | N/A in S06-001 | Deferred S06-002 |
| Settings commands hotkeys | Skip in S06-001 (`ExecutionKind.Settings`) | Skip | Skip |

Источники:
- [`docs/migration/03-powerpoint-desktop-windows.md`](../../docs/migration/03-powerpoint-desktop-windows.md) § «Глобальные шорткаты»
- [Keyboard shortcuts for Office Add-ins](https://learn.microsoft.com/en-us/office/dev/add-ins/design/keyboard-shortcuts)
- [Configure shared runtime](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/configure-your-add-in-to-use-a-shared-runtime)

## 3. Декомпозиция Sprint 06

| Приоритет | ID | Тема | Rationale |
|-----------|-----|------|-----------|
| **P1** | **S06-001** | Shared Runtime + Keyboard Shortcuts (Tier 1) | Инфраструктура + catalog default shortcuts (~12–14 cmd) |
| **P1** | **S06-002** | `replaceShortcuts` ↔ UserSettings (все bindings) | Shortcut Manager Save → live hotkeys; `areShortcutsInUse` |
| **P2** | S06-003 | Import/export settings JSON | README stretch (deferred S05) |
| **P2** | S06-004 | Object Statistics MIN/MAX/AVG UI | Addup уже в Core |
| **P3** | S06-005 | Color Picker eyedropper / HEX | deferred S04 |

**Минимальный DoD Sprint 06:** S06-001 + S06-002.

## 4. Tier 1 shortcuts (S06-001 baseline)

**Action ID:** `actionId === CommandId` (строка `"AlignLeft"`, …).

### Из `CommandCatalog.DefaultShortcut` (12 команд)

| CommandId | Keys | Execution |
|-----------|------|-----------|
| AlignLeft … DistributeVertical | Alt+1 … Alt+8 | ServerLayout |
| SameWidth, SameHeight | Alt+B, Alt+H | ServerLayout |
| FillColor | Alt+G | HostScript |
| ToggleZoom | F1 | HostScript (`support=None` → unsupported path OK) |

### Consulting preset extras (McKinsey, non-conflicting, **не** Settings)

| CommandId | Keys | Note |
|-----------|------|------|
| DuplicateRight | Alt+D | McKinsey preset |
| AddupTextFields | Alt+A | McKinsey preset |

**Не включать в S06-001:** `OpenColorScheme` (Alt+L), `OpenShortcutManager`, `ResetToDefaults` — `ExecutionKind.Settings`.

**BCG-only keys** (Ctrl+Alt+*, Alt+Shift+D) — в S06-002 через `replaceShortcuts` при выборе профиля.

## 5. Архитектурные решения (S06-001)

1. **Shared runtime:** один URL (`taskpane.html` или unified bootstrap); `FunctionFile` → тот же runtime.
2. **Manifest:** `SharedRuntime 1.1` + `KeyboardShortcuts 1.1` в `<Requirements>`; `<ExtendedOverrides Url="…/shortcuts.json">`.
3. **shortcuts.json:** генерируется `scripts/build-shortcuts.mjs` из Tier 1; prod URL = GitHub Pages.
4. **Handler:** `Office.actions.associate(commandId, async (event) => { await executeCommandById(commandId); event.completed(); })` — thin wrapper над `runCommand`.
5. **Feature detection:** `Office.context.requirements.isSetSupported('KeyboardShortcuts', '1.1')` — иначе no-op, без crash на Web.
6. **Settings MessageBar:** «Hotkeys active on PowerPoint Desktop Windows (version 2601+). On Web, use task pane buttons.»

## 6. Риски → builder

| Риск | Митигация |
|------|-----------|
| `VersionOverrides V1_0` + shared runtime | Изучить MS sample + [configure shared runtime](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/configure-your-add-in-to-use-a-shared-runtime) до кодирования; возможен `VersionOverrides 1.1` |
| Alt+1… конфликт с native PP | MS conflict dialog; задокументировать в PR |
| Web regression (S01-008) | `npm run validate:prod` + manual note; не ломать task pane load |
| Дублирование `runCommand` | Единый `executeCommandById` → cached catalog + existing `runCommand` |

## 7. Процесс этой сессии

1. [x] PR #43 merged — `03-powerpoint-desktop-windows.md` в main
2. [x] `goals.md` / `backlog.md` / task S06-001
3. [x] GitHub Issue #N → backlog In Progress
4. [ ] `/builder` S06-001 → PR → architect приёмка → merge
5. [ ] S06-002 постановка (после merge S06-001)

## 8. Инварианты

- `ShapeBounds` boundary; anchor = last selected
- `runCommand` — единый executor; hotkeys только trigger
- Api↔AddIn sync; 79 команд без новых CommandIds
- `VstoLegacy*` frozen

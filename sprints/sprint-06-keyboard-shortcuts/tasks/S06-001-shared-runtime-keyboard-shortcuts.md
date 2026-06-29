# S06-001 — Shared Runtime + Keyboard Shortcuts (Tier 1)

> Передача builder'у: `/builder выполни S06-001`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-001` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | AddIn + manifest + scripts (+ optional Core Tests) |
| **Статус** | Done |
| **Issue** | #44 (closed) |
| **PR** | #46 |

## Цель

На **PowerPoint Desktop Windows 2601+** зарегистрировать глобальные шорткаты для команд **Tier 1**
(команды с `DefaultShortcut` в `CommandCatalog` + consulting preset keys без конфликта) через официальный
Office **Keyboard Shortcuts API**. На остальных платформах — **без регрессии** (task pane only, no crash).

## Контекст

- S03 Done: Shortcut Manager хранит bindings в `UserSettings`; **не** регистрирует клавиши.
- S05 Done: McKinsey/BCG presets в Core (`ConsultingProfilePresets`).
- Сейчас: `webpack` entry `taskpane` + `commands` (раздельные HTML); manifest `VersionOverrides V1_0`, **без** Shared Runtime.
- Единая точка исполнения: `runCommand(descriptor)` в `src/taskpane/runCommand.ts`.
- PR #43 merged: `docs/migration/03-powerpoint-desktop-windows.md` описывает текущее ограничение и целевой путь A.

## Решения architect (зафиксировано)

### Action ID

`actionId === CommandId` — строка enum имени: `"AlignLeft"`, `"DuplicateRight"`, …

### Tier 1 command set (минимум 14 shortcuts)

**A. Все entries `CommandCatalog` с non-null `DefaultShortcut` (12):**

| CommandId | Keys | Execution |
|-----------|------|-----------|
| AlignLeft | Alt+1 | ServerLayout |
| AlignCenterHorizontal | Alt+2 | ServerLayout |
| AlignRight | Alt+3 | ServerLayout |
| AlignTop | Alt+4 | ServerLayout |
| AlignMiddleVertical | Alt+5 | ServerLayout |
| AlignBottom | Alt+6 | ServerLayout |
| DistributeHorizontal | Alt+7 | ServerLayout |
| DistributeVertical | Alt+8 | ServerLayout |
| SameWidth | Alt+B | ServerLayout |
| SameHeight | Alt+H | ServerLayout |
| FillColor | Alt+G | HostScript |
| ToggleZoom | F1 | HostScript (`support=None` → `runUnsupportedWebCommand` path) |

**B. McKinsey consulting preset keys, не конфликтующие с (A), не Settings (2):**

| CommandId | Keys |
|-----------|------|
| DuplicateRight | Alt+D |
| AddupTextFields | Alt+A |

**Не регистрировать в S06-001:**

- `ExecutionKind.Settings`: `OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults`
- BCG-only keys (`Ctrl+Alt+*`, `Alt+Shift+D`) — S06-002 через `replaceShortcuts`

### Handler pattern

```typescript
Office.actions.associate(commandId, async (event) => {
  await executeCommandById(commandId);
  event.completed();
});
```

`executeCommandById(id)` — loads descriptor from **cached catalog** (already fetched in task pane bootstrap),
calls existing `runCommand(descriptor, settingsActions, layoutOptions)`. **Не дублировать** switch/cases из `runCommand`.

`settingsActions` и `layoutOptions` (snapToGrid) должны быть доступны из shared bootstrap — вынести в модуль,
который и React task pane, и shortcut handlers используют совместно.

### Shared Runtime

1. **Manifest** (`manifest.template.xml` + regen `manifest.prod.xml`):
   - `<Requirements>`: `SharedRuntime 1.1`, `KeyboardShortcuts 1.1`
   - `<Runtimes>`: один shared runtime (`lifetime`: `long` per MS guidance for shortcuts + task pane)
   - Task pane + Function file → **один runtime URL** (refactor webpack или unified bootstrap)
   - Root-level `<ExtendedOverrides Url="{{ADDIN_BASE_URL}}/shortcuts.json">` (или template placeholder)
2. **shortcuts.json** (MS extended-manifest schema):
   - `actions[]`: `{ id, type: "ExecuteFunction", name }` — `id` = CommandId, `name` = catalog title
   - `shortcuts[]`: `{ action, key: { default: "Alt+1" } }` — Office format (modifier+key; verify MS guidelines)
3. **Build:** `scripts/build-shortcuts.mjs` — input Tier 1 CommandIds + keys from catalog + McKinsey extras;
   output `dist/shortcuts.json`; prod URL via `ADDIN_BASE_URL` in manifest (like `build-manifest.mjs`)
4. **Dev:** shortcuts.json served from `localhost:3000` (webpack copy or devServer static)
5. **CI:** `npm run build:prod` генерирует `shortcuts.json`; optional drift check in CI (nice-to-have)

### Feature detection / degradation

```typescript
if (Office.context.requirements.isSetSupported("KeyboardShortcuts", "1.1")) {
  registerCommandActions(); // associate Tier 1
}
// else: no-op — task pane works unchanged
```

- **Web / старый PP:** hotkeys не активны; **не** вызывать `associate` без detection → no crash
- **Settings MessageBar** (`SettingsPanel.tsx`): заменить текст на:
  > Hotkeys active on PowerPoint Desktop Windows (version 2601+). On Web, use task pane buttons.

## Scope

### 1. Manifest migration

- `manifest.template.xml`: SharedRuntime + KeyboardShortcuts requirements, Runtimes, ExtendedOverrides
- `scripts/build-manifest.mjs`: substitute `shortcuts.json` URL if templated
- Regenerate `manifest.prod.xml`
- `npm run validate:prod` — зелёный

### 2. Webpack / runtime bootstrap

- Unified shared runtime entry OR refactor so `taskpane.html` loads bootstrap that:
  - `Office.onReady` → feature detection → `registerCommandActions()` → mount React (`App`)
- Preserve `commands.html` compatibility if manifest still references FunctionFile (может указывать на тот же URL)
- Copy `shortcuts.json` to `dist/` on build

### 3. `scripts/build-shortcuts.mjs`

- Read Tier 1 from static list mirroring architect table OR parse from shared JSON source of truth
- Map keys: `Alt+1` → Office format per [MS guidelines](https://learn.microsoft.com/en-us/office/dev/add-ins/design/keyboard-shortcuts)
- Optional: Core `ShortcutKeyFormatter` for normalize + 2–4 unit tests (если логика non-trivial)

### 4. Execution module

- `executeCommandById(commandId: string)` — thin wrapper → `runCommand`
- `registerCommandActions()` — associate all Tier 1 action ids
- Shared module: catalog cache, settingsActions factory, layoutOptions from UserSettings

### 5. Settings UX

- Update `SettingsPanel.tsx` MessageBar (platform hint)

### 6. Docs (в PR; PRODUCT_CONTEXT post-merge architect)

- `docs/migration/03-powerpoint-desktop-windows.md` — обновить § «Как сейчас» → Tier 1 active после merge
- `docs/PRODUCT_CONTEXT.md` — architect обновит при приёмке

## Анти-scope

- `Office.actions.replaceShortcuts` из UserSettings — **S06-002**
- Все **79** action IDs в manifest — **S06-002** (если не влезает в 001)
- VSTO legacy
- Core layout / Api changes (кроме optional `ShortcutKeyFormatter`)
- Import/export JSON
- Регистрация hotkeys для Settings commands

## Затрагиваемые файлы (ожидаемо)

| Область | Файлы |
|---------|-------|
| Manifest | `manifest.template.xml`, `scripts/build-manifest.mjs`, `manifest.prod.xml` |
| Shortcuts | `scripts/build-shortcuts.mjs`, `dist/shortcuts.json` (generated) |
| Webpack | `webpack.config.js`, `package.json` scripts |
| Runtime | `src/runtime/` или refactor `src/taskpane/index.tsx`, `src/commands/commands.ts` |
| Execution | новый `executeCommandById.ts`, `registerCommandActions.ts` |
| Settings | `SettingsPanel.tsx` |
| Docs | `03-powerpoint-desktop-windows.md` |
| Tests | optional `ShortcutKeyFormatterTests.cs` |

## Критерии приёмки (Definition of Done)

1. [x] Manifest: `SharedRuntime 1.1` + `KeyboardShortcuts 1.1`; `ExtendedOverrides` → `shortcuts.json`
2. [x] `npm run validate:prod` — зелёный
3. [x] Tier 1: `Office.actions.associate` → `executeCommandById` → `runCommand` path
4. [x] `shortcuts.json` содержит Tier 1 defaults; build script воспроизводим (`npm run build:prod`)
5. [x] Feature detection: без API — no crash, task pane OK (Web regression guard)
6. [x] Settings MessageBar отражает платформенные ограничения
7. [x] `dotnet test PptPowerKeys.sln` — 114 passed
8. [x] `npm run typecheck`, `npm run validate:prod`, `npm run build:prod` — зелёные
9. [x] PR #46 merged в `main`
10. [x] `.github/review/CHECKLIST.md` пройден (Web Add-in scope)
11. [ ] Manual test: PP Desktop Win 2601+ (post-merge, вне CI)

## Приёмка (architect, 2026-06-29)

- PR #46 merged. Scope соблюдён: shared runtime, Tier 1 shortcuts (14), `associate` → `runCommand` wrapper, feature detection, docs.
- CHECKLIST: VstoLegacy не тронут; validate:prod зелёный; Settings commands исключены.
- Красные флаги не обнаружены. Manual QA на PP Desktop 2601+ — пользователю после deploy Pages.

## Зависимости

- S03 Shortcut Manager, S05 consulting presets — в main
- PR #43 desktop runbook — merged
- **Блокер manual QA:** версия PowerPoint пользователя ≥ 2601

## Риски (architect → builder)

| Риск | Действие |
|------|----------|
| Manifest V1_0 → shared runtime может потребовать VersionOverrides 1.1 | Изучить [MS sample](https://github.com/OfficeDev/Office-Add-in-samples/tree/main/Samples/office-keyboard-shortcuts) до кодирования |
| Конфликты Alt+1… с PowerPoint native | MS conflict dialog; document in PR |
| Webpack shared runtime ломает Online load (S01-008) | Проверить `validate:prod`; не менять prod URLs без необходимости |
| Дублирование логики `runCommand` | Только wrapper; code review red flag |

## Примечание для builder

- Ветка: `cursor/S06-001-shared-runtime-keyboard-shortcuts-<suffix>`
- Issue: см. backlog после создания architect'ом
- MS refs: [keyboard-shortcuts](https://learn.microsoft.com/en-us/office/dev/add-ins/design/keyboard-shortcuts), [shared runtime](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/configure-your-add-in-to-use-a-shared-runtime), [ExtendedOverrides](https://learn.microsoft.com/en-us/office/dev/add-ins/develop/extended-overrides)

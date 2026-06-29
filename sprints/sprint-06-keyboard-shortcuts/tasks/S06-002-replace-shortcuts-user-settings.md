# S06-002 — replaceShortcuts ↔ UserSettings (все bindings)

> Передача builder'у: `/builder выполни S06-002`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S06-002` |
| **Спринт** | `sprint-06-keyboard-shortcuts` |
| **Компонент** | AddIn |
| **Статус** | In Progress |
| **Issue** | #48 |
| **PR** | — |

## Цель

На **PowerPoint Desktop Windows 2601+** после загрузки task pane и после Save/Reset в Settings
синхронизировать глобальные шорткаты с `UserSettings.shortcuts` через `Office.actions.replaceShortcuts()`.
Поддержать все hotkey-eligible команды (76 = 79 − 3 Settings). Consulting profile (McKinsey/BCG)
после Save → live hotkeys. Conflict hints через `areShortcutsInUse`.

## Контекст (после S06-001)

| Компонент | Состояние |
|-----------|-----------|
| Shared Runtime | `src/runtime/bootstrap.tsx` — unified entry |
| Tier 1 associate | 14 ids в `tier1Commands.ts` + `registerCommandActions()` |
| shortcuts.json | 14 actions + default keys (`build-shortcuts.mjs`) |
| UserSettings | Shortcut Manager Save → API only; **не** `replaceShortcuts` |
| Settings MessageBar | «Hotkeys active on PP Desktop 2601+…» |

**Gap:** пользователь меняет bindings в Shortcut Manager → hotkeys в PowerPoint не обновляются
(остаются Tier 1 defaults из `shortcuts.json`).

## Решения architect (зафиксировано)

### Hotkey-eligible scope

**76 CommandIds** — все из каталога кроме `ExecutionKind.Settings` (3 команды):
`OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults`.

`actionId === CommandId` (как S06-001).

Settings-команды: **не** associate, **не** replaceShortcuts (открытие панели через hotkey — anti-scope).

### Associate vs replaceShortcuts

1. **`registerCommandActions()`** — расширить с Tier 1 (14) на все **76** ids (один handler → `executeCommandById`).
2. **`shortcuts.json`** (build script):
   - `actions[]` — **76** entries (`id`, `type: ExecuteFunction`, `name` из catalog titles или static map).
   - `shortcuts[]` — Tier 1 defaults (14) как сейчас; для остальных 62 actions без default key в JSON
     или key omitted — user keys приходят только через `replaceShortcuts`.
   - Проверить MS schema: если action без default key обязателен placeholder — зафиксировать решение builder'а в PR.

### `syncKeyboardShortcuts(settings: UserSettings)` — новый модуль

`src/runtime/syncKeyboardShortcuts.ts`:

```typescript
// pseudo
if (!Office.context.requirements.isSetSupported("KeyboardShortcuts", "1.1")) return;
const map = bindingsToOfficeMap(settings.shortcuts); // commandId → "Alt+1" | null
await Office.actions.replaceShortcuts(map);
```

- Пустой/whitespace keys → `null` (снять shortcut для action).
- Duplicate keys в bindings: **last wins** (duplicate warning уже в UI).
- Key format: `normalizeShortcutKeys()` из `shortcutBindings.ts` → Office format
  (`Alt+Shift+D`, не `Alt+shift+d`); добавить `toOfficeShortcutKey()` если нужно.

### Call sites (обязательно)

| Момент | Где |
|--------|-----|
| После bootstrap + `registerCommandActions()` | `bootstrap.tsx` (settings уже в commandContext после `bootstrapCommandContext`) |
| После Save | `SettingsPanel.onSave` → после `updateUserSettings` |
| После Reset | `SettingsPanel.onReset` |
| После App load settings | `App.tsx` `onSettingsUpdated` / initial load |
| После resetSettings via command | `settingsActions.resetToDefaults` в `App.tsx` |

Consulting profile: McKinsey/BCG меняют shortcuts в editor → Save → `replaceShortcuts`
(не live до Save — как presets сейчас).

### Conflict detection (non-blocking, optional)

В `ShortcutManager` при edit keys (onBlur или debounced): если API доступен,
`Office.actions.areShortcutsInUse({ "CommandId": keys })` → MessageBar warning (не блок Save).
Не дублировать existing duplicate-key warning внутри bindings.

### 9 `support=None` команд

Associate + replaceShortcuts разрешены; handler → `runCommand` → existing unsupported path
(как `ToggleZoom` в Tier 1).

### Degradation

- Web / PP без KeyboardShortcuts 1.1: `syncKeyboardShortcuts` no-op; task pane без crash.
- Settings MessageBar уточнить: «Edit shortcuts below and click Save to apply hotkeys on Desktop Windows.»

## Scope builder

| Область | Изменения |
|---------|-----------|
| `build-shortcuts.mjs` | 76 actions; Tier 1 defaults preserved; gen `shortcuts.json` |
| `tier1Commands.ts` | Rename/extend → `hotkeyEligibleCommandIds.ts` (76 ids) или generate from catalog list |
| `registerCommandActions.ts` | Loop all 76 ids |
| `syncKeyboardShortcuts.ts` | новый — map + `replaceShortcuts` |
| `bootstrap.tsx` | call sync after associate |
| `SettingsPanel.tsx`, `App.tsx` | call sync on save/reset/update |
| `ShortcutManager.tsx` | optional `areShortcutsInUse` warning |
| `commandContext.ts` | export helper if needed |
| Docs | `03-powerpoint-desktop-windows.md`, `PRODUCT_CONTEXT.md` (architect post-merge) |

## Анти-scope

- Новые CommandIds
- Settings commands as hotkeys
- VSTO / COM hook
- Persist shortcuts отдельно от UserSettings API
- Import/export JSON (S06-003)
- Core changes (optional `ShortcutKeyFormatter` в Core только если non-trivial — prefer AddIn)

## Затрагиваемые файлы (ожидаемо)

| Область | Файлы |
|---------|-------|
| Shortcuts | `scripts/build-shortcuts.mjs`, `dist/shortcuts.json`, `shortcuts.json` |
| Runtime | `src/runtime/hotkeyEligibleCommandIds.ts`, `registerCommandActions.ts`, `syncKeyboardShortcuts.ts`, `bootstrap.tsx` |
| Settings | `SettingsPanel.tsx`, `ShortcutManager.tsx`, `App.tsx` |
| Bindings | `src/taskpane/shortcutBindings.ts` (Office key format helper) |
| Docs | `docs/migration/03-powerpoint-desktop-windows.md` |

## Критерии приёмки (Definition of Done)

1. [ ] `shortcuts.json` declares **76** actions; Tier 1 default keys unchanged.
2. [ ] `registerCommandActions` associates **76** ids → `executeCommandById`.
3. [ ] `syncKeyboardShortcuts` вызывает `replaceShortcuts` from `UserSettings.shortcuts`.
4. [ ] Sync on: bootstrap (post-load), Save, Reset.
5. [ ] McKinsey/BCG profile + Save → BCG keys active (e.g. `Ctrl+Alt+B` for SameWidth).
6. [ ] Feature detection: no crash on Web without API.
7. [ ] (Optional) `areShortcutsInUse` warning in Shortcut Manager.
8. [ ] Settings MessageBar updated.
9. [ ] `dotnet test PptPowerKeys.sln` — зелёный (114+).
10. [ ] `npm run typecheck`, `validate:prod`, `build:prod` — зелёные.
11. [ ] PR: `cursor/S06-002-replace-shortcuts-user-settings-<suffix>`, `Closes #<issue>`.
12. [ ] CHECKLIST; manual QA note PP Desktop 2601+.

## Зависимости

- S06-001 merged (PR #46) — блокер ✓
- S03 Shortcut Manager, S05 presets — в main

## Риски (architect → builder)

| Риск | Mitigation |
|------|------------|
| 76 actions ломают `validate:prod` | Run validate early; follow MS sample schema |
| `replaceShortcuts` async errors | try/catch; log to status bar non-blocking |
| Online regression | Feature gate all `Office.actions` calls |
| shortcuts.json size / manifest limits | Document count in PR |

## Красные флаги (reject)

- Только Tier 1 associate без расширения до 76
- `replaceShortcuts` без feature detection
- Settings commands в hotkey map
- Дублирование `runCommand` logic
- `validate:prod` красный
- Web task pane regression

## Примечание для builder

- Ветка: `cursor/S06-002-replace-shortcuts-user-settings-<suffix>`
- Issue: см. backlog
- MS refs: [keyboard-shortcuts](https://learn.microsoft.com/en-us/office/dev/add-ins/design/keyboard-shortcuts), [replaceShortcuts](https://learn.microsoft.com/en-us/javascript/api/office/office.actions#office-office-actions-replaceshortcuts-member(1))

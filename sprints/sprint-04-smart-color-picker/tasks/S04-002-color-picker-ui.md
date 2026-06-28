# S04-002 — ColorPicker UI + wire OpenColorScheme

> Статус: **Todo** — старт после merge S04-001.

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S04-002` |
| **Спринт** | `sprint-04-smart-color-picker` |
| **Компонент** | AddIn |
| **Статус** | In Progress |

## Цель
React-панель **Smart Color Picker**: сетка swatches (theme + recent), apply Fill/Line/Text к выделению;
команда `OpenColorScheme` открывает picker вместо stub.

## Scope
- `ColorPickerPanel.tsx` (Fluent UI, стиль как `SettingsPanel.tsx`).
- Swatches grid + recent row; кнопки Apply Fill / Line / Text.
- Wire `App.tsx` `openColorScheme` → show/focus picker; убрать stub message.
- Использовать palette из S04-001 (`getActivePalette` / theme bootstrap).

## Анти-scope
- Persist recent — S04-003.
- Eyedropper / HEX input (stretch).
- VstoLegacy.

## Критерии приёмки
1. `OpenColorScheme` открывает рабочий picker (не stub).
2. Swatches отражают theme + recent palette.
3. Apply buttons меняют цвет выделения через HostScript.
4. `npm run typecheck`, `npm run build` — зелёные.

## Зависимости
- S04-001 (theme colors + palette merge).

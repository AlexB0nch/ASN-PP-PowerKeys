# S04-003 — Integration + persist recent colors

> Статус: **Todo** — старт после merge S04-002.

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S04-003` |
| **Спринт** | `sprint-04-smart-color-picker` |
| **Компонент** | AddIn (+ опционально Core/Api для UserSettings) |
| **Статус** | In Progress |

## Цель
Полная интеграция VSTO parity: `Alt+G`/`Alt+L` cycle по theme+recent; **persist** 5 recent colors
между reload task pane; MessageBar при недоступности theme read на Web.

## Scope
- Persist recent: **architect preference** — `localStorage` key `ppt-powerkeys-recent-colors` (per device, без Api round-trip); document in `PRODUCT_CONTEXT.md`. UserSettings extension — только если уже trivial.
- Refresh theme colors при смене презентации (если Office event доступен) или при открытии picker.
- MessageBar в picker/status: «Theme colors unavailable — using default palette» when `source === 'fallback'` on Web.
- E2E wiring: picker apply + Fill/Line/Text commands share `recordRecentColor` + persisted recent.

## Анти-scope
- FormatPainter, SSO, VstoLegacy.

## Критерии приёмки
1. Recent colors survive task pane reload (localStorage).
2. Fill/Line/Text cycle uses theme + persisted recent (10+5 VSTO parity).
3. Web fallback shows non-blocking warning when theme unreadable.
4. `dotnet test`, `npm run typecheck` — зелёные.
5. `docs/PRODUCT_CONTEXT.md` updated with persist decision.

## Зависимости
- S04-001, S04-002.

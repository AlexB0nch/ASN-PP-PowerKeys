# Sprint 04 — Smart Color Picker / Slide Master palette

> Контекст: Sprint 03 Done. `OpenColorScheme` — stub «planned (Sprint 04)»; `FillColor`/`LineColor` циклят
> фиксированную палитру в `formatColorState.ts` (10 theme-like hex + 5 recent in-memory).
> Кикофф — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта
Реализовать **Smart Color Picker**: UI палитры по команде `OpenColorScheme`, чтение theme colors
из презентации где возможно (Office.js), recent colors, интеграция с `FillColor`/`LineColor`/`TextColor`.

## Цели (декомпозирует architect в задачи `S04-0YY`)
- [ ] Чтение theme/slide colors через Office.js (Partial на Web — graceful fallback)
- [ ] Color Picker UI (swatches + recent + apply to selection)
- [ ] Wiring `OpenColorScheme` — открыть picker вместо stub
- [ ] Интеграция палитры с `formatColorState` / color commands (10 theme + 5 recent как VSTO)
- [ ] Персистентность recent colors (localStorage и/или `UserSettings` extension — architect решает)
- [ ] Тесты Core (palette logic); `dotnet test`, `npm run typecheck/validate:prod`

## Ограничения Office Web
- Чтение Slide Master palette **ограничено** vs VSTO COM — см. `CommandCatalog` (`FillColor` = Partial).
- Eyedropper / HEX input — optional stretch (README); Web may not support eyedropper.
- `ThemeColor` в Core уже есть (`src/PptPowerKeys.Core/Colors/ThemeColor.cs`).

## Вне фокуса
- FormatPainter, SSO, VstoLegacy
- Consulting Mode, Backup, Multi-slide (Sprint 05+)

## Definition of Done спринта
- `OpenColorScheme` открывает рабочий Color Picker (не stub)
- Fill/Line color commands используют расширенную палитру (theme + recent)
- Трассировка `S04-0YY` → PR → merge

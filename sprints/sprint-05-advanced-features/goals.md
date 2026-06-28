# Sprint 05 — Consulting Mode, Backup, Multi-slide

> Контекст: Sprint 04 Done (Smart Color Picker). Фичи из README «Дополнительные инновационные функции».
> Кикофф — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта
Реализовать **продвинутые продуктовые фичи** поверх базового паритета команд (Sprint 02):
Consulting Mode, Slide Backup Manager, Multi-slide операции — с учётом ограничений Office.js на Web.

## Кандидаты (architect декомпозирует в `S05-0YY`)

### A) Consulting Mode
- Профили шорткатов **McKinsey / BCG / Custom** (расширение `UserSettings.profile` + presets в Core)
- Snap to grid **0.1 cm** при layout (Core + ServerLayout или HostScript apply)
- Snap to nearest object — оценить feasibility; может быть отложено

### B) Slide Backup Manager
- Перемещение выделенных слайдов в «Backup» секцию в конце deck (README)
- Быстрое hide/show backup section — оценить Office.js API

### C) Multi-slide
- Paste/copy shape на несколько выделенных слайдов
- Remove object with same id/name across slides — оценить API

### D) Вне scope Sprint 05 (backlog)
- Smart Duplicate с памятью gap (есть `DuplicationEngine` + `gap` param, не wired)
- Object Statistics beyond Addup (MIN/MAX/AVG UI)
- Eyedropper / HEX (Sprint 04 deferred)

## Ограничения
- Office Web: multi-slide и slide sections **Partial** — явная деградация где None
- Математика snap/grid — **Core** (`ShapeBounds`), тесты без PowerPoint
- `VstoLegacy*` не трогать

## Definition of Done спринта
- Architect выбрал и закрыл приоритетный подмножество (не обязательно все три темы)
- Каждая реализованная фича: task → PR → merge + тесты где применимо
- `docs/PRODUCT_CONTEXT.md` обновлён

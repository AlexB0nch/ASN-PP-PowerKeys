# Sprint 01 — MVP Foundation

- **Версия:** v1.0 MVP
- **Период:** TBD

## Sprint Goal

> Плагин загружается в PowerPoint, показывает вкладку PowerKeys и умеет выравнивать и изменять размер объектов через шорткаты.

## Scope (в спринте)

- Инициализация VSTO COM Add-in
- Ядро: `ShortcutManager`, `CommandDispatcher`, `ColorSchemeReader`
- Команды выравнивания (18)
- Команды изменения размера (20)
- Вкладка PowerKeys в Ribbon
- Базовый Shortcut Manager

## Out of scope (не в этом спринте)

- Object / Format / Text / Slide команды (Sprint 02)
- Профили шорткатов, Smart Color Picker (Sprint 03)

## Риски и зависимости

- VSTO-сборку и тестирование можно выполнить только на Windows + PowerPoint (см. `setup/environment`).
- Конфликты шорткатов с нативными комбинациями PowerPoint.

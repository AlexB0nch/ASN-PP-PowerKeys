# Sprint 02 — Objects, Format, Text, Slide

- **Версия:** v1.0 MVP
- **Период:** TBD

## Sprint Goal

> Доступны все базовые команды работы с объектами, форматированием, текстом и слайдами — MVP-набор команд закрыт.

## Scope (в спринте)

- `ObjectCommands` — Insert, Duplicate, Group, Order, выделение по строке/столбцу
- `FormatCommands` — Fill/Line/Text color из Slide Master, Format Painter, sub/superscript
- `TextCommands` — Paste (un)formatted, выравнивание текста
- `SlideCommands` — Zoom toggle, Slide Sorter, Print, Copy Slide

## Out of scope

- Smart Color Picker с пипеткой (Sprint 03)
- Multi-slide paste, Backup Manager (Sprint 04)

## Риски и зависимости

- Зависит от `CommandDispatcher` и `ShortcutManager` (Sprint 01)
- Чтение Slide Master зависит от `ColorSchemeReader` (S01-007)

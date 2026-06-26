# Sprint 03 — v1.5

- **Версия:** v1.5
- **Период:** TBD

## Sprint Goal

> Smart Color Picker с пипеткой, профили шорткатов (McKinsey/BCG/Custom), Smart Duplicate и суммирование текстовых полей.

## Scope (в спринте)

- Smart Color Picker: палитра Slide Master, последние 5 цветов, пипетка, HEX/RGB
- Профили шорткатов с импортом/экспортом `.json`
- Smart Duplicate с памятью шага
- Суммирование текстовых полей (`Alt+A`)

## Out of scope

- Slide Backup Manager, Multi-slide paste, статистика MIN/MAX/AVG (Sprint 04)

## Риски и зависимости

- Зависит от `FormatCommands` и `ColorSchemeReader` (Sprint 02 / 01)
- Пипетка (eyedropper) может требовать обхода ограничений Office Interop

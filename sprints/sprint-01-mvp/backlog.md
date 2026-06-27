# Sprint 01 — Backlog

> Статусы: `Todo` → `In Progress` → `In Review` → `Done`. Колонка «Issue / PR» обеспечивает сквозную
> трассировку (см. `docs/TASK_MANAGEMENT.md`). Примечание: исходный MVP формулировался под VSTO; проект
> мигрировал на Office Web Add-in (Core/Api/AddIn), см. `docs/migration/`.

| ID | Задача | Компонент | Статус | Issue / PR |
|----|--------|-----------|--------|------------|
| S01-001 | Инициализация VSTO-проекта | setup | Todo | — |
| S01-002 | ShortcutManager — регистрация клавиш | Core | Todo | — |
| S01-003 | CommandDispatcher — роутинг команд | Core | Todo | — |
| S01-004 | AlignmentCommands (18 команд) | Commands | Todo | — |
| S01-005 | ResizeCommands (20 команд) | Commands | Todo | — |
| S01-006 | RibbonTab.xml — вкладка PowerKeys | UI | Todo | — |
| S01-007 | ColorSchemeReader — палитра Slide Master | Core | Todo | — |
| S01-008 | Исправить загрузку Add-in в PowerPoint Online (Web) | AddIn | Done | PR #7, #8 |
| S01-009 | Отдельный Id prod-манифеста (кэш Office отдаёт localhost) | AddIn | Done | PR #9 |
| S01-010 | Закоммитить готовый production-манифест в репозиторий | AddIn | Done | PR #10 |
| S01-011 | Задеплоить API на публичный HTTPS (Cannot reach backend) | Api | In Progress | — |

# Спринты

Папка для планирования и отслеживания спринтов разработки PPT PowerKeys.

> Управление задачами описано в [`docs/TASK_MANAGEMENT.md`](../docs/TASK_MANAGEMENT.md)
> (агенты `architect`/`builder`, сквозная трассировка `S0X-0YY`). Шаблон задачи — [`TASK_TEMPLATE.md`](./TASK_TEMPLATE.md).
> Статусы задач: `Todo` → `In Progress` → `In Review` → `Done`.

## Структура спринта

Каждый спринт — отдельная подпапка:

```
sprints/
├── sprint-01-mvp/
│   ├── goals.md          # Цели спринта
│   ├── backlog.md        # Задачи
│   └── retrospective.md  # Итоги
└── sprint-02-...
```

## Roadmap

| Спринт | Фокус | Статус |
|--------|-------|--------|
| Sprint 01 | Инфраструктура Web Add-in: Core/Api/AddIn, манифест, деплой статики + API на VDS | **Done** |
| Sprint 02 | Функциональный паритет команд (Objects, Format, Text, Alignment, Slides, UX) | **Done** — `sprint-02-functionality/retrospective.md` |
| Sprint 03 | Settings UI, persistent SettingsStore, Shortcut Manager | **Done** — `sprint-03-settings/retrospective.md` |
| Sprint 04 | Smart Color Picker / Slide Master palette (`OpenColorScheme`) | **Active** — `sprint-04-smart-color-picker/ARCHITECT-KICKOFF.md` |
| Sprint 05 | Backup, Multi-slide, Statistics | Planned |

> Sprint 01 по факту сфокусировался на миграции на Office Web Add-in и доведении сквозного пути до
> рабочего состояния в PowerPoint Online (исходные VSTO-цели переосмыслены). Детали — в его `retrospective.md`.

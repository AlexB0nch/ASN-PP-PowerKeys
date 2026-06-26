# Спринты

Планирование и отслеживание спринтов разработки **PPT PowerKeys**.

## Длительность и ритм

- Длительность спринта: **2 недели**.
- Цель спринта формулируется одним предложением (Sprint Goal).
- Все задачи имеют ID вида `S<NN>-<NNN>` (например `S01-004`).

## Структура папки спринта

Каждый спринт — отдельная подпапка (см. шаблон в [`_TEMPLATE/`](./_TEMPLATE)):

```
sprints/
├── _TEMPLATE/             # Шаблон для нового спринта
│   ├── goals.md           # Sprint Goal + scope
│   ├── backlog.md         # Задачи с acceptance criteria
│   └── retrospective.md   # Итоги
├── sprint-01-mvp/
├── sprint-02-objects-format/
├── sprint-03-v1.5/
└── sprint-04-v2.0/
```

## Церемонии

| Церемония | Когда | Результат |
|-----------|-------|-----------|
| Planning | Начало спринта | Заполнен `goals.md` и `backlog.md` |
| Daily / async standup | Ежедневно | Обновлён статус задач в `backlog.md` |
| Review (demo) | Конец спринта | Демо рабочих фич, смерженные PR |
| Retrospective | Конец спринта | Заполнен `retrospective.md` |

## Definition of Ready (задача готова к взятию в работу)

- [ ] Есть описание и привязка к компоненту (`Core` / `Commands` / `UI` / `Settings`)
- [ ] Сформулированы acceptance criteria
- [ ] Нет неразрешённых зависимостей от других задач

## Definition of Done (задача завершена)

- [ ] Код в `src/PptPowerKeys/` по архитектуре проекта
- [ ] Acceptance criteria выполнены
- [ ] PR создан, прошёл review по [`.github/review/CHECKLIST.md`](../.github/review/CHECKLIST.md)
- [ ] Протестировано в PowerPoint (указана версия) или покрыто тестами
- [ ] Нет конфликтов шорткатов
- [ ] Смержено в `main`

## Статусы задач

`Todo` → `In Progress` → `In Review` → `Done` (или `Blocked`)

## Roadmap

| Спринт | Фокус | Версия | Статус |
|--------|-------|--------|--------|
| [Sprint 01](./sprint-01-mvp) | Core, Alignment, Resize, Ribbon, базовый Shortcut Manager | v1.0 MVP | Planned |
| [Sprint 02](./sprint-02-objects-format) | Object, Format, Text, Slide команды | v1.0 MVP | Planned |
| [Sprint 03](./sprint-03-v1.5) | Smart Color Picker, профили шорткатов, Smart Duplicate, Addup | v1.5 | Planned |
| [Sprint 04](./sprint-04-v2.0) | Backup Manager, Multi-slide, разрезание, статистика, JSON-экспорт | v2.0 | Planned |

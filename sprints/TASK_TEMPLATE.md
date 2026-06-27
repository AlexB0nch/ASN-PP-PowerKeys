# Шаблон задачи спринта

> Используется агентом `architect` при постановке задач. В backlog задача отражается одной строкой
> таблицы, а полное описание живёт в GitHub Issue (форма `.github/ISSUE_TEMPLATE/task.yml`).

## Строка в `backlog.md`
```
| S0X-0YY | <краткое название> | <Core/Api/AddIn/Tests/docs/setup> | <Todo / In Progress / In Review / Done> | #<issue> / PR #<n> |
```

## Полное описание задачи (Issue)
- **Task ID:** `S0X-0YY`
- **Sprint:** `sprint-XX-*`
- **Проект/компонент:** Core / Api / AddIn / Tests / docs / setup
- **Цель:** зачем задача, какую ценность даёт.
- **Scope:** что входит.
- **Анти-scope:** что НЕ входит (явно).
- **Затрагиваемые файлы:** пути (`src/PptPowerKeys.Core/...`, `src/PptPowerKeys.AddIn/...`, ...).
- **Зависимости:** другие задачи `S0X-0YY` / внешние факторы.
- **Критерии приёмки (Definition of Done):** проверяемый список. Указывай, что должно проходить:
  `dotnet test PptPowerKeys.sln`, для панели — `npm run typecheck`/`npm run build`; новую логику — юнит-тесты
  в `PptPowerKeys.Tests` без PowerPoint; если нужен реальный sideload — отдельным пунктом (Windows/Mac + Office, вне CI).

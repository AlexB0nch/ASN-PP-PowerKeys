# Система управления задачами PPT PowerKeys

Цель — чтобы **каждая новая сессия (Desktop или Cloud-агент) понимала, что делать, и контекст не терялся**.
Управление построено на двух агентах и сквозной трассировке задач через спринты и GitHub.

## Агенты (`.cursor/agents/`)
| Агент | Вызов | Роль |
|-------|-------|------|
| **architect** | `/architect` | Держит контекст всего продукта (`docs/PRODUCT_CONTEXT.md`), ведёт спринты, пишет/декомпозирует задачи, ставит критерии приёмки, проверяет выполнение. |
| **builder** | `/builder` | Берёт одну задачу по ID, реализует строго в рамках scope (Core/Api/AddIn), прогоняет тесты/сборку, открывает PR. |

> В Cursor именованные агенты вызываются через `/имя` (`/architect`), а **не** через `@`.
> Синтаксис `@` подключает файлы и manual-правила, но не запускает агента.

Типичный диалог:
```
/architect составь задачи по целям текущего спринта и заведи их в GitHub Issues + backlog
/builder выполни задачу S02-003
/architect прими задачу S02-003 по PR #N
```

## Правила контекста (`.cursor/rules/`, подгружаются автоматически)
- `product-context` и `task-workflow` — всегда активны (anti-drift).
- `dotnet-core` — для `Core/Api/Tests`; `office-addin` — для `AddIn`; `vsto-legacy` — заморозка VSTO.

## Жизненный цикл задачи
1. **Backlog** — `architect` создаёт GitHub Issue по `.github/ISSUE_TEMPLATE/task.yml` + строку в
   `sprints/sprint-XX-*/backlog.md`. ID: `S0X-0YY`.
2. **In Progress** — `builder` создаёт ветку `cursor/<task-id>-<slug>`, реализует задачу.
3. **In Review** — `builder` открывает PR в `main` (`.github/PULL_REQUEST_TEMPLATE.md`, поля `Sprint`/`Task ID`,
   `Closes #<issue>`) с результатами `dotnet test` и (если затронута панель) `npm run typecheck/build`.
4. **Done** — `architect` проверяет по критериям приёмки + `.github/review/CHECKLIST.md` + зелёный CI,
   ставит `Done` в backlog и закрывает Issue. Иначе — возврат с конкретными правками.

## Сквозная трассировка (анти-дрейф контекста)
Один ID `S0X-0YY` присутствует везде: **Issue → backlog → ветка → PR → коммиты**. Любая сессия восстанавливает
контекст, начав с `docs/PRODUCT_CONTEXT.md`, текущего `backlog.md` и открытых Issues/PR.

## Спринты (`sprints/`)
- `sprint-XX-*/goals.md` — цели; `backlog.md` — задачи со статусами и ссылками на Issue/PR; `retrospective.md` — итоги.
- **Epic multi-sprint:** `epic-ltsc-windows-native/ROADMAP.md` (S07–S11, Product Line B).
- Шаблон задачи — `sprints/TASK_TEMPLATE.md`.

## Product Line B (LTSC Windows)

| Компонент | Путь | CI |
|-----------|------|-----|
| `PptPowerKeys.Windows` | `src/PptPowerKeys.Windows/` (S07-002+) | Windows + VS + VSTO (отдельный workflow позже) |
| Shared Core | `src/PptPowerKeys.Core` (netstandard2.0 + net8.0) | `dotnet test PptPowerKeys.sln` (Linux) |

Builder для Windows-задач: сборка/ручной QA на Windows; Core-only задачи (S07-001) — стандартный Linux CI.

## GitHub как место описания задач
- Задачи — **Issues** по `.github/ISSUE_TEMPLATE/task.yml` (Sprint, Task ID, проект/компонент, scope, критерии приёмки, зависимости).
- Баги — `.github/ISSUE_TEMPLATE/bug.yml`. PR связывается через `Closes #<n>`; ревью — `.github/review/`.

## Definition of Done
- Все критерии приёмки выполнены; зелёные `dotnet test PptPowerKeys.sln` и (для панели) `npm run typecheck/build`.
- Бизнес-логика покрыта тестами в `PptPowerKeys.Tests` без PowerPoint.
- Трассировка ID соблюдена; продуктовые решения отражены в `docs/PRODUCT_CONTEXT.md`/ретроспективе.

# Система управления задачами PPT PowerKeys

Эта система обеспечивает, чтобы **каждая новая сессия (Desktop или Cloud-агент) понимала, что делать,
и контекст не терялся**. Управление построено на двух агентах и сквозной трассировке задач через спринты
и GitHub.

## Агенты (`.cursor/agents/`)
| Агент | Вызов | Роль |
|-------|-------|------|
| **architect** | `/architect` | Держит контекст всего продукта (`docs/PRODUCT_CONTEXT.md`), ведёт спринты, пишет и декомпозирует задачи, ставит критерии приёмки, проверяет выполнение. |
| **builder** | `/builder` | Берёт одну задачу по ID, реализует строго в рамках scope, открывает PR. |

> Важно: в Cursor именованные агенты вызываются через `/имя` (например `/architect`), а **не** через `@`.
> Синтаксис `@` подключает файлы и manual-правила, но не запускает агента.

Типичный диалог:
```
/architect составь задачи по целям sprint-01 и заведи их в GitHub Issues и backlog
/builder выполни задачу S01-004
/architect прими задачу S01-004 по PR #N
```

## Жизненный цикл задачи
1. **Backlog** — `architect` создаёт GitHub Issue по `.github/ISSUE_TEMPLATE/task.yml` + строку в
   `sprints/sprint-XX-*/backlog.md`. ID задачи: `S0X-0YY`.
2. **In Progress** — `builder` создаёт ветку `cursor/<task-id>-<slug>` и реализует задачу.
3. **In Review** — `builder` открывает PR в `main` (шаблон `.github/PULL_REQUEST_TEMPLATE.md`,
   поля `Sprint`/`Task ID`, `Closes #<issue>`).
4. **Done** — `architect` проверяет по критериям приёмки + `.github/review/CHECKLIST.md`, ставит `Done`
   в backlog и закрывает Issue. Иначе — возврат с конкретными правками.

## Сквозная трассировка (анти-дрейф контекста)
Один ID `S0X-0YY` присутствует везде: **Issue → backlog → ветка → PR → коммиты**. Любая сессия
восстанавливает контекст, начав с `docs/PRODUCT_CONTEXT.md`, текущего `backlog.md` и открытых Issues/PR.

## Спринты (`sprints/`)
- `sprint-XX-*/goals.md` — цели; `backlog.md` — задачи со статусами и ссылками на Issue/PR;
  `retrospective.md` — итоги и решения.
- Шаблон новой задачи в backlog — `sprints/TASK_TEMPLATE.md`.

## GitHub как место описания задач
- Задачи заводятся как **Issues** по форме `.github/ISSUE_TEMPLATE/task.yml` (поля: Sprint, Task ID,
  компонент, scope, критерии приёмки, зависимости).
- Баги — по `.github/ISSUE_TEMPLATE/bug.yml`.
- PR связывается с Issue через `Closes #<n>`; ревью — по `.github/review/PROCESS.md` и `CHECKLIST.md`.

## Definition of Done
- Все критерии приёмки задачи выполнены.
- Чеклист ревью пройден (или обоснованно пропущен).
- Трассировка ID соблюдена; продуктовые решения отражены в `docs/PRODUCT_CONTEXT.md`/ретроспективе.

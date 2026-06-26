# Процесс Review

## Workflow

1. **Branch** — работа в ветке `cursor/<описание>-7151` или `feature/<описание>`
2. **PR** — создать Pull Request в `main` с заполненным шаблоном
3. **Review** — минимум 1 approve перед merge
4. **Merge** — squash merge в `main`

## Роли

| Роль | Ответственность |
|------|-----------------|
| Author | Реализация, self-review, заполнение PR-шаблона |
| Reviewer | Проверка по [CHECKLIST.md](./CHECKLIST.md) |
| Maintainer | Merge, релизы, спринт-планирование |

## Критерии merge

- Все пункты чеклиста review пройдены или обоснованно пропущены
- CI зелёный (когда настроен)
- Нет unresolved conversations

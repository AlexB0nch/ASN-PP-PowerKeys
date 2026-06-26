# Contributing — PPT PowerKeys

## Перед началом

1. Настройте окружение: [`setup/environment/README.md`](./setup/environment/README.md).
2. Ознакомьтесь с архитектурой: [`docs/architecture.md`](./docs/architecture.md).
3. AI-workflow (архитектор + субагенты): [`docs/cursor-workflow.md`](./docs/cursor-workflow.md).

## Рабочий процесс

1. Возьмите задачу из текущего спринта (`sprints/sprint-NN-*/backlog.md`), переведите в `In Progress`.
2. Создайте ветку: `feature/<кратко>` или `cursor/<кратко>-7151` (для агентов Cursor).
3. Коммитьте по [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`.
4. Откройте PR в `main`, заполните [шаблон PR](./.github/PULL_REQUEST_TEMPLATE.md), укажите Sprint и Task ID.
5. Пройдите review по [`.github/review/CHECKLIST.md`](./.github/review/CHECKLIST.md) и [`PROCESS.md`](./.github/review/PROCESS.md).
6. После approve — squash merge, переведите задачу в `Done`.

## Структура кода

Код плагина — в `src/PptPowerKeys/` по слоям `Core` / `Commands` / `UI` / `Settings`. Соблюдайте правила в `.cursor/rules/`.

## Соглашения C# / VSTO

- Освобождайте COM-объекты Interop (никаких утечек).
- Обрабатывайте случай «нет/мало выделенных объектов».
- Новые шорткаты — без конфликтов с PowerPoint/Windows и переопределяемые.

## Ветки

- Не пушьте напрямую в `main`.
- Одна ветка — одна логическая задача.

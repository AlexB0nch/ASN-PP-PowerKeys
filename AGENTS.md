# AGENTS.md — PPT PowerKeys

Durable-инструкции для всех AI-агентов в этом репозитории. Этот файл читается автоматически и переживает любую сессию. Не дублируйте сюда то, что лучше живёт в `.cursor/rules/*.mdc` — здесь только общий, постоянный контекст.

## Что это за проект

**PPT PowerKeys** — COM-надстройка (VSTO) для Microsoft PowerPoint: вкладка ленты PowerKeys + >100 настраиваемых шорткатов для выравнивания, изменения размера, работы с объектами, форматированием, текстом и слайдами. Полное описание продукта — в [`README.md`](./README.md). Архитектура — в [`docs/architecture.md`](./docs/architecture.md).

## Стек

- C# + VSTO (Visual Studio Tools for Office)
- `Microsoft.Office.Interop.PowerPoint`
- .NET Framework 4.7.2+ / .NET 6+
- Регистрация COM Add-in per-user (`HKCU`)

## Где что лежит

| Путь | Назначение |
|------|------------|
| `src/PptPowerKeys/` | Код плагина (`Core`, `Commands`, `UI`, `Settings`) |
| `tests/` | Тесты |
| `installer/` | MSI/EXE инсталлятор |
| `docs/` | Архитектура, ADR, workflow |
| `sprints/` | Спринты и backlog |
| `.cursor/` | Правила, субагенты, BUGBOT для Cursor |

## Главные правила

1. **Опорный объект.** Выравнивание/размер — относительно последнего выделенного объекта, не края слайда.
2. **COM cleanup.** Всегда освобождайте COM-объекты Interop.
3. **Edge cases.** Обрабатывайте «нет/мало выделенных объектов».
4. **Шорткаты.** Новые — без конфликтов с PowerPoint/Windows, переопределяемые.
5. **Per-user.** Без прав администратора.
6. **Совместимость.** PowerPoint 2013–2021 / M365, x86 и x64.

## Процесс

- Берите задачи из `sprints/sprint-NN-*/backlog.md`.
- Ветки: `cursor/<кратко>-7151` (агенты) или `feature/<кратко>`.
- Conventional Commits. PR в `main` по шаблону, review по `.github/review/CHECKLIST.md`.
- Значимые решения фиксируйте как ADR в `docs/adr/`.

## Важно про окружение

Сборку и тестирование VSTO можно делать **только на Windows с установленным PowerPoint**. Cloud-агенты (Linux) используйте для правок кода, рефактора, документации и review PR — не для сборки/прогона против Office.

## AI-workflow

Роли архитектора, builder и reviewer и как не терять контекст — в [`docs/cursor-workflow.md`](./docs/cursor-workflow.md). Конфигурация ролей — в `.cursor/agents/`.

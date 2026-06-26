# Architecture Decision Records (ADR)

Здесь фиксируются значимые архитектурные решения проекта.

## Зачем

ADR сохраняет **контекст решений** (почему сделали так, какие были альтернативы) — это критично, чтобы ни люди, ни AI-агенты не теряли контекст между сессиями.

## Формат

Каждый ADR — отдельный файл `NNNN-краткое-название.md` по шаблону [`_template.md`](./_template.md).

Статусы: `Proposed` → `Accepted` → (`Deprecated` / `Superseded by NNNN`).

## Список

| # | Решение | Статус |
|---|---------|--------|
| [0001](./0001-record-architecture-decisions.md) | Вести ADR | Accepted |
| [0002](./0002-tech-stack-vsto.md) | Стек: VSTO + C# + Office Interop | Accepted |
| [0003](./0003-ai-assisted-workflow.md) | AI-workflow: архитектор + субагенты в Cursor | Accepted |

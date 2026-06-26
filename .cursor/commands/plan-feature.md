# /plan-feature

Спланируй реализацию задачи спринта силами архитектора.

1. Уточни задачу (ID из `sprints/sprint-NN-*/backlog.md`).
2. Запусти субагента `architect` (read-only) в Plan Mode.
3. Архитектор анализирует затронутые компоненты, готовит пошаговый план, acceptance criteria и риски.
4. Сохрани план в workspace (`Save to workspace`), чтобы builder исполнил его без потери контекста.
5. Если решение значимое — предложи новый ADR в `docs/adr/`.

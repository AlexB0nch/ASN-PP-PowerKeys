# Кикофф architect — S06-004 (Object Statistics MIN/MAX/AVG UI)

> **Роль сессии:** полный цикл S06-004 — уточнить постановку → Issue → `/builder` → приёмка PR → merge → docs.
> S06-001…003 Done. Следующая задача backlog: **S06-004**.

## Контекст (2026-06-29)

| Метрика | Значение |
|---------|----------|
| Команды | **79** |
| Тесты | **125** `dotnet test` |
| Addup backend | `NumberAggregator` + `POST /api/text/addup` — готово |
| Addup UX сегодня | Status bar: `Sum X · avg Y · min Z · max W (N numbers)` — всегда все метрики |
| Settings | Export/import JSON v1 (PR #52); hotkeys sync on Save (PR #49) |
| **Gap S06-004** | Нет UI выбора «показать только MIN/MAX/AVG/SUM»; нет persist режима |

## Файлы для чтения

1. `docs/PRODUCT_CONTEXT.md`
2. `sprints/sprint-06-keyboard-shortcuts/tasks/S06-004-object-statistics-min-max-avg-ui.md` — черновик постановки
3. `src/PptPowerKeys.Core/Text/NumberAggregator.cs`
4. `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts` — case `AddupTextFields`
5. `src/PptPowerKeys.AddIn/src/taskpane/SettingsPanel.tsx` — паттерн snap-to-grid / export-import

## Рекомендуемый scope (architect фиксирует в Issue)

- `addupDisplayMode`: `all` | `sum` | `min` | `max` | `average` в **UserSettings**
- Core `AddupStatusFormatter` + tests
- Settings dropdown + Save; export/import поле
- `runCommand` форматирует status по режиму
- Optional: «Last addup result» в Text section (session-only)

**Anti-scope:** новые CommandIds; запись в shapes; изменение NumberAggregator math.

## Процесс сессии (чеклист)

1. [ ] Прочитать контекст; при необходимости уточнить task file
2. [ ] GitHub Issue по `.github/ISSUE_TEMPLATE/task.yml` → backlog **In Progress**
3. [ ] `/builder выполни S06-004` (ветка `cursor/S06-004-…`)
4. [ ] Приёмка PR: критерии из task file + `.github/review/CHECKLIST.md` + CI
5. [ ] Merge → backlog **Done** → закрыть Issue
6. [ ] Post-merge: `PRODUCT_CONTEXT.md` journal, README checkbox S06-004, goals DoD

## Copy-paste промпт

См. блок в конце этого файла или выдай пользователю из PR/commit message.

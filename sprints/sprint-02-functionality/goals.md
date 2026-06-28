# Sprint 02 — Функциональный паритет команд (Office Web Add-in)

> Контекст: инфраструктура завершена (Sprint 01). Надстройка работает в живом PowerPoint Online,
> API на собственном VDS. Кикофф для архитектора — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта
Довести **функциональный паритет** с исходным VSTO «ShortCut Tools»: реализовать исполнение команд,
которые помечены «not wired up yet» в `runCommand.ts`, с инвариантом `ShapeBounds` (математика — в Core).

## Цели (декомпозирует architect в задачи `S02-0YY`)
- [x] **Objects** — S02-001, PR #16
- [x] **Format** — S02-002, PR #17
- [x] **Text** — S02-003, PR #18
- [x] **Alignment** (edge-align + copy-and-align) — S02-004, PR #19
- [ ] **Slides** — **S02-005** (следующий приоритет)
- [ ] Единая деградация `support=None` + UX — **S02-006**
- [x] Юнит-тесты новой логики в `PptPowerKeys.Tests` (55 tests после S02-004)
- [x] Контракт `Api/Contracts` ↔ `AddIn/src/services/types.ts` синхронизирован по мере задач

## Вне фокуса (следующие спринты)
- Персистентное хранилище `SettingsStore` + SSO
- Smart Color Picker / палитра Slide Master
- Хардненинг деплоя (SSH host key pin, свой домен)

## Definition of Done спринта
- Бизнес-логика покрыта тестами; зелёные `dotnet test`, `npm run typecheck/validate:prod`
- Команды проверены в PowerPoint Online или явно помечены неподдерживаемыми на Web
- Трассировка `S02-0YY`: task → backlog → ветка → PR → merge

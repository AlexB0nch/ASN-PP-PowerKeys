# Sprint 02 — Функциональный паритет команд (Office Web Add-in)

> Контекст: инфраструктура завершена (Sprint 01). Надстройка работает в живом PowerPoint Online,
> API на собственном VDS, команды `ServerLayout` (alignment/resize/distribute) исполняются.
> Кикофф для архитектора новой сессии — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта
Довести **функциональный паритет** с исходным VSTO «ShortCut Tools»: реализовать исполнение команд,
которые сейчас помечены «not wired up yet» в `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`,
сохраняя инвариант границы `ShapeBounds` (математика — в `Core`, не на клиенте).

## Цели (декомпозирует архитектор в задачи `S02-0YY` для builder)
- [ ] Полное покрытие команд **Objects** (group/ungroup, duplicate-варианты, copy/paste position, insert-варианты).
- [ ] Команды **Format** (заливка/обводка/тень и т.п.) через Office.js — что реально поддерживается на Web.
- [ ] Команды **Text** (помимо Addup) — где применимо к выделению.
- [ ] Команды **Slides** (multi-slide операции) — оценить поддержку Office.js на Web.
- [ ] Явная и корректная деградация для команд, **не поддерживаемых** Office.js на Web (бейдж support + сообщение).
- [ ] Покрытие новой бизнес-логики юнит-тестами в `PptPowerKeys.Tests` (без PowerPoint).
- [ ] Обновление контракта `Api/Contracts` ↔ `AddIn/src/services/types.ts` при изменениях.

## Вне фокуса (кандидаты в следующие спринты)
- Профили/шорткаты с **персистентным** хранилищем (сейчас `SettingsStore` — in-memory заглушка) и SSO.
- Smart Color Picker / палитра Slide Master.
- Хардненинг деплоя (пин SSH host key, свой домен вместо `sslip.io`).

## Definition of Done спринта
- Бизнес-логика покрыта тестами; зелёные `dotnet test PptPowerKeys.sln`, `npm run typecheck/validate:prod`.
- Реализованные команды проверены в живом PowerPoint Online (или явно помечены как неподдерживаемые на Web).
- Трассировка `S02-0YY`: задача → backlog → ветка → PR → коммиты.

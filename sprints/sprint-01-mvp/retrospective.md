# Sprint 01 — Retrospective

_Заполняется по завершении спринта._

## Что прошло хорошо

- **S01-008 (PR #7, #8):** разделение dev/prod манифестов, `build:manifest`, CORS для GitHub Pages, workflow деплоя статики, runbook; фикс `Office.onReady`/`AppDomains`.
- **S01-009 (PR #9):** отдельный `<Id>` prod-манифеста — обошли кэш Office Online по GUID.
- **S01-010 (PR #10):** prod-манифест закоммичен в репозиторий → доступен через `git pull`; CI-защита от дрейфа.
- **Итог:** надстройка реально загрузилась в PowerPoint Online (task pane «PptPowerKeys (Web)» рендерится).

## Что улучшить

- Загрузка add-in в Online потребовала 3 итераций (домен `alexbonch`→`alexb0nch`, кэш по `<Id>`, незакоммиченный prod-манифест) — стоило сразу учесть, как именно пользователь устанавливает (git pull + upload), и что Office кэширует по Id.
- API-хостинг не был частью S01-008 — панель грузится, но «Cannot reach backend». Вынесено в S01-011.
- GitHub Issues не заводились (нет прав у cloud-агента) — трассировка через task-файлы + backlog + PR.

## Action items

- [x] Включить GitHub Pages и проверить публикацию `manifest.xml` (сделано).
- [ ] **S01-011:** задеплоить API на публичный HTTPS-хост, убрать «Cannot reach backend».

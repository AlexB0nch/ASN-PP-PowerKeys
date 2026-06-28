# Sprint 01 — Retrospective

_Итог: инфраструктурный путь add-in доведён до конца — надстройка работает в живом PowerPoint Online,
API на собственном VDS, команды выравнивания исполняются. Цепочка S01-008…012 закрыта._

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
- [x] **S01-011/012:** API задеплоен на собственный VDS (`sslip.io` + Caddy auto-HTTPS), «Cannot reach backend» устранён.
- [x] Сквозной путь подтверждён в живом PowerPoint Online (команды загружаются и исполняются).
- [ ] **Хардненинг (необязательно):** пин SSH host key (`VDS_SSH_FINGERPRINT`); свой домен вместо `sslip.io`.
- [ ] **Sprint 02:** функциональный паритет команд — см. `sprints/sprint-02-functionality/`.

## Сделанные задачи спринта
| ID | Итог |
|----|------|
| S01-008 | dev/prod манифесты, сборка prod, CORS, деплой статики на Pages |
| S01-009 | отдельный `<Id>` prod-манифеста (обход кэша Office по GUID) |
| S01-010 | prod-манифест закоммичен в репо (доступен через `git pull`) + CI drift-check |
| S01-011 | контейнеризация API (Dockerfile, `$PORT`), deploy-конфиг |
| S01-012 | деплой API на собственный VDS: Caddy auto-HTTPS, SSH-деплой через GitHub Actions, health-check |

> Примечание: исходные пункты целей Sprint 01 (VSTO-регистрация, Ribbon, Shortcut Manager) сформулированы под
> старую VSTO-архитектуру и **переосмыслены** под Office Web Add-in. Функциональная реализация команд вынесена
> в Sprint 02.

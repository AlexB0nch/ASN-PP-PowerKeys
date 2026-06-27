# S01-012 — Деплой API на собственный VDS (HTTPS через Caddy)

> Шаблон Issue: `.github/ISSUE_TEMPLATE/task.yml`.
> Передача builder'у: `/builder выполни S01-012`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S01-012` |
| **Спринт** | `sprint-01-mvp` |
| **Компонент** | Api (deploy) + AddIn (API_BASE_URL) + CI/CD |
| **Статус** | Часть A — Done (PR #12); часть B — за владельцем |
| **Связано с** | S01-011 (Dockerfile/контейнеризация готовы); снимает «Cannot reach backend» |

## Контекст
Владелец разворачивает API на собственном VDS `95.140.152.103` (вместо Azure). Надстройка уже грузится в
PowerPoint Online (S01-008/009/010), но без API показывает «Cannot reach backend». Dockerfile + биндинг
`$PORT`/8080 готовы (S01-011).

## Критичное требование — HTTPS с доверенным сертификатом
Панель на `https://alexb0nch.github.io` делает `fetch` к API. Браузер блокирует `http://<ip>` (mixed content)
и self-signed. Let's Encrypt не выдаёт на голый IP. Решение по умолчанию — хост **`95.140.152.103.sslip.io`**
(резолвится в IP), Caddy автоматически выпускает Let's Encrypt-сертификат. Если у владельца есть домен —
использовать его (A-запись → IP). Хост должен быть **параметризуемым** (одна точка изменения).

## Разделение работ
### A. Builder (репозиторий/конфиг — без секретов)
- **`docker-compose.yml`** (на VDS): сервис `api` (`build: .` из корневого Dockerfile, `ASPNETCORE_ENVIRONMENT=Production`,
  `expose: 8080`, `restart: unless-stopped`) + сервис `caddy` (`caddy:2`, порты `80:443`→`80/443`, том data/config,
  `restart: unless-stopped`).
- **`Caddyfile`**: `{$API_PUBLIC_HOST} { reverse_proxy api:8080 }` — авто-HTTPS. `API_PUBLIC_HOST` по умолчанию
  `95.140.152.103.sslip.io`, переопределяется env/`.env`.
- **`.github/workflows/deploy-vds.yml`**: на `workflow_dispatch` (+ опц. push в `main`), gated на наличие секрета
  `VDS_SSH_KEY`. Шаги: checkout → копировать на VDS нужные файлы (compose, Caddyfile, Dockerfile, src) через scp/rsync
  по SSH (secrets `VDS_HOST`/`VDS_USER`/`VDS_SSH_KEY`/`VDS_SSH_PORT`) → на сервере `docker compose up -d --build`.
  Без секрета — job не падает (skip notice). Используй проверенный SSH-action (например `appleboy/ssh-action`,
  `appleboy/scp-action`) либо ручной ssh с `known_hosts`.
- **Перенаправить API на новый хост:** `API_BASE_URL` = `https://95.140.152.103.sslip.io` в
  `manifest.template.xml`-генерации (через workflow-env), `deploy-addin-pages.yml`, `deploy.yml` (addin job),
  `webpack.config.js` дефолт prod, `src/config.ts` — где уместно. Пересобрать и закоммитить `manifest.prod.xml`
  (S01-010: он в git + CI drift-check) с новым `API_DOMAIN`/AppDomain.
- **CORS:** origin вызывающего (`https://alexb0nch.github.io`) не меняется — проверить, что остаётся разрешён.
- **Docs:** `docs/migration/02-powerpoint-web-deploy.md` (+ при необходимости `AGENTS.md`) — раздел «Деплой на VDS»:
  что должен сделать владелец (часть B), как триггерить workflow, как проверить `https://<host>/health`.

### B. Владелец (требует доступа/секретов)
- Завести deploy-пользователя + отдельную SSH-пару; публичный ключ → `~/.ssh/authorized_keys` на VDS.
- Установить на VDS **Docker + docker compose**; открыть порты **80 и 443** (firewall/iptables).
- Добавить в GitHub Actions Secrets: `VDS_HOST=95.140.152.103`, `VDS_USER=<deploy>`, `VDS_SSH_KEY=<приватный ключ>`,
  опц. `VDS_SSH_PORT`.
- Подтвердить хост: `sslip.io` по умолчанию **или** свой домен (тогда сообщить домен — builder подставит).
- Запустить workflow `Deploy API to VDS` (Actions → Run workflow). Проверить `https://<host>/health` → 200.

## Анти-scope
- Не выполнять реальный деплой из агента (нет доступа) — только конфиг/документация.
- Не хранить ключи/секреты в репозитории/коммитах/чате.
- Бизнес-логика Core, БД, SSO — вне задачи.

## Затрагиваемые файлы (ожидаемо)
- `docker-compose.yml`, `Caddyfile` (новые, корень)
- `.github/workflows/deploy-vds.yml` (новый)
- `manifest.template.xml`-генерация / `deploy-addin-pages.yml` / `deploy.yml` / `webpack.config.js` / `src/config.ts` — `API_BASE_URL`
- `src/PptPowerKeys.AddIn/manifest.prod.xml` (пересборка, новый `API_DOMAIN`)
- `docs/migration/02-powerpoint-web-deploy.md`, опц. `AGENTS.md`

## Критерии приёмки (Definition of Done)
1. [ ] `docker-compose.yml` + `Caddyfile`: Caddy проксирует на `api:8080`, авто-HTTPS по `API_PUBLIC_HOST` (дефолт `95.140.152.103.sslip.io`).
2. [ ] `deploy-vds.yml`: SSH-деплой gated на `VDS_SSH_KEY`; без секрета — skip без падения.
3. [ ] `API_BASE_URL` во всех точках = `https://95.140.152.103.sslip.io`; `manifest.prod.xml` пересобран (AppDomain/SourceLocation API → новый хост), CI drift-check зелёный.
4. [ ] CORS для `https://alexb0nch.github.io` сохранён (тест/локальный curl).
5. [ ] `dotnet test`, `npm run typecheck`, `npm run validate:prod` — зелёные.
6. [ ] Docs: пошагово часть B (Docker, порты, секреты, запуск workflow, проверка `/health`).
7. [ ] PR в `main` со ссылкой на S01-012.
8. [ ] (Владелец, post-deploy) `https://95.140.152.103.sslip.io/health` → 200; панель в PowerPoint Online загружает команды без «Cannot reach backend».

## Примечание для builder
Хост держи в **одной** переменной (`API_PUBLIC_HOST` / env), чтобы переключение на домен было тривиальным.
Реальный деплой и секреты — за владельцем (часть B); твоя зона — рабочая обвязка + документация.

## Приёмка части A (architect, 2026-06-27) — PR #12
Принято. Проверено: `docker-compose.yml` (api + `caddy:2`, авто-HTTPS), `Caddyfile` с `{$API_PUBLIC_HOST:95.140.152.103.sslip.io}`,
`deploy-vds.yml` (scp+ssh, gated на `VDS_SSH_KEY`, host key не пиннится — отмечено комментарием-риском).
`manifest.prod.xml`: `AppDomain` API → `https://95.140.152.103.sslip.io`, add-in URLs остались `alexb0nch.github.io`.
`dotnet test` — 47 passed (incl. CORS), `validate:prod` — valid, drift-check — пусто.

**Остаётся часть B (владелец):** deploy-пользователь + SSH-пара, Docker + compose, порты 80/443, секреты
`VDS_HOST`/`VDS_USER`/`VDS_SSH_KEY`/(`VDS_SSH_PORT`), запуск workflow «Deploy API to VDS»,
проверка `https://95.140.152.103.sslip.io/health` → 200.

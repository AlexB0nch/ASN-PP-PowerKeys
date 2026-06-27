# S01-011 — Задеплоить API на публичный HTTPS (убрать «Cannot reach backend»)

> Шаблон Issue: `.github/ISSUE_TEMPLATE/task.yml`.
> Передача builder'у: `/builder выполни S01-011`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S01-011` |
| **Спринт** | `sprint-01-mvp` |
| **Компонент** | Api (+ AddIn config, CI/CD) |
| **Статус** | Часть A — Done (PR #11); часть B — за владельцем |
| **Связано с** | S01-008/009/010 (загрузка add-in решена) |

## Симптом (от пользователя)
Надстройка в PowerPoint Online **открывается** (task pane «PptPowerKeys (Web)» рендерится), но показывает
**«Cannot reach backend: Failed to fetch»**. Причина: `API_BASE_URL = https://pptpowerkeys-api.azurewebsites.net`
не задеплоен — `fetch /api/commands` падает.

## Цель
`/api/commands` (и прочие эндпоинты) доступны по **публичному HTTPS** из браузера PowerPoint Online; панель
загружает каталог команд без «Cannot reach backend». CORS уже разрешает origin `https://alexb0nch.github.io`.

## Разделение работ
Деплой требует **внешнего хостинга и учётных данных**, которых нет у агента/CI. Поэтому задача делится:

### A. Builder (код/конфиг — можно сделать сейчас, без секретов)
- **Контейнеризация:** добавить `Dockerfile` для `src/PptPowerKeys.Api` (multi-stage `mcr.microsoft.com/dotnet/sdk:8.0`
  → `aspnet:8.0`), чтобы API можно было задеплоить на любой контейнер-хост.
- **Биндинг порта:** API должен слушать порт из env (`PORT` / `ASPNETCORE_URLS=http://+:${PORT:-8080}`),
  как требуют контейнер-хосты (Azure App Service, Render, Fly, Railway). Проверить `Program.cs`/Docker CMD.
- **Health/CORS:** `/health` отвечает 200; CORS для `https://alexb0nch.github.io` сохранён и работает в Release.
- **Гибкость URL:** `API_BASE_URL` должен задаваться при сборке add-in (уже так через webpack/`build-manifest`).
  Предусмотреть, что после деплоя может понадобиться обновить `API_BASE_URL` в `manifest.template.xml`/workflow,
  если итоговый хост не `pptpowerkeys-api.azurewebsites.net`.
- **Deploy-конфиг:** привести `.github/workflows/deploy.yml` (Azure) в рабочее состояние и/или добавить
  альтернативу (например `render.yaml`/`fly.toml`) для выбранного хоста. Оставить деплой gated на секрет/наличие хоста.
- **Локальная проверка:** `docker build` + `docker run -p 8080:8080`, затем `curl http://localhost:8080/health`
  и `curl http://localhost:8080/api/commands` — оба 200. Зелёные `dotnet test`, `npm run typecheck/validate:prod`.
- **Документация:** в `docs/migration/02-powerpoint-web-deploy.md` — точные шаги деплоя для выбранного хоста
  и что именно должен сделать владелец (см. секцию B).

### B. Пользователь (требует учётных данных — после A)
- Выбрать/создать хостинг и поднять API. **Рекомендация architect:** Azure App Service (free F1), имя
  `pptpowerkeys-api` (совпадает с текущим `API_BASE_URL` → манифест менять не придётся). Альтернатива без Azure —
  Render/Fly (free), но тогда builder обновит `API_BASE_URL` на новый хост и передеплоит Pages.
- Для Azure: создать App Service `pptpowerkeys-api`, скачать **Publish Profile**, добавить секрет
  `AZURE_WEBAPP_PUBLISH_PROFILE` в GitHub (Settings → Secrets and variables → Actions). Затем `deploy.yml`
  задеплоит API на push в `main`.
- После деплоя: открыть `https://<host>/health` (200) и сообщить URL в задачу/PR.

## Анти-scope
- Реальное выполнение layout-команд в живом PowerPoint (Office.js write-back) — Phase 4, не здесь.
- Постоянное хранилище настроек/БД — `SettingsStore` остаётся in-memory заглушкой.
- Аутентификация/SSO — отдельная задача.

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.Api/Dockerfile` (новый)
- `src/PptPowerKeys.Api/Program.cs` (порт из env, при необходимости)
- `.github/workflows/deploy.yml` (рабочий деплой) и/или `render.yaml`/`fly.toml`
- `docs/migration/02-powerpoint-web-deploy.md`
- (если меняется хост) `src/PptPowerKeys.AddIn/manifest.template.xml`, workflow `ADDIN/API_BASE_URL`

## Критерии приёмки (Definition of Done)
1. [ ] `Dockerfile` собирает образ API; `docker run` поднимает сервис, `/health` и `/api/commands` → 200 локально.
2. [ ] API слушает порт из env (совместимо с App Service / Render / Fly).
3. [ ] CORS для `https://alexb0nch.github.io` работает в Release-конфигурации (проверено интеграционным тестом/curl).
4. [ ] `deploy.yml` (или альтернативный blueprint) корректен и gated на секрет/хост; не падает при отсутствии секрета.
5. [ ] `dotnet test PptPowerKeys.sln`, `npm run typecheck`, `npm run validate:prod` — зелёные.
6. [ ] Документация: пошаговая инструкция деплоя + что должен сделать владелец (аккаунт, секрет/URL).
7. [ ] PR в `main` со ссылкой на S01-011.
8. [ ] (Пользователь, post-deploy) `https://<host>/health` → 200; в PowerPoint Online панель загружает команды
       без «Cannot reach backend».

## Примечание для builder
Сфокусируйся на части **A** — сделай API контейнеризуемым и хост-агностичным, приведи deploy-конфиг и документацию
в порядок. Часть **B** (создание аккаунта/секрета/URL) выполняет владелец репозитория; чётко опиши эти шаги в доках/PR.

## Приёмка части A (architect, 2026-06-27) — PR #11
Принято. Проверено независимо (docker на VM нет → запуск опубликованного API с теми же env, что в образе):
- `Dockerfile` (context = корень репо, restore Core+Api, multi-stage) + `.dockerignore`; `render.yaml` как альтернатива.
- `Program.cs` читает `$PORT` (проверено `PORT=10000` → `/health` 200); дефолт `ASPNETCORE_URLS=http://+:8080`.
- `deploy.yml`: gating исправлен (job-level env), без секрета build+test проходят, deploy скипается.
- Локально: `/health` → 200, `/api/commands` → 200 (76 команд), CORS preflight → 204 `Access-Control-Allow-Origin: https://alexb0nch.github.io`.
- `dotnet test -c Release` — 47 passed; `npm run validate:prod` — valid.

**Остаётся часть B (владелец):** создать Azure App Service `pptpowerkeys-api` (free F1) → скачать Publish Profile →
добавить секрет `AZURE_WEBAPP_PUBLISH_PROFILE` в GitHub Actions → push в `main` задеплоит API. Проверить
`https://pptpowerkeys-api.azurewebsites.net/health` → 200 и панель в PowerPoint Online без «Cannot reach backend».
Альтернатива без Azure: Render (Blueprint `render.yaml`) — тогда передать новый URL, builder обновит `API_BASE_URL`.

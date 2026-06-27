# Деплой Add-in для PowerPoint Online (Web)

Задача **S01-008**: надстройка должна загружаться в **PowerPoint Online** с публичными HTTPS URL,
а не с `localhost` (браузер не может достучаться до dev-сервера разработчика).

## Почему ломалось

| Проблема | Симптом в PowerPoint Web |
|----------|--------------------------|
| `manifest.xml` указывал на `https://localhost:3000` | «Error del complemento» / панель не открывается |
| `API_BASE_URL` по умолчанию `https://localhost:7168` | Панель грузится, но команды не работают (API недоступен) |
| CORS API только для `localhost:3000` | `fetch` к API блокируется из production origin |

> **WebFormFactor:** в add-in only manifest **нет** элемента `WebFormFactor`. По [документации Microsoft](https://learn.microsoft.com/en-us/javascript/api/manifest/desktopformfactor) элемент `DesktopFormFactor` покрывает **PowerPoint on the web**, Windows и Mac. Для Web достаточно корректных публичных URL в `DefaultSettings` / `SourceLocation`.

## Два режима

| Режим | Манифест | Статика | API |
|-------|----------|---------|-----|
| **Dev** (Desktop) | `manifest.dev.xml` / `manifest.xml` | `npm start` → `https://localhost:3000` | `dotnet run` → `https://localhost:7168` |
| **Prod** (Web + Desktop) | `manifest.prod.xml` (генерируется) | GitHub Pages / CDN | Azure App Service (или другой HTTPS хост) |

## Production URL по умолчанию

| Переменная | Значение по умолчанию |
|------------|----------------------|
| `ADDIN_BASE_URL` | `https://alexbonch.github.io/ASN-PP-PowerKeys` |
| `API_BASE_URL` | `https://pptpowerkeys-api.azurewebsites.net` |

Переопределяются при сборке (см. ниже).

## Сборка production bundle

```bash
cd src/PptPowerKeys.AddIn
npm ci

# Сгенерировать manifest.prod.xml с публичными URL
ADDIN_BASE_URL=https://alexbonch.github.io/ASN-PP-PowerKeys \
API_BASE_URL=https://pptpowerkeys-api.azurewebsites.net \
npm run build:prod

# Проверки (как в CI)
npm run typecheck
npm run validate:prod
```

Артефакты для деплоя:

- `dist/` — статика task pane (`taskpane.html`, JS, assets)
- `manifest.prod.xml` — манифест для sideload / Centralized Deployment

## Деплой статики (GitHub Pages)

Workflow `.github/workflows/deploy-addin-pages.yml` (при push в `main`):

1. `npm run build:prod`
2. Публикует `dist/` + `manifest.prod.xml` на GitHub Pages

Включите Pages в настройках репозитория: **Settings → Pages → Source: GitHub Actions**.

Итоговый URL статики: `https://alexbonch.github.io/ASN-PP-PowerKeys/taskpane.html`

## Деплой API

Workflow `.github/workflows/deploy.yml` публикует API в Azure Web App `pptpowerkeys-api`, если задан секрет `AZURE_WEBAPP_PUBLISH_PROFILE`.

CORS в production (`appsettings.Production.json` + `Program.cs`) разрешает origin `https://alexbonch.github.io`.

## Sideload в PowerPoint Online

1. Задеплойте статику и API (или используйте уже задеплоенные URL из PR).
2. Скачайте `manifest.prod.xml` из артефакта CI / GitHub Pages.
3. Откройте [PowerPoint Online](https://office.com) → **Insert → Add-ins → Upload My Add-in**.
4. Загрузите `manifest.prod.xml`.
5. На вкладке **Home** нажмите **PowerKeys** (или **Add-ins** → выберите PptPowerKeys).
6. **Ожидаемый результат:** task pane открывается без «Error del complemento»; UI загружается.
7. Если API доступен: список команд подтягивается с `/api/commands` (нет ошибки CORS в консоли браузера).

### Отличие ошибок

| Ошибка | Причина |
|--------|---------|
| «Error del complemento» при старте | Неверный / недоступный URL статики в манифесте |
| «Cannot reach backend» в панели | Статика OK, API недоступен или CORS |

## Локальная разработка (без изменений)

```bash
# Terminal 1 — API
cd src/PptPowerKeys.Api && dotnet run

# Terminal 2 — Add-in dev server
cd src/PptPowerKeys.AddIn && npm start
```

Sideload `manifest.dev.xml` (или `manifest.xml`) на **PowerPoint Desktop** с dev-сертификатами (`npx office-addin-dev-certs install`).

## Проверка манифеста

```bash
npm run validate          # dev (localhost) — для локальной разработки
npm run validate:prod     # production — все URL публичные HTTPS
```

# Деплой Add-in для PowerPoint Online (Web)

Задача **S01-008**: надстройка должна загружаться в **PowerPoint Online** с публичными HTTPS URL,
а не с `localhost` (браузер не может достучаться до dev-сервера разработчика).

> **Статус (2026-06-27): загрузка надстройки подтверждена.** После S01-008/009/010 task pane
> «PptPowerKeys (Web)» открывается в PowerPoint Online и рендерит UI. Остаточная ошибка
> «**Cannot reach backend: Failed to fetch**» — **ожидаема**: API ещё не задеплоен (см. задачу
> S01-011). Это НЕ ошибка загрузки манифеста.

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
| `ADDIN_BASE_URL` | `https://alexb0nch.github.io/ASN-PP-PowerKeys` |
| `API_BASE_URL` | `https://pptpowerkeys-api.azurewebsites.net` |

Переопределяются при сборке (см. ниже).

## Сборка production bundle

```bash
cd src/PptPowerKeys.AddIn
npm ci

# Сгенерировать manifest.prod.xml с публичными URL
ADDIN_BASE_URL=https://alexb0nch.github.io/ASN-PP-PowerKeys \
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

Итоговый URL статики: `https://alexb0nch.github.io/ASN-PP-PowerKeys/taskpane.html`

## Деплой API

Workflow `.github/workflows/deploy.yml` публикует API в Azure Web App `pptpowerkeys-api`, если задан секрет `AZURE_WEBAPP_PUBLISH_PROFILE`.

CORS в production (`appsettings.Production.json` + `Program.cs`) разрешает origin `https://alexb0nch.github.io`.

## Разные `<Id>` у dev и prod манифестов

> **Важно (S01-009).** Office Online **кэширует надстройку по `<Id>`**. Раньше dev и prod манифесты
> имели **одинаковый** GUID `92d7d44c-7eb7-4ed2-b83b-d6fa8fff9873`: после загрузки localhost-манифеста
> Office запоминал его по этому Id и при повторной загрузке prod-манифеста отдавал **закэшированную
> localhost-версию** (`SourceLocation = https://localhost:3000`) → «localhost refused to connect».
> Bump версии кэш по Id не сбрасывает.

| Манифест | `<Id>` | `DisplayName` | `SourceLocation` |
|----------|--------|---------------|------------------|
| `manifest.xml` / `manifest.dev.xml` (dev) | `92d7d44c-7eb7-4ed2-b83b-d6fa8fff9873` | `PptPowerKeys` | `https://localhost:3000/taskpane.html` |
| `manifest.prod.xml` (генерируется) | `5b0ca36f-a511-4705-a5e2-9609ff931f85` | `PptPowerKeys (Web)` | `https://alexb0nch.github.io/ASN-PP-PowerKeys/taskpane.html` |

Prod-GUID можно переопределить через env `ADDIN_ID` при сборке (`npm run build:manifest`); по умолчанию
используется `5b0ca36f-...`. Менять dev-GUID **не нужно**.

## Какой манифест грузить (важно)

> **Для PowerPoint Online загружайте только `src/PptPowerKeys.AddIn/manifest.prod.xml`.**
> Этот файл **закоммичен в репозиторий** (S01-010) — после `git pull origin main` он уже есть в вашем
> клоне с публичными HTTPS URL (`alexb0nch.github.io` / `pptpowerkeys-api.azurewebsites.net`) и prod-`<Id>`
> `5b0ca36f-...`. Грузите его напрямую из клона — скачивать что-либо отдельно не нужно.

| Файл | Назначение | Грузить в Online? |
|------|------------|-------------------|
| `manifest.prod.xml` | **Production** (публичные URL, prod-Id) | ✅ **Да — единственный для Online** |
| `manifest.xml` / `manifest.dev.xml` | Локальная dev-разработка (`localhost:3000`, dev-Id `92d7d44c-...`) | ❌ Нет — вызывает «localhost refused to connect» |

`manifest.prod.xml` детерминированно генерируется из `manifest.template.xml` (`npm run build:manifest`),
а CI проверяет, что закоммиченная копия не разошлась с шаблоном. Если нужно переопределить URL/Id —
пересоберите его и закоммитьте заново.

## Sideload в PowerPoint Online

1. `git pull origin main` — в клоне появляется `src/PptPowerKeys.AddIn/manifest.prod.xml`
   (статика/API уже задеплоены на публичные URL).
2. Возьмите файл `src/PptPowerKeys.AddIn/manifest.prod.xml` **прямо из клона** (НЕ `manifest.xml`/`manifest.dev.xml`).
3. **Удалите старую надстройку из Office, если загружали её раньше:** **Insert → My Add-ins →**
   найдите PptPowerKeys → **Remove** (это сбрасывает кэш localhost-версии). Из-за нового prod-`<Id>`
   prod-надстройка появится как **`PptPowerKeys (Web)`** — отдельно от dev-`PptPowerKeys`.
4. Откройте [PowerPoint Online](https://office.com) → **Insert → Add-ins → Upload My Add-in**.
5. Загрузите `manifest.prod.xml`.
6. На вкладке **Home** нажмите **PowerKeys** (или **Add-ins** → выберите PptPowerKeys (Web)).
7. **Ожидаемый результат:** task pane открывается с `https://alexb0nch.github.io/...`, без
   «localhost refused to connect» / «Error del complemento»; UI загружается.
8. Если API доступен: список команд подтягивается с `/api/commands` (нет ошибки CORS в консоли браузера).

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

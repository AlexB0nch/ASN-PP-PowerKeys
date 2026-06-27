# S01-010 — Закоммитить готовый production-манифест в репозиторий

> Шаблон Issue: `.github/ISSUE_TEMPLATE/bug.yml` (баг workflow деплоя/UX).
> Передача builder'у: `/builder выполни S01-010`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S01-010` |
| **Спринт** | `sprint-01-mvp` |
| **Компонент** | AddIn (manifest + CI + docs) |
| **Статус** | In Progress |
| **Связано с** | S01-008, S01-009 |

## Симптом (от пользователя)
Пользователь устанавливает надстройку так: `git pull origin main` на локальной машине → загружает
полученный файл манифеста в PowerPoint Online. Каждый раз панель показывает **«localhost refused to connect»**,
а в шапке — «PptPowerKeys» (а не «PptPowerKeys (Web)»).

## Диагноз (architect)
**Production-манифест не закоммичен в репозиторий.** После `git pull origin main` доступны только:
- `manifest.xml` — **dev/localhost** (Id `92d7d44c-...`, `SourceLocation = https://localhost:3000`),
- `manifest.dev.xml` — dev/localhost,
- `manifest.template.xml` — шаблон с плейсхолдерами `{{ADDIN_BASE_URL}}` / `{{ADDIN_ID}}` (не пригоден для загрузки as-is).

`manifest.prod.xml` генерируется `npm run build:manifest` и **исключён через `.gitignore`** (строка
`src/PptPowerKeys.AddIn/manifest.prod.xml`). Поэтому пользователь физически не получает prod-манифест через
pull и грузит localhost-версию. Деплой на GitHub Pages (`alexb0nch.github.io/.../manifest.xml`) корректен,
но пользователь его не использует — берёт файл из git-клона.

## Цель
После `git pull origin main` в репозитории есть **готовый к sideload production-манифест** с публичными
HTTPS URL и prod-Id `5b0ca36f-a511-4705-a5e2-9609ff931f85`, который пользователь загружает напрямую и
получает рабочую панель (без «localhost refused to connect»).

## Scope
- Убрать `src/PptPowerKeys.AddIn/manifest.prod.xml` из `.gitignore` и **закоммитить сгенерированный**
  `manifest.prod.xml` (публичные URL `alexb0nch.github.io`, prod-Id `5b0ca36f-...`, `DisplayName "PptPowerKeys (Web)"`).
- Защита от дрейфа: добавить шаг в CI (`.github/workflows/ci.yml`, add-in job) — `npm run build:manifest`
  и `git diff --exit-code -- src/PptPowerKeys.AddIn/manifest.prod.xml`: если закоммиченный файл разошёлся
  с генерацией из `manifest.template.xml` — job падает.
- Документация (`docs/migration/02-powerpoint-web-deploy.md` + при необходимости `AGENTS.md`): явно указать,
  что для PowerPoint Online нужно загружать **`src/PptPowerKeys.AddIn/manifest.prod.xml`**, а `manifest.xml`/
  `manifest.dev.xml` — только для локальной dev-разработки (localhost). Добавить шаг «удалить старую надстройку».

## Анти-scope
- Не менять dev-сценарий (`manifest.xml`/`manifest.dev.xml` остаются localhost + `92d7d44c-...`).
- Не трогать Core/Api/CORS.
- Деплой API на Azure — отдельно (панель должна грузиться и без API → «Cannot reach backend», но НЕ localhost-ошибка).

## Затрагиваемые файлы (ожидаемо)
- `.gitignore` (убрать строку manifest.prod.xml)
- `src/PptPowerKeys.AddIn/manifest.prod.xml` (новый, закоммиченный)
- `.github/workflows/ci.yml` (sync-check)
- `docs/migration/02-powerpoint-web-deploy.md`, опц. `AGENTS.md`

## Критерии приёмки (Definition of Done)
1. [ ] `manifest.prod.xml` присутствует в git (отслеживается), содержит `<Id>5b0ca36f-...`,
       `SourceLocation = https://alexb0nch.github.io/ASN-PP-PowerKeys/taskpane.html`, `DisplayName "PptPowerKeys (Web)"`.
2. [ ] `manifest.prod.xml` больше не упомянут в `.gitignore`.
3. [ ] CI-шаг проверяет синхронность закоммиченного `manifest.prod.xml` с генерацией (diff = пусто).
4. [ ] dev-манифесты не изменены; `npm run validate` и `npm run validate:prod` — зелёные.
5. [ ] `npm run typecheck`, `npm run build:prod`, `dotnet test PptPowerKeys.sln` — зелёные.
6. [ ] Документация: чётко указано, какой файл грузить (`manifest.prod.xml`) и удалить старую надстройку.
7. [ ] PR в `main` со ссылкой на S01-010.
8. [ ] (Ручная проверка пользователем) `git pull` → upload `manifest.prod.xml` → панель открывает Pages-URL без «localhost refused to connect».

## Примечание для builder
Суть: пользователь грузит файл из git-клона, а prod-манифест туда не попадает. Закоммить `manifest.prod.xml`
и сделай его очевидным в доках. CI-diff удержит файл в синхроне с шаблоном/скриптом.

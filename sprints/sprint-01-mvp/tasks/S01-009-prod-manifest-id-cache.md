# S01-009 — Отдельный Id production-манифеста (кэш Office Online отдаёт localhost)

> Шаблон Issue: `.github/ISSUE_TEMPLATE/bug.yml` (баг деплоя/манифеста).
> Передача builder'у: `/builder выполни S01-009`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S01-009` |
| **Спринт** | `sprint-01-mvp` |
| **Компонент** | AddIn (manifest + build) |
| **Статус** | In Progress |
| **Связано с** | S01-008 (PR #7, #8) |

## Симптом (от пользователя)
После переустановки production-манифеста (v1.0.0.1, URL `alexb0nch.github.io`) в **PowerPoint Online**
панель PptPowerKeys показывает **«localhost refused to connect»** — то есть `SourceLocation` указывает
на `https://localhost:3000`, хотя задеплоенный `manifest.xml` содержит публичные HTTPS URL.

## Диагноз (architect)
**Корневая причина — одинаковый `<Id>` у dev и prod манифестов.**
- `manifest.xml` / `manifest.dev.xml` / `manifest.template.xml` → один GUID `92d7d44c-7eb7-4ed2-b83b-d6fa8fff9873`.
- PowerPoint Online **кэширует надстройку по `<Id>`**. Пользователь ранее загружал localhost-манифест с этим
  GUID; при повторном «Upload My Add-in» с тем же ID Office отдаёт **старую закэшированную** запись
  (SourceLocation = localhost), а не новый prod-манифест. Bump версии (1.0.0.1) кэш по ID не сбрасывает.
- Дополнительный риск: пользователь мог по ошибке загрузить repo-`manifest.xml` (он остаётся localhost
  для dev) вместо файла с Pages.

## Цель
Production-манифест имеет **собственный, отличный от dev `<Id>`**, чтобы Office Online считал его
новой надстройкой и не подмешивал закэшированный localhost-SourceLocation. После переустановки панель
открывается с `https://alexb0nch.github.io/ASN-PP-PowerKeys/taskpane.html`, без «localhost refused to connect».

## Scope
- Ввести отдельный GUID для prod-манифеста. Предлагаемое значение: `5b0ca36f-a511-4705-a5e2-9609ff931f85`
  (можно сгенерировать свой). dev сохраняет `92d7d44c-...`.
- Реализовать через `build-manifest.mjs`: плейсхолдер `{{ADDIN_ID}}` в `manifest.template.xml`,
  подстановка prod-GUID при сборке (или env `ADDIN_ID` с дефолтом prod-GUID).
- Сделать prod визуально отличимым: `DisplayName` = `PptPowerKeys (Web)` (опционально, помогает пользователю
  не перепутать с dev в списке надстроек).
- Обновить `docs/migration/02-powerpoint-web-deploy.md`: явный шаг «удалить старую надстройку перед загрузкой»
  + примечание про разные Id dev/prod.
- `npm run typecheck`, `npm run validate:prod`, `dotnet test PptPowerKeys.sln` — зелёные.

## Анти-scope
- Не менять dev-сценарий (`manifest.dev.xml` / root `manifest.xml`) — остаётся localhost + старый GUID.
- Не трогать бизнес-логику Core/Api, CORS уже корректен (origin `alexb0nch.github.io`).
- Деплой API на Azure — отдельная задача (панель должна грузиться и без API, показывая «Cannot reach backend»).

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/manifest.template.xml` — `{{ADDIN_ID}}`, опц. `DisplayName (Web)`
- `src/PptPowerKeys.AddIn/scripts/build-manifest.mjs` — подстановка prod-GUID
- `docs/migration/02-powerpoint-web-deploy.md` — инструкция удалить старую надстройку
- (при необходимости) `sprints/sprint-01-mvp/backlog.md`

## Критерии приёмки (Definition of Done)
1. [ ] Сгенерированный `manifest.prod.xml` содержит `<Id>`, **отличный** от dev (`92d7d44c-...`); `validate:prod` зелёный.
2. [ ] dev-манифест(ы) сохраняют прежний GUID и localhost — `npm run validate` зелёный.
3. [ ] `npm run typecheck`, `npm run build:prod`, `dotnet test PptPowerKeys.sln` — зелёные.
4. [ ] Документация: добавлен явный шаг «Remove old add-in → Upload new» + примечание про разные Id.
5. [ ] PR в `main` со ссылкой на S01-009; после merge deploy GitHub Pages публикует обновлённый `manifest.xml`.
6. [ ] (Ручная проверка пользователем после deploy) переустановка → панель открывает Pages-URL, нет «localhost refused to connect».

## Примечание для builder
Главное — **разные `<Id>`** dev vs prod. Это первопричина. Остальное (DisplayName, docs) — вспомогательное.
После merge на Pages должен лежать `manifest.xml` с новым GUID и `SourceLocation` на `alexb0nch.github.io`.

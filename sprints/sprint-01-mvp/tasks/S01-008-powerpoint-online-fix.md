# S01-008 — Исправить загрузку Add-in в PowerPoint Online (Web)

> Шаблон Issue: `.github/ISSUE_TEMPLATE/task.yml` (или `bug.yml` — по сути баг деплоя).
> Передача builder'у: `/builder выполни S01-008`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S01-008` |
| **Спринт** | `sprint-01-mvp` |
| **Компонент** | AddIn (+ Api для CORS/деплоя) |
| **Статус** | Done |

## Симптом (от пользователя)
В **PowerPoint Online** (браузер) при открытии надстройки **PptPowerKeys** показывается ошибка:
«Error del complemento» / «Es posible que este complemento no se cargue correctamente…» (см. скриншот).
Кнопка «INICIAR» не приводит к рабочей панели.

## Диагноз (architect)
1. **`manifest.xml` указывает на `https://localhost:3000`** для всех `SourceLocation`, иконок и `AppDomains`.
   PowerPoint Web в браузере **не может** достучаться до localhost разработчика.
2. В `VersionOverrides` объявлен только **`DesktopFormFactor`** — нет **`WebFormFactor`** для Presentation host.
3. **`API_BASE_URL`** в `src/config.ts` по умолчанию `https://localhost:7168` — API тоже недоступен из Web.
4. `npm run validate` проходит (схема XML ок), но это **не** означает работоспособность в production/Web.

## Цель
Надстройка **загружается и открывает task pane** в PowerPoint Online без ошибки «complemento no se cargue»,
при sideload/деплое production-манифеста с публичными HTTPS URL.

## Scope
- Production-манифест (или build-time подстановка URL) с **публичными HTTPS** адресами статики Add-in.
- Добавить **`WebFormFactor`** в `manifest.xml` (и при необходимости `MobileFormFactor` для iPad — опционально в этой задаче).
- Сборка Add-in с **`API_BASE_URL`** на публичный endpoint API (webpack `DefinePlugin` / env).
- Настройка **CORS** в Api для origin панели и Office Web.
- Документация: как задеплоить Add-in + API для теста в PowerPoint Web (минимальный runbook в `docs/migration/` или `AGENTS.md`).
- Разделение **dev** (`localhost`) и **prod** манифестов или скрипт `npm run build:manifest`.

## Анти-scope
- Полный CI/CD на Azure (можно заглушку/ручной деплой, если нет хостинга — указать конкретный хост в PR).
- Реализация команд / бизнес-логики Core.
- VSTO legacy.
- Centralized Deployment в Microsoft 365 admin (отдельная задача, если понадобится).

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.AddIn/manifest.xml` (и/или `manifest.dev.xml`, `manifest.prod.xml`)
- `src/PptPowerKeys.AddIn/webpack.config.js` — `API_BASE_URL`, копирование манифеста
- `src/PptPowerKeys.AddIn/package.json` — скрипты build/validate prod
- `src/PptPowerKeys.AddIn/src/config.ts`
- `src/PptPowerKeys.Api/Program.cs` или `appsettings.json` — CORS
- `docs/migration/00-architecture.md` — инструкция деплоя для Web
- `.github/workflows/ci.yml` — при необходимости артефакт production build

## Критерии приёмки (Definition of Done)
1. [x] Production-манифест: все URL — **валидный публичный HTTPS** (не `localhost`); `npm run validate:prod` проходит.
2. [x] Манифест поддерживает PowerPoint on the web: **`DesktopFormFactor`** (add-in only; отдельного `WebFormFactor` нет — [MS docs](https://learn.microsoft.com/en-us/javascript/api/manifest/desktopformfactor)); `validate:prod` перечисляет «PowerPoint on the web».
3. [x] `npm run typecheck` и `npm run build:prod` — зелёные; `dotnet test PptPowerKeys.sln` — 47 passed.
4. [x] **Ручная проверка:** после merge → deploy GitHub Pages → sideload `manifest.xml` с Pages (runbook `docs/migration/02-powerpoint-web-deploy.md`). Блокер localhost устранён; smoke-test в PowerPoint Online — post-deploy (вне Cloud CI).
5. [x] CORS: интеграционный тест `Cors_AllowsGitHubPagesOrigin` для `https://alexbonch.github.io`.
6. [x] PR #7: production URL, шаги sideload, runbook.

## Приёмка (architect, 2026-06-27)
- PR #7, ветка `cursor/s01-008-powerpoint-online-fix-6260`.
- CI зелёный; локально повторены `dotnet test`, `npm run typecheck`, `validate`, `validate:prod`, `build:prod`.
- Отклонение от исходного scope: `WebFormFactor` не добавлялся — подтверждено схемой Office и валидатором манифеста.

## Зависимости
- Наличие хостинга для статики Add-in и API (GitHub Pages не подходит для API; варианты: Azure Static Web Apps + Azure App Service, Cloudflare Pages + Fly.io, и т.д. — зафиксировать в PR).
- SSL-сертификаты на production-доменах.

## Примечание для builder
Dev-сценарий (`npm start` + `manifest` с localhost) **должен сохраниться** для локальной разработки на Desktop.
Не ломать `npm run validate` для dev-манифеста.

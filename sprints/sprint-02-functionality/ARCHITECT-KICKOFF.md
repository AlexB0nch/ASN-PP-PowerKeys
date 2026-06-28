# Кикофф для архитектора (новая сессия) — Sprint 02

> Это входная точка для **новой сессии `/architect`**. Цель — продолжить развитие продукта: довести
> функциональный паритет команд и поставить задачи `S02-0YY` для `/builder`. Прочитай этот файл, затем —
> `docs/PRODUCT_CONTEXT.md`, `docs/TASK_MANAGEMENT.md`, `goals.md` рядом и `sprints/sprint-01-mvp/retrospective.md`.

## 1. Где мы сейчас (готово в Sprint 01)
Сквозной путь **работает в живом PowerPoint Online** (подтверждено 2026-06-28):
- **Статика add-in** — GitHub Pages: `https://alexb0nch.github.io/ASN-PP-PowerKeys/` (manifest `manifest.prod.xml`,
  отдельный prod-`<Id>` `5b0ca36f-...`, DisplayName «PptPowerKeys (Web)»). Деплой — `.github/workflows/deploy-addin-pages.yml`.
- **API** — собственный VDS `https://95.140.152.103.sslip.io` (Docker Compose: Caddy auto-HTTPS Let's Encrypt +
  Kestrel-контейнер). Деплой по SSH — `.github/workflows/deploy-vds.yml` (секреты `VDS_*` в GitHub Actions,
  запуск на любой push в `main`). `API_PUBLIC_HOST` — единственная точка смены хоста (sslip.io → свой домен).
- **CORS** для `https://alexb0nch.github.io` — настроен (`Program.cs`, тест `Cors_AllowsGitHubPagesOrigin`).
- Панель рендерит **76 команд** по категориям; команды `ServerLayout` (alignment/resize/distribute) **исполняются**.

## 2. Главная проблема для развития (фокус Sprint 02)
**Функциональный паритет неполный.** В `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`:
- `ServerLayout` (геометрия) — round-trip через `Api`/`Core.LayoutEngine`, работает для команд, что умеет `LayoutEngine`.
- `HostScript` — реализованы только: `InsertRectangle/Square/Ellipse/Line/Arrow`, `BringToFront/SendToBack/BringForward/SendBackward`, `AddupTextFields`.
- **Все остальные** падают в ветку «`...` is not wired up yet» → для пользователя команда видна, но не работает.

Задача архитектора: пройти по `CommandCatalog` (`src/PptPowerKeys.Core/Commands/`) и реестру в панели, сверить с
исходным VSTO «ShortCut Tools» (см. `README.md`, `docs/migration/` карта VSTO→Office.js), и **декомпозировать**
недостающие команды на задачи `S02-0YY` по категориям (Objects → Format → Text → Slides).

## 3. Инварианты (соблюдать при постановке задач)
- **Граница `ShapeBounds`**: вся математика layout — в `Core` (чистые функции, юнит-тесты без PowerPoint).
  Панель читает выделение через Office.js → отправляет в `Api` → применяет результат обратно по `id`. Anchor = последняя выделенная.
- `Core` — без `Microsoft.Office.*`/ASP.NET/UI. `Api` — тонкий слой. Enum команд — строковые имена.
- Контракт `Api/Contracts` ↔ `AddIn/src/services/types.ts` держать в синхроне.
- Не трогать `VstoLegacy*` (заморожен). Новый код — только `Core`/`Api`/`AddIn`.

## 4. Что важно учесть (ограничения Office.js на Web)
- Часть операций VSTO **недоступна** или ограничена в Office.js на PowerPoint Web. Для каждой команды решить:
  (а) `ServerLayout` (геометрия), (б) `HostScript` (есть Office.js API), (в) **не поддерживается на Web** —
  тогда явная деградация (бейдж `support` + понятное сообщение), без «not wired up yet».
- Проверяй доступность API по Office.js requirement sets; помечай команды реалистично.

## 5. Рекомендованный план задач (черновик — уточни и заведи в backlog)
1. **S02-001** — Objects: group/ungroup, duplicate-варианты (есть `DuplicationEngine` в `Core`), copy/paste position.
2. **S02-002** — Format: заливка/обводка/тень/прозрачность — что реально умеет Office.js на Web.
3. **S02-003** — Text: операции с текстом выделения (помимо Addup).
4. **S02-004** — Slides: multi-slide операции (оценить поддержку на Web).
5. **S02-005** — Единая корректная деградация неподдерживаемых команд (UX + бейджи support).
6. (Параллельно) покрытие новой логики тестами в `PptPowerKeys.Tests`.

## 6. Процесс
1. Заведи задачи `S02-0YY`: файл в `sprints/sprint-02-functionality/tasks/` (по образцу S01-0XX) + строка в `backlog.md`.
   (GitHub Issues у cloud-агента не создаются — нет прав; трассировка через task-файл + backlog + ветка + PR.)
2. Передавай builder'у: `/builder выполни S02-0YY`.
3. Принимай по критериям приёмки + `.github/review/CHECKLIST.md`; обновляй `docs/PRODUCT_CONTEXT.md` при продуктовых решениях.

## 7. Полезные ссылки (файлы)
- Исполнение команд: `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`, `src/office/powerpoint.ts`.
- Каталог команд: `src/PptPowerKeys.Core/Commands/`. Геометрия: `src/PptPowerKeys.Core/Layout/`, `Geometry/`.
- API: `src/PptPowerKeys.Api/Program.cs`, `Api/Contracts/`. Клиент: `AddIn/src/services/{api,types}.ts`.
- Деплой: `.github/workflows/{deploy-addin-pages,deploy-vds,ci}.yml`, корневые `Dockerfile`/`docker-compose.yml`/`Caddyfile`.

## 8. Хардненинг/долги (необязательно, можно отдельными задачами)
- Пин SSH host key в `deploy-vds.yml` (`VDS_SSH_FINGERPRINT`) — сейчас не пиннится (комментарий-риск в workflow).
- Свой домен вместо `sslip.io` (сменить `API_PUBLIC_HOST` + A-запись + пересборка манифеста/Pages).
- Персистентное хранилище для `SettingsStore` (сейчас in-memory) + профили/шорткаты + SSO.

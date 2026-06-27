# AGENTS.md

## Система управления задачами (читать в начале каждой сессии)

Работа над продуктом управляется через двух агентов и спринты — чтобы контекст не терялся между сессиями.
Перед началом работы прочитай:
- `docs/PRODUCT_CONTEXT.md` — единый источник правды о продукте (архитектура, инварианты, ограничения).
- `docs/TASK_MANAGEMENT.md` — процесс «architect → builder → PR → приёмка» и сквозная трассировка задач.
- `docs/migration/` — целевая архитектура (Office Web Add-in) и карта VSTO → Office.js.
- Текущий спринт: `sprints/sprint-XX-*/{goals,backlog,retrospective}.md`.

Агенты (`.cursor/agents/`), вызов через **`/имя`** (`/architect`, `/builder`; синтаксис `@` подключает только
файлы/manual-правила и агента не запускает):
- **`/architect`** — держит контекст продукта, пишет/декомпозирует задачи (GitHub Issues по
  `.github/ISSUE_TEMPLATE/task.yml` + строка в backlog), ставит критерии приёмки, проверяет результат.
- **`/builder`** — берёт задачу `S0X-0YY`, реализует её в ветке `cursor/<task-id>-<slug>`, открывает PR.

Правила в `.cursor/rules/` (`product-context`, `task-workflow`, `dotnet-core`, `office-addin`, `vsto-legacy`)
подгружаются автоматически. Сквозная трассировка: один ID `S0X-0YY` идёт через Issue → backlog → ветку → PR → коммиты.

## Cursor Cloud specific instructions

### Что это за репозиторий
PPT PowerKeys мигрирует с Windows-only VSTO на **кроссплатформенный Office Web Add-in**. Активная разработка —
в трёх проектах (см. `docs/migration/00-architecture.md`):
- `src/PptPowerKeys.Core` — .NET 8 class library (чистый C#, бизнес-логика, без `Microsoft.Office.*`).
- `src/PptPowerKeys.Api` — ASP.NET Core Minimal API + Swagger поверх Core.
- `src/PptPowerKeys.AddIn` — Office task pane (TypeScript + React + Fluent UI + Office.js).

Корневой `PptPowerKeys.sln` объединяет **только** Core + Api + Tests. Старый VSTO живёт в
`src/PptPowerKeys.VstoLegacy*` (отдельный `*.sln`), **заморожен** и исключён из CI (см. `FROZEN.md`).

### Сборка / тесты / запуск (всё работает на Linux/Cloud)
- **Backend (Core + Api + Tests):**
  - `dotnet test PptPowerKeys.sln` — юнит/интеграционные тесты (не требуют PowerPoint).
  - `cd src/PptPowerKeys.Api && dotnet run` — API на `http://localhost:5168` (Swagger `/swagger`).
    Для фикс-порта без профиля: `ASPNETCORE_URLS=http://localhost:5168 dotnet run --no-launch-profile`.
- **Add-in (task pane):** `cd src/PptPowerKeys.AddIn`, затем `npm ci`; dev-сервер `npm start`
  (https://localhost:3000, self-signed); проверки `npm run typecheck`, `npm run validate` (манифест), `npm run build`.
- **CI:** `.github/workflows/ci.yml` — две job'ы (backend .NET 8 + add-in TypeScript) на ubuntu; повторяют команды выше.

### Нюансы (non-obvious)
- **`dotnet test PptPowerKeys.sln` НЕ собирает VstoLegacy** — это by design; legacy собирается только на Windows + VS + VSTO.
  Не пытайся собрать `src/PptPowerKeys.VstoLegacy.sln` на Linux — он не предназначен для CI.
- **Реальный sideload надстройки в PowerPoint требует Office (Windows/Mac/Web)** — вне Cloud VM. На Linux
  проверяется только бэкенд (через HTTP) и сборка/typecheck панели, но не загрузка в живой PowerPoint.
  Dev-сертификаты (`npx office-addin-dev-certs install`) ставятся только на Windows/Mac.
- **Контракт Api↔AddIn:** enum'ы команд сериализуются строками (`"AlignLeft"`); типы клиента в
  `AddIn/src/services/types.ts` держи в синхроне с `Api/Contracts`.
- Инвариант геометрии: математика layout — в `Core` через границу `ShapeBounds`; anchor = последняя выделенная фигура.

### Toolchain на VM
- **.NET 8 SDK** (`dotnet`) и **Node.js** (`npm`) присутствуют (через snapshot / update-скрипт).
- Mono также установлен, но нужен только для legacy-экспериментов; основной разработке не требуется.

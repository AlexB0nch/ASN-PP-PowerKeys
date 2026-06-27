# PRODUCT_CONTEXT — PPT PowerKeys

> Единый источник правды о продукте. Владелец — агент `architect`. Обновляется при каждом решении,
> меняющем продукт. Цель — чтобы любая новая сессия (Desktop или Cloud) за минуты восстановила контекст.

## 1. Что это за продукт
**PPT PowerKeys** — надстройка для Microsoft PowerPoint, воспроизводящая/расширяющая «ShortCut Tools»
(>100 команд для быстрой работы со слайдами: выравнивание относительно опорной фигуры, ресайз, операции
с объектами, форматирование, текст, слайды). Продуктовая спецификация функционала — в `README.md`.

Проект **мигрирует с Windows-only VSTO на кроссплатформенный Office Web Add-in**
(Windows / Mac / Web / iPad). Детали миграции — в `docs/migration/`.

## 2. Архитектура (`docs/migration/00-architecture.md`)
```
PowerPoint (Desktop/Web/Mac/iPad)
  Task Pane (React + Fluent UI + Office.js)   → src/PptPowerKeys.AddIn
        │ HTTP REST (JSON)
  ASP.NET Core Minimal API (Swagger)          → src/PptPowerKeys.Api
        │ ссылка на проект
  PptPowerKeys.Core (.NET 8, чистый C#)        → src/PptPowerKeys.Core
```
| Проект | Назначение |
|--------|------------|
| `src/PptPowerKeys.Core` | .NET 8 class library, чистый C# без `Microsoft.Office.*`. Вся переносимая логика (`LayoutEngine`, `DuplicationEngine`, `NumberAggregator`, `CommandCatalog`, `ShapeBounds`). |
| `src/PptPowerKeys.Api` | ASP.NET Core Minimal API + Swagger. Тонкий слой поверх Core. |
| `src/PptPowerKeys.Tests` | xUnit: тесты Core + интеграционные тесты API. Не требуют PowerPoint. |
| `src/PptPowerKeys.AddIn` | Office Web Add-in: TypeScript + React + Fluent UI + Office.js. |
| `src/PptPowerKeys.VstoLegacy*` | **Заморожен** (Windows-only VSTO). Новые фичи не добавляются (`FROZEN.md`). |

Корневой `PptPowerKeys.sln` = Core + Api + Tests (кроссплатформенно).
`src/PptPowerKeys.VstoLegacy.sln` — отдельный legacy-solution (только Windows + **Visual Studio 2022** + workload VSTO).
**Важно:** для legacy нужна именно **Visual Studio 2022** (solution target — VS 17); VS Code / Rider / VS 2019 не подходят.
Для активной разработки (Core/Api/AddIn) Visual Studio **не обязательна** — достаточно `dotnet` CLI и Node.js (см. `AGENTS.md`).

## 3. Инварианты / правила (нарушать нельзя)
- **Граница `ShapeBounds`** (`{id,left,top,width,height}` в points): host читает геометрию через Office.js →
  Core считает чистыми функциями → host применяет обратно по `id`. Никакой математики layout на Office-типах.
- **Anchor = последняя выделенная фигура** (`AnchorIndex` переопределяет). Сохранено из VSTO.
- `Core` свободен от Office/ASP.NET/UI; `Api` — тонкий; бизнес-логика покрыта тестами **без PowerPoint**.
- DTO в `Api/Contracts` синхронизированы с `AddIn/src/services/types.ts`; команды — строковые имена (`"AlignLeft"`).
- Новый код только в `Core`/`Api`/`AddIn`; `VstoLegacy*` — заморожен.

## 4. API (эндпоинты)
| Метод | Путь | Назначение |
|-------|------|------------|
| GET | `/health` | health-check |
| GET | `/api/commands` (`/{id}`) | каталог команд + Office.js feasibility |
| POST | `/api/layout/apply` | применить геометрическую команду к `ShapeBounds[]` |
| POST | `/api/objects/duplicate-offset` | позиция дубликата (smart-duplicate) |
| POST | `/api/text/addup` | сумма/мин/макс/среднее чисел из текста |
| GET/PUT | `/api/settings`, POST `/api/settings/reset` | профиль + шорткаты (заглушка хранилища) |

## 5. Окружение и команды
Подробности и нюансы Cloud — в `AGENTS.md`. Кратко:
- Backend: `dotnet test PptPowerKeys.sln`; запуск API `cd src/PptPowerKeys.Api && dotnet run` (http://localhost:5168, `/swagger`).
- Add-in: `cd src/PptPowerKeys.AddIn && npm ci`; dev `npm start` (https://localhost:3000); проверки `npm run typecheck|validate|build`.
- CI: `.github/workflows/ci.yml` (jobs: backend .NET 8 + add-in TypeScript на ubuntu).
- Реальный sideload в PowerPoint требует Windows/Mac с Office (вне CI) — Phase 4.

## 6. Дорожная карта / статус
Статусы — в `sprints/` и в `docs/migration/00-architecture.md` (Definition of Done эпика миграции).

## 7. Журнал ключевых решений (анти-дрейф контекста)
- **VS 2022 для VSTO:** сборка/отладка `VstoLegacy` — только Windows + Visual Studio 2022 + workload «Office/SharePoint development» (VSTO). Не «любая Visual Studio».
- **PowerPoint Online (Web):** dev-манифест с `https://localhost:3000` не работает в браузере; для Web нужны публичные HTTPS URL, `WebFormFactor` в манифесте и задеплоенный API (задача `S01-008`).

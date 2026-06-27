# PptPowerKeys — целевая архитектура (Office Web Add-in)

Реализация эпика «Миграция PptPowerKeys с VSTO на Office Web Add-in».

## Обзор

```
PowerPoint (Desktop / Web / Mac / iPad)
  ┌───────────────────────────────────────────┐
  │  Task Pane  (React + Fluent UI + office.js) │  src/PptPowerKeys.AddIn
  └───────────────────┬───────────────────────┘
                      │ HTTP REST (JSON)
  ┌───────────────────▼───────────────────────┐
  │  ASP.NET Core Minimal API  (Swagger)        │  src/PptPowerKeys.Api
  └───────────────────┬───────────────────────┘
                      │ ссылка на проект
  ┌───────────────────▼───────────────────────┐
  │  PptPowerKeys.Core  (.NET 8, pure C#)       │  src/PptPowerKeys.Core
  │  LayoutEngine, NumberAggregator, Catalog…   │
  └─────────────────────────────────────────────┘
```

## Структура solution

| Проект | Назначение |
|---|---|
| `src/PptPowerKeys.Core` | .NET 8 class library. Чистый C# **без** `Microsoft.Office.*`. Содержит всю переносимую бизнес-логику. |
| `src/PptPowerKeys.Api` | ASP.NET Core Minimal API + Swagger. Тонкий слой поверх Core. |
| `src/PptPowerKeys.Tests` | xUnit. Тесты Core + интеграционные тесты API. **Не требуют PowerPoint.** |
| `src/PptPowerKeys.AddIn` | Office Web Add-in: TypeScript + React + Fluent UI + Office.js. |
| `src/PptPowerKeys.VstoLegacy*` | Замороженный старый VSTO-проект (Windows-only). Не развивается. |

Корневой `PptPowerKeys.sln` объединяет .NET 8 проекты (Core + Api + Tests).
`src/PptPowerKeys.VstoLegacy.sln` — отдельный solution для legacy (только Windows + **Visual Studio 2022** + workload VSTO).

## Ключевой архитектурный приём: граница `ShapeBounds`

Геометрические операции (выравнивание/распределение/resize) во VSTO писались бы прямо
против `Microsoft.Office.Interop.PowerPoint.Shape` — это невозможно юнит-тестировать.

В новой архитектуре введён контракт-граница [`ShapeBounds`](../../src/PptPowerKeys.Core/Geometry/ShapeBounds.cs)
(`{ id, left, top, width, height }` в точках):

1. Панель читает геометрию выделения через Office.js → `ShapeBounds[]`.
2. Бэкенд считает новую геометрию в `LayoutEngine` (чистая функция).
3. Панель применяет результат обратно на живые фигуры по `id`.

Так вся математика выравнивания/resize покрыта тестами без PowerPoint, и при этом
переиспользуется и из бэкенда, и (при желании) на клиенте.

Семантика **якоря** сохранена из VSTO: anchor = **последняя** выделенная фигура.

## Как запустить локально

### Бэкенд

```bash
cd src/PptPowerKeys.Api
dotnet run
# Swagger UI: http://localhost:5168/swagger
```

### Тесты

```bash
dotnet test PptPowerKeys.sln
```

### Task pane

```bash
cd src/PptPowerKeys.AddIn
npm install
npm start          # https://localhost:3000 (webpack dev server)
```

Sideload `src/PptPowerKeys.AddIn/manifest.xml` в PowerPoint
(Web: Insert → Add-ins → Upload My Add-in; Desktop: разделяемая папка / Centralized Deployment).
По умолчанию панель ходит на бэкенд по `API_BASE_URL` (см. `src/config.ts`).

> Примечание: dev-сервер использует self-signed HTTPS. Для sideload на desktop
> выполните `npx office-addin-dev-certs install` (требует Windows/Mac).

## Endpoints API

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/api/commands` | Каталог команд + Office.js feasibility |
| POST | `/api/layout/apply` | Применить геометрическую команду к `ShapeBounds[]` |
| POST | `/api/objects/duplicate-offset` | Позиция дубликата для smart-duplicate |
| POST | `/api/text/addup` | Сумма/мин/макс/среднее чисел из текста |
| GET/PUT | `/api/settings` | Профиль + шорткаты (заглушка хранилища, SSO-ready) |
| POST | `/api/settings/reset` | Сброс к значениям по умолчанию |

## Definition of Done эпика — статус

- [x] Бизнес-логика покрыта юнит-тестами **без** зависимости от PowerPoint.
- [x] В новой архитектуре нет зависимости от VSTO и .NET Framework 4.8.
- [x] Манифест валиден по схеме (`npm run validate`) для **локального sideload** (Desktop).
- [ ] **PowerPoint Web:** манифест пока указывает на `localhost` и содержит только `DesktopFormFactor` — в браузере надстройка не загружается (см. `S01-008`).
- [x] CI/CD (GitHub Actions) собирает Core+API, гоняет тесты, собирает add-in.
- [ ] Полный функциональный паритет и реальный sideload в PowerPoint — требует
      Windows/Mac с Office (вне CI), выполняется в Phase 4.

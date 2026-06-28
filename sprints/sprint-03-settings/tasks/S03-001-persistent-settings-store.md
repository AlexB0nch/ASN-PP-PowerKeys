# S03-001 — Persistent SettingsStore (file-backed JSON per user)

> Передача builder'у: `/builder выполни S03-001`

## Метаданные
| Поле | Значение |
|------|----------|
| **Task ID** | `S03-001` |
| **Спринт** | `sprint-03-settings` |
| **Комponent** | Core + Api + Tests + setup/docs |
| **Статус** | Done |

## Цель
Заменить in-memory `SettingsStore` на **file-backed** хранилище, чтобы настройки пользователя
(`UserSettings` + shortcuts) **переживали рестарт** API-контейнера на VDS. Это фундамент Sprint 03:
Settings UI (S03-002) и Shortcut Manager (S03-003) опираются на стабильную персистентность.

## Контекст
- Сейчас `SettingsStore` — `ConcurrentDictionary` in-memory; данные теряются при `docker compose up --build`.
- API уже есть: `GET/PUT /api/settings`, `POST /api/settings/reset`, header `X-User-Id`.
- Core: `UserSettings.Serialize`/`Deserialize`, `CreateDefaults()` из `CommandCatalog`.
- Legacy JSON shape сохранён (`Profile`, `Shortcuts[]` с `CommandId`, `Keys`) — см.
  `src/PptPowerKeys.VstoLegacy/Settings/UserSettings.json` (read-only reference).
- Anonymous user key: `__anonymous__` (как в текущем store).

## Scope

### Core — контракт хранилища
- Добавить интерфейс `IUserSettingsStore` (или эквивалент) в `PptPowerKeys.Core/Settings/`:
  `Get(string? userId)`, `Save(string? userId, UserSettings)`, `Reset(string? userId)`.
- Логика нормализации userId (`null`/whitespace → `__anonymous__`) может жить в реализации Api,
  но интерфейс — в Core для тестируемости.

### Api — file-backed реализация
- Заменить/расширить `SettingsStore` (или `FileUserSettingsStore`): один JSON-файл на пользователя
  в каталоге `SETTINGS_DATA_PATH` (env, default `/data/settings` в контейнере).
- Имя файла: безопасное для ФС (например `{sanitizedUserId}.json`; для `__anonymous__` — `__anonymous__.json`).
- При `Get`: если файла нет — создать defaults и **записать на диск** (или lazy create при первом save — зафиксируй поведение в коде/тестах).
- При `Save`/`Reset`: атомарная запись (write temp + move) чтобы не повредить файл при crash.
- Регистрация в DI: `Program.cs` читает `SETTINGS_DATA_PATH` из конфигурации/env.

### Docker / deploy
- `docker-compose.yml`: named volume `settings_data:/data/settings`, env `SETTINGS_DATA_PATH=/data/settings` для `api`.
- `Dockerfile`: при необходимости создать каталог `/data/settings` (или rely on runtime create).
- `docs/migration/02-powerpoint-web-deploy.md`: краткий подраздел про volume и env (backup note опционально).

### Тесты
- **Unit/integration:** save → dispose/recreate store (новый экземпляр с тем же temp path) → get возвращает сохранённые данные.
- **Integration (WebApplicationFactory):** существующий `Settings_DefaultsThenSaveRoundTrips` должен остаться зелёным; при необходимости настроить test env на temp directory.
- `dotnet test PptPowerKeys.sln` — зелёный.

## Анти-scope
- SSO / Azure AD / `getAccessToken()` для `X-User-Id` — отдельная задача.
- Settings UI в AddIn — S03-002.
- Shortcut Manager UI — S03-002/S03-003.
- Изменения `VstoLegacy*`.
- Изменения DTO контракта Api↔AddIn (если не требуется для persistence).

## Затрагиваемые файлы (ожидаемо)
- `src/PptPowerKeys.Core/Settings/IUserSettingsStore.cs` (новый)
- `src/PptPowerKeys.Api/Services/SettingsStore.cs` (refactor → file-backed)
- `src/PptPowerKeys.Api/Program.cs` — DI + конфигурация path
- `src/PptPowerKeys.Tests/` — тесты persistence
- `docker-compose.yml`
- `docs/migration/02-powerpoint-web-deploy.md` (volume/env)

## Критерии приёмки (Definition of Done)
1. [x] `SettingsStore` сохраняет `UserSettings` в JSON на диск per `X-User-Id` (или `__anonymous__`).
2. [x] Env `SETTINGS_DATA_PATH` настраивает каталог (default `/data/settings`).
3. [x] После «рестарта» store (новый экземпляр, тот же path) `GET /api/settings` возвращает ранее сохранённые данные.
4. [x] `POST /api/settings/reset` перезаписывает файл defaults из `UserSettings.CreateDefaults()`.
5. [x] Docker Compose монтирует volume для settings data на VDS.
6. [x] Runbook обновлён (volume path, env var).
7. [x] `dotnet test PptPowerKeys.sln` — зелёный (61 passed).
8. [x] PR #23: ветка `cursor/S03-001-persistent-settings-store-9e95`, Task ID `S03-001`.

## Приёмка (architect, 2026-06-28)
- PR #23 merged в `main` (commit `be40c4e`).
- CHECKLIST: scope соблюдён — Core interface + Api file impl, AddIn/VstoLegacy не тронуты.
- `FileUserSettingsStore`: atomic write, sanitize filenames, lazy create defaults on Get.
- Локально повторён `dotnet test` — 61 passed.

## Зависимости
- Нет блокеров; Settings API и `UserSettings` уже в main.

## Примечание для builder
- JSON через `UserSettings.Serialize`/`Deserialize` — не дублируй сериализацию.
- Сохрани совместимость JSON shape с legacy VSTO files.
- AddIn не трогать в этой задаче.
- Ветка: `cursor/S03-001-persistent-settings-store-9e95`.

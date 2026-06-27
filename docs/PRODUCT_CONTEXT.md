# PRODUCT_CONTEXT — PPT PowerKeys

> Единый источник правды о продукте. Владелец — агент `architect`. Обновляется при каждом решении,
> меняющем продукт. Цель — чтобы любая новая сессия (Desktop или Cloud) за минуты восстановила контекст.

## 1. Что это за продукт
**PPT PowerKeys** — COM-надстройка (VSTO) для Microsoft PowerPoint на Windows (C# / .NET Framework 4.8).
Добавляет вкладку ленты **PowerKeys** и >100 настраиваемых горячих клавиш для быстрой работы со слайдами
(выравнивание относительно опорного объекта, ресайз, операции с объектами, форматирование из палитры Slide
Master, работа с текстом и слайдами). Аналог/расширение «ShortCut Tools». Подробный функционал — в `README.md`.

**Чего НЕТ:** серверов, БД, портов, веб-интерфейса. Продукт исполняется внутри PowerPoint.

## 2. Архитектура (модули `src/PptPowerKeys/`)
| Слой | Файлы | Назначение |
|------|-------|------------|
| Точка входа | `ThisAddIn.cs`, `Globals.cs`, `UI/PowerKeysRibbon.cs`, `UI/RibbonTab.xml` | Запуск надстройки, лента PowerKeys |
| Core | `Core/ShortcutManager.cs`, `Core/CommandDispatcher.cs`, `Core/ColorSchemeReader.cs`, `Core/CommandContext.cs` (+ интерфейсы `I*.cs`) | Перехват клавиш, маршрутизация команд, чтение палитры, контекст выполнения |
| Команды | `Commands/*.cs`, `Commands/CommandIds.cs` (enum), `Commands/ICommand.cs` | Реализация команд по категориям |
| Настройки | `Settings/UserSettings.cs`, `Settings/default-shortcuts.json` | Профили и привязки шорткатов (JSON) |
| Тесты | `tests/*.cs` (xUnit, net48) | Smoke-тесты `CommandIds` и сериализации `UserSettings` |

## 3. Инварианты / правила (нарушать нельзя)
- Команды реализуют `ICommand` и регистрируются через `ICommandDispatcher`; ID — в `CommandIds` (enum).
- Корректное освобождение COM-объектов; обработка случая «нет выделения».
- Совместимость PowerPoint 2013–2021 / Microsoft 365.
- Шорткаты настраиваемы и не конфликтуют с нативными сочетаниями PowerPoint/Windows.
- Целевой фреймворк `net48`; без зависимостей вне .NET Framework 4.8.

## 4. Ограничения окружения
- **Сборка/запуск/`dotnet test PptPowerKeys.sln`** — только на Windows + Visual Studio 2022 (workload
  Office/SharePoint) + Office + VSTO Runtime. На Linux/Cloud не собирается (legacy VSTO `.csproj` + Office interop).
- На Linux/Cloud проверяется только платформонезависимая логика (`CommandIds`, `UserSettings`) через **Mono**.
- Подробности и команды — в `AGENTS.md`.

## 5. Дорожная карта (статус по спринтам)
Источник статусов — `sprints/`. Кратко (см. `sprints/README.md`):
- Sprint 01 — MVP: Core, Alignment, Resize, Ribbon — *Planned*.
- Sprint 02 — Objects, Format, Shortcut Manager — *Planned*.
- Sprint 03 — v1.5: Smart Color Picker, Profiles — *Planned*.
- Sprint 04 — v2.0: Backup, Multi-slide, Statistics — *Planned*.

## 6. Открытые вопросы / решения
> Журнал ключевых продуктовых решений. Добавляй сюда новые записи, чтобы контекст не терялся.

- _(пока пусто — заполняется по мере принятия решений)_

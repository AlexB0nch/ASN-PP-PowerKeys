# Setup Environment — PPT PowerKeys

Инструкции по настройке окружения разработки.

## Требования

| Компонент | Версия |
|-----------|--------|
| ОС | Windows 10 / 11 |
| Visual Studio | 2022+ с workload «Office/SharePoint development» |
| .NET | .NET Framework 4.8 |
| PowerPoint | 2013, 2016, 2019, 2021 или Microsoft 365 |
| VSTO Runtime | [Скачать](https://aka.ms/vstor) |

## Шаги настройки

1. Установить Visual Studio 2022 с компонентом **Visual Studio Tools for Office (VSTO)**  
   (Workloads → Office/SharePoint development → Visual Studio Tools for Office)
2. Установить Microsoft Office (PowerPoint)
3. Установить [VSTO Runtime](https://aka.ms/vstor), если не установлен вместе с Office
4. Клонировать репозиторий:
   ```bash
   git clone https://github.com/AlexB0nch/ASN-PP-PowerKeys.git
   cd ASN-PP-PowerKeys
   ```

## Открытие и сборка solution

1. Открыть `src/PptPowerKeys.sln` в Visual Studio 2022
2. В **Solution Explorer** убедиться, что проект `PptPowerKeys` загружен (тип: PowerPoint Add-in, VSTO)
3. Выбрать конфигурацию **Debug | Any CPU**
4. Собрать solution: **Build → Build Solution** (Ctrl+Shift+B)
5. Ожидаемый результат: сборка без ошибок, выходная папка `src/PptPowerKeys/bin/Debug/`

> **Примечание:** Сборка возможна только на Windows с установленным VSTO workload. На Linux/macOS файлы проекта валидны, но MSBuild не найдёт Office Tools targets.

## Запуск и отладка (F5)

1. Установить проект `PptPowerKeys` как **Startup Project** (правый клик → Set as Startup Project)
2. Нажать **F5** (Start Debugging) — Visual Studio:
   - соберёт надстройку;
   - зарегистрирует COM Add-in для текущего пользователя;
   - запустит PowerPoint с подключённой надстройкой **PPT PowerKeys**
3. В PowerPoint должна появиться вкладка **PowerKeys** на ленте (кнопки пока заглушки)
4. Остановка отладки (Shift+F5) снимает регистрацию debug-версии

### Если F5 не запускает PowerPoint

- Проверить, что PowerPoint установлен и закрыт перед запуском
- **Project → Properties → Debug** — действие должно быть «Start external program» → путь к `POWERPNT.EXE`
- Убедиться, что VSTO Runtime установлен

## Запуск тестов

1. Открыть **Test Explorer** (Test → Test Explorer)
2. Запустить тесты проекта `PptPowerKeys.Tests`
3. Smoke-тесты проверяют `CommandIds` и сериализацию `UserSettings`

## Структура репозитория

```
ASN-PP-PowerKeys/
├── src/
│   ├── PptPowerKeys.sln          # Solution
│   └── PptPowerKeys/             # VSTO PowerPoint Add-in
│       ├── Core/                 # ShortcutManager, CommandDispatcher, ColorSchemeReader
│       ├── Commands/             # Команды по категориям
│       ├── UI/                   # RibbonTab.xml, PowerKeysRibbon.cs
│       └── Settings/             # UserSettings, default-shortcuts.json
├── tests/                        # xUnit тесты
├── installer/                    # MSI / EXE инсталлятор
├── docs/                         # Документация
├── sprints/                      # Спринты и backlog
├── setup/environment/            # Настройка окружения (эта папка)
└── .github/                      # PR-шаблоны, review-процесс
```

## Переменные окружения

_Будут добавлены при настройке CI/CD._

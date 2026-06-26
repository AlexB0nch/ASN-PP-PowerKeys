# Setup Environment — PPT PowerKeys

Инструкции по настройке окружения разработки.

## Требования

| Компонент | Версия |
|-----------|--------|
| ОС | Windows 10 / 11 |
| Visual Studio | 2022+ с workload «Office/SharePoint development» |
| .NET | .NET Framework 4.7.2+ или .NET 6+ |
| PowerPoint | 2013, 2016, 2019, 2021 или Microsoft 365 |
| VSTO Runtime | [Скачать](https://aka.ms/vstor) |

## Шаги настройки

1. Установить Visual Studio с компонентом **Visual Studio Tools for Office (VSTO)**
2. Установить Microsoft Office (PowerPoint)
3. Клонировать репозиторий:
   ```bash
   git clone https://github.com/AlexB0nch/ASN-PP-PowerKeys.git
   cd ASN-PP-PowerKeys
   ```
4. Открыть solution в `src/` (будет создан при инициализации проекта)
5. Зарегистрировать COM Add-in для отладки (F5)

## Структура репозитория

```
ASN-PP-PowerKeys/
├── src/PptPowerKeys/     # Исходный код плагина
│   ├── Core/             # ShortcutManager, CommandDispatcher, ColorSchemeReader
│   ├── Commands/         # Команды по категориям
│   ├── UI/               # Ribbon, формы
│   └── Settings/         # UserSettings.json
├── tests/                # Unit / integration тесты
├── installer/            # MSI / EXE инсталлятор
├── docs/                 # Документация
├── sprints/              # Спринты и backlog
├── setup/environment/    # Настройка окружения (эта папка)
└── .github/              # PR-шаблоны, review-процесс
```

## Переменные окружения

_Будут добавлены при настройке CI/CD._

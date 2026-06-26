# Архитектура PPT PowerKeys

COM-надстройка (VSTO) для Microsoft PowerPoint, добавляющая вкладку **PowerKeys** и >100 настраиваемых шорткатов.

## Слои

```
┌──────────────────────────────────────────────┐
│                    UI                          │
│  RibbonTab.xml · ColorPickerForm ·             │
│  ShortcutManagerForm                           │
├──────────────────────────────────────────────┤
│                  Core                          │
│  ShortcutManager · CommandDispatcher ·         │
│  ColorSchemeReader                             │
├──────────────────────────────────────────────┤
│                Commands                        │
│  Alignment · Resize · Object · Format ·        │
│  Text · Slide                                  │
├──────────────────────────────────────────────┤
│                Settings                        │
│  UserSettings.json (профили, шорткаты)         │
├──────────────────────────────────────────────┤
│   Microsoft.Office.Interop.PowerPoint (COM)    │
└──────────────────────────────────────────────┘
```

## Компоненты

| Компонент | Папка | Ответственность |
|-----------|-------|-----------------|
| `ShortcutManager` | `Core` | Регистрация, перехват и разрешение конфликтов клавиш |
| `CommandDispatcher` | `Core` | Роутинг шортката → команда по категориям |
| `ColorSchemeReader` | `Core` | Чтение палитры активного Slide Master |
| `*Commands` | `Commands` | Бизнес-логика команд (Alignment, Resize, Object, Format, Text, Slide) |
| `RibbonTab.xml` | `UI` | Декларация вкладки и групп ленты |
| `ColorPickerForm` | `UI` | Диалог Smart Color Picker |
| `ShortcutManagerForm` | `UI` | Настройка и профили шорткатов |
| `UserSettings.json` | `Settings` | Хранилище пользовательских настроек |

## Ключевые принципы

1. **Опорный объект.** Команды выравнивания/размера работают относительно *последнего выделенного* объекта (`Selection.ShapeRange`), а не края слайда.
2. **Освобождение COM.** Все COM-объекты Interop освобождаются детерминированно; избегаем утечек (см. правило `.cursor/rules/20-csharp-vsto.mdc`).
3. **Настраиваемость.** Любой шорткат переопределяем; дефолты — в профилях, переопределения — в `UserSettings.json`.
4. **Per-user установка.** Без прав администратора; регистрация COM Add-in в `HKCU`.
5. **Совместимость.** PowerPoint 2013–2021 / Microsoft 365, x86 и x64.

## Поток выполнения команды

```
Нажатие клавиши
   → ShortcutManager (перехват, сопоставление)
   → CommandDispatcher (резолв команды по категории)
   → *Command.Execute(Selection)
   → Interop.PowerPoint (изменение слайда)
```

## Архитектурные решения

Значимые решения фиксируются как ADR в [`docs/adr/`](./adr).

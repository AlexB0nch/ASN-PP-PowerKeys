# Sprint 01 — Backlog

| ID | Задача | Компонент | Оценка (SP) | Статус |
|----|--------|-----------|-------------|--------|
| S01-001 | Инициализация VSTO-проекта (COM Add-in) | setup | 5 | Todo |
| S01-002 | `ShortcutManager` — регистрация и перехват клавиш | Core | 8 | Todo |
| S01-003 | `CommandDispatcher` — роутинг команд по категориям | Core | 5 | Todo |
| S01-004 | `AlignmentCommands` — 18 команд выравнивания | Commands | 8 | Todo |
| S01-005 | `ResizeCommands` — 20 команд изменения размера | Commands | 8 | Todo |
| S01-006 | `RibbonTab.xml` — вкладка PowerKeys в Ribbon | UI | 5 | Todo |
| S01-007 | `ColorSchemeReader` — чтение палитры Slide Master | Core | 3 | Todo |
| S01-008 | Базовый Shortcut Manager (просмотр/переопределение) | UI | 5 | Todo |

## Acceptance criteria

### S01-001 — Инициализация VSTO-проекта
- [ ] Solution собирается в Visual Studio (Debug/Release, x86 и x64)
- [ ] Add-in регистрируется per-user без прав администратора
- [ ] При F5 PowerPoint запускается с загруженным плагином

### S01-002 — ShortcutManager
- [ ] Глобальные шорткаты перехватываются в активной презентации
- [ ] Шорткаты читаются из `Settings/UserSettings.json`
- [ ] Поддержка модификаторов Alt/Ctrl/Shift и функциональных клавиш

### S01-004 — AlignmentCommands
- [ ] Выравнивание идёт относительно последнего выделенного объекта
- [ ] Все 18 команд работают на выделении из 2+ объектов
- [ ] Корректное поведение при <2 выделенных объектах (no-op)

### S01-005 — ResizeCommands
- [ ] Same width/height по опорному объекту
- [ ] Вариант с сохранением пропорций
- [ ] Шаги изменения размера (большой/малый) работают стрелками

### S01-006 — RibbonTab.xml
- [ ] Вкладка PowerKeys видна в ленте PowerPoint
- [ ] Группы Выравнивание/Размер с рабочими кнопками

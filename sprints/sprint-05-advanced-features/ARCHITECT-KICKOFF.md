# Кикофф для architect — Sprint 05 (планирование)

> **Роль этой сессии:** декомпозиция Sprint 05 в задачи `S05-0YY`, приоритизация, постановка **первой** задачи builder.
> Sprint 04 Done — `sprint-04-smart-color-picker/retrospective.md`.

## 1. Где мы (2026-06-28)
| Sprint | PR | Итог |
|--------|-----|------|
| 01 | #7–#15 | Infra, PowerPoint Online, VDS |
| 02 | #16–#22 | 76 commands wired |
| 03 | #23–#27 | Settings, Shortcut Manager |
| 04 | #29–#31 | Smart Color Picker, theme+recent |

**76 tests** passed. AddIn + Api + Core стабильны.

## 2. Продуктовый backlog (README)
| Фича | Описание | Office Web (ожидание) |
|------|----------|------------------------|
| **Consulting Mode** | McKinsey/BCG shortcut profiles; grid 0.1cm; snap to object | Profiles ✅ (Settings API); grid/snap ⚠️ |
| **Slide Backup** | Move slides to Backup section at end | ⚠️ Partial (slides API) |
| **Multi-slide paste** | Object on multiple selected slides | ⚠️ Partial |
| Smart Duplicate gap | `DuplicationEngine` + gap=0 today | ⚠️ |
| Object Statistics | Addup done; MIN/MAX/AVG in UI | ✅ partial |

## 3. Существующие крючки в коде
- `UserSettings.profile` + `ShortcutManager` + `CreateDefaults()` — профили можно расширить
- `POST /api/objects/duplicate-offset` — параметр `gap` (не используется AddIn)
- `CopySlide` — duplicate one slide (S02-005)
- `LayoutEngine` — вся геометрия через Core
- Нет команд в `CommandCatalog` для Backup/Multi-slide — **architect решает**: новые CommandIds vs host-only UI

## 4. Рекомендуемая декомпозиция (черновик — architect уточняет)
| ID | Тема | Рationale |
|----|------|-----------|
| **S05-001** | Consulting profiles (McKinsey/BCG presets → UserSettings) | Низкий risk, переиспользует S03 |
| **S05-002** | Snap-to-grid 0.1 cm (Core + apply) | Consulting Mode core value |
| **S05-003** | Slide Backup Manager | Slide API, отдельная сложность |
| **S05-004** | Multi-slide shape paste | Office.js multi-slide selection |
| S05-005 | Smart Duplicate gap memory | Малый scope, optional |

**Приоритет architect:** начать с **S05-001** (profiles) или **S05-003** (Backup) — зафиксируй в backlog с обоснованием.

## 5. Процесс
1. Прочитай `README.md` (инновационные функции), `CommandCatalog`, Office.js docs.
2. Заполни `goals.md` / `backlog.md` финальной декомпозицией.
3. Создай task-файл **первой** задачи → In Progress → `/builder`.
4. Полный цикл: приёмка → merge → следующая задача S05-00x.

## 6. Инварианты
- `ShapeBounds` boundary; anchor = last selected
- `VstoLegacy*` frozen

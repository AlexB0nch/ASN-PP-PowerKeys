# Кикофф для архитектора (новая сессия) — Sprint 02

> Входная точка для **новой сессии `/architect`**. Прочитай этот файл, затем —
> `docs/PRODUCT_CONTEXT.md`, `docs/TASK_MANAGEMENT.md`, `goals.md` и `backlog.md`.

## 1. Где мы сейчас
Сквозной путь **работает в живом PowerPoint Online** (2026-06-28):
- **Add-in** — GitHub Pages: `https://alexb0nch.github.io/ASN-PP-PowerKeys/` (`manifest.prod.xml`, Id `5b0ca36f-...`).
- **API** — VDS `https://95.140.152.103.sslip.io` (Caddy + Docker, deploy `deploy-vds.yml` на push в `main`).
- **CORS** для GitHub Pages — настроен.
- Панель рендерит **76 команд**; **S02-001…005 Done** (Objects, Format, Text, Alignment, Slides). Следующий — **S02-006** (единый UX `support=None`).

## 2. Главная проблема (фокус Sprint 02)
В `runCommand.ts` default «not wired up yet» **больше не срабатывает** для каталоговых команд.
**S02-006:** единый UX для `support=None` (бейджи, консистентные сообщения).

## 3. Инварианты
- Математика layout — только в `Core` (`ShapeBounds`); anchor = последняя выделенная.
- `Api/Contracts` ↔ `AddIn/src/services/types.ts` в синхроне.
- `VstoLegacy*` не трогать.
- ServerLayout / HostScript / явная деградация — не «not wired up yet».

## 4. Ограничения Office.js на Web
См. `docs/migration/01-vsto-to-officejs-mapping.md` и `CommandCatalog`. Slides: view/zoom/print — **None**;
`CopySlide` — **Partial** (`slides.add` / insert API в новых requirement sets).

## 5. План задач Sprint 02 (актуальный)
| ID | Статус | Содержание |
|----|--------|------------|
| S02-001 | Done (#16) | Objects |
| S02-002 | Done (#17) | Format |
| S02-003 | Done (#18) | Text |
| S02-004 | Done (#19) | Alignment: edge-align + copy-and-align |
| **S02-005** | **Done (#21)** | **Slides: CopySlide + деградация view/print** |
| **S02-006** | **Todo** | **Единая деградация `support=None` + UX бейджи** |

## 6. Процесс
1. Task-файл в `sprints/sprint-02-functionality/tasks/` + строка в `backlog.md`.
2. `/builder выполни S02-0YY`.
3. Приёмка + merge PR; обновить `docs/PRODUCT_CONTEXT.md` при продуктовых решениях.

## 7. Полезные файлы
- `src/PptPowerKeys.AddIn/src/taskpane/runCommand.ts`, `src/office/powerpoint.ts`
- `src/PptPowerKeys.Core/Commands/CommandCatalog.cs`
- `docs/migration/01-vsto-to-officejs-mapping.md` (секция Slides)

## 8. Долги (необязательно)
- Пин SSH host key в `deploy-vds.yml`
- Свой домен вместо `sslip.io`
- Персистентный `SettingsStore` + SSO

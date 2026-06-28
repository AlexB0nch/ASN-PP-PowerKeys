# Sprint 02 — Retrospective

_Завершён 2026-06-28. Все задачи S02-001…006 Done._

## Что прошло хорошо

- **Функциональный паритет закрыт:** все 76 каталоговых команд wired — ServerLayout, HostScript, Settings или явная деградация `support=None`.
- **Инвариант `ShapeBounds` сохранён:** edge-align и copy-and-align вынесены в Core/HostScript без дублирования математики на клиенте.
- **Сквозной путь в PowerPoint Online подтверждён:** Pages + VDS, 76 команд по категориям.
- **Деградация предсказуема:** S02-006 централизовал 9 None-команд в `unsupportedWebCommands.ts` и убрал ложные красные «Error» в UI.

## Что улучшить

- Settings UI (Shortcut Manager, Color Scheme) — только заглушки `execution=Settings`; отдельный спринт.
- Нет автотеста покрытия всех `runCommand` case (guard только для None-команд в каталоге).
- Ручная проверка в PowerPoint Online остаётся post-merge (вне CI на Linux).

## Action items

- Sprint 03: Settings UI + персистентный `SettingsStore`.
- Опционально: свой домен вместо `sslip.io`, pin SSH host key в `deploy-vds.yml`.
- Smart Color Picker / Slide Master palette — backlog следующих спринтов.

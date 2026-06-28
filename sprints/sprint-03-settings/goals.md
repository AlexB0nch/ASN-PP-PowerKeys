# Sprint 03 — Settings UI, персистентность, Shortcut Manager

> Контекст: Sprint 02 завершён — все 76 команд wired. Settings-команды сейчас заглушки.
> Кикофф для архитектора — [`ARCHITECT-KICKOFF.md`](./ARCHITECT-KICKOFF.md).

## Цель спринта
Дать пользователю **рабочий Settings UI** в task pane: загрузка/сохранение настроек через API,
**персистентное** хранилище на VDS (не in-memory), **Shortcut Manager** для редактирования привязок клавиш.

## Цели (декомпозирует architect в задачи `S03-0YY`)
- [ ] **Персистентный `SettingsStore`** — JSON на диске (Docker volume на VDS), переживает рестарт API.
- [ ] **Settings panel** в AddIn — загрузка при старте, save/reset через `/api/settings`.
- [ ] **Shortcut Manager UI** — список команд + keys, редактирование, валидация, сохранение.
- [ ] Wiring Settings-команд: `OpenShortcutManager`, `ResetToDefaults` (и `OpenColorScheme` — stub или минимальный UI).
- [ ] Тесты Api/Core для persistence; `dotnet test`, `npm run typecheck/validate:prod`.
- [ ] Документация деплоя (volume path, env vars).

## Ограничение Office Web (важно)
Office Web Add-in **не перехватывает глобальные hotkeys** как VSTO. Shortcut Manager в Sprint 03 —
**хранение и редактирование привязок** + справочник; фактическое срабатывание шорткатов в Web может быть
ограничено (architect явно фиксирует scope в task-файлах). SSO / `getAccessToken()` для `X-User-Id` — опционально
(S03 или отдельная задача); до SSO — anonymous или client-generated id в localStorage.

## Вне фокуса
- Smart Color Picker / Slide Master palette (Sprint 04+)
- Полноценный SSO / Azure AD
- VstoLegacy изменения

## Definition of Done спринта
- Настройки сохраняются между рестартами API на VDS
- Settings UI открывается из категории Settings и из команд OpenShortcutManager / ResetToDefaults
- Трассировка `S03-0YY`: task → backlog → PR → merge

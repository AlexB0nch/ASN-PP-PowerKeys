# Установка PptPowerKeys на PowerPoint Desktop (Windows)

Runbook для **PowerPoint Desktop for Windows** (Microsoft 365 / Office 2016+).  
Production-манифест **тот же**, что и для PowerPoint Online — отдельный `.msi` не нужен.

> См. также: [деплой и Online](./02-powerpoint-web-deploy.md) · [архитектура](./00-architecture.md)

## Что вы получаете

| | Web Add-in (текущий продукт) | VSTO legacy (`VstoLegacy*`, frozen) |
|--|------------------------------|-------------------------------------|
| Установка | Sideload `manifest.prod.xml` | `.msi` / COM (не поддерживается в CI) |
| UI | Кнопка **PowerKeys** на вкладке **Home** + task pane | Отдельная вкладка Ribbon (замысел README) |
| Команды | **79** — клик в task pane | Не развивается |
| Глобальные шорткаты | **См. раздел ниже** — сейчас не работают как в VSTO | Замысел COM-перехвата (не реализован в legacy) |
| Интернет | Нужен (статика GitHub Pages + API на VDS) | Offline возможен |

---

## Быстрая установка (production)

### Требования

- **Windows 10/11**
- **PowerPoint Desktop** (Microsoft 365 или Office 2016+)
- **Интернет** — task pane грузится с GitHub Pages, команды — с API
- Актуальный клон репозитория (`git pull origin main`)

### Шаги

1. **Обновите репозиторий**
   ```bash
   git pull origin main
   ```

2. **Возьмите манифест** (только production):
   ```
   src/PptPowerKeys.AddIn/manifest.prod.xml
   ```
   Не используйте `manifest.xml` / `manifest.dev.xml` — там `localhost`.

3. **Удалите старую версию** (если ставили раньше):
   - PowerPoint → **Insert** → **My Add-ins**
   - Найдите **PptPowerKeys** / **PptPowerKeys (Web)** → **Remove**  
   (сбрасывает кэш dev-манифеста с localhost)

4. **Загрузите надстройку**
   - **Insert** → **Get Add-ins** (или **Add-ins**)
   - **Upload My Add-in** → выберите `manifest.prod.xml`

5. **Откройте панель**
   - Вкладка **Home** → группа **PowerKeys** → кнопка **PowerKeys**
   - Должен открыться task pane с каталогом команд (79 шт.)

6. **Проверка backend** (опционально):
   - В браузере: `https://95.140.152.103.sslip.io/health` → `{"status":"ok"}`
   - В панели не должно быть «Cannot reach backend»

### Ожидаемый результат

| Компонент | URL |
|-----------|-----|
| Task pane (статика) | `https://alexb0nch.github.io/ASN-PP-PowerKeys/taskpane.html` |
| API | `https://95.140.152.103.sslip.io` |
| Display name | **PptPowerKeys (Web)** |
| Manifest Id | `5b0ca36f-a511-4705-a5e2-9609ff931f85` |

---

## Локальная разработка (Windows)

Если меняете код AddIn/API локально:

```powershell
# Terminal 1 — API
cd src\PptPowerKeys.Api
dotnet run

# Terminal 2 — Add-in (HTTPS localhost)
cd src\PptPowerKeys.AddIn
npm ci
npx office-addin-dev-certs install   # один раз: доверие к dev-сертификату
npm start
```

Sideload **`manifest.dev.xml`** (или `manifest.xml`) через **Upload My Add-in**.  
Панель будет на `https://localhost:3000`, API — `https://localhost:7168` (или порт из `dotnet run`).

---

## Установка через IT (организация)

Для развёртывания на многих рабочих местах:

1. **Centralized Deployment** — Microsoft 365 Admin Center → Integrated apps → upload `manifest.prod.xml`
2. **Trusted Catalog (Shared Folder)** — manifest в сетевой папке + реестр доверенного каталога Office

Подробности Microsoft: [Deploy Office Add-ins](https://learn.microsoft.com/en-us/office/dev/add-ins/publish/publish).

---

## Устранение неполадок

| Симптом | Причина | Решение |
|---------|---------|---------|
| «localhost refused to connect» | Загружен dev-манифест или кэш старой надстройки | Remove add-in → загрузить **только** `manifest.prod.xml` |
| «Cannot reach backend» | API недоступен / firewall | Проверить `/health` на VDS; интернет |
| Панель пустая / ошибка загрузки | GitHub Pages недоступен | Проверить URL статики в браузере |
| Команда «Not available on PowerPoint Web» | `support=None` (9 команд) | Ожидаемая деградация — используйте host (Print, Slide Show и т.д.) |
| Alt+1 / Alt+D не работают | Глобальные шорткаты **не зарегистрированы** | См. [Глобальные шорткаты](#глобальные-шорткаты-windows) |

---

## Глобальные шорткаты (Windows)

### Как сейчас (без доработок кода)

**Глобальные клавиши (Alt+1, Alt+D, …) не перехватываются.**  
Shortcut Manager в Settings **хранит и редактирует** привязки, но Office Web Add-in в текущей архитектуре **не регистрирует** их в PowerPoint.

**Как работать сегодня:**

1. Откройте task pane (**Home → PowerKeys**).
2. Выделите фигуры в слайде.
3. **Кликните команду** в каталоге (поиск по названию вверху панели).
4. Профили McKinsey/BCG и snap-to-grid — в **Settings** внутри панели.

Это ограничение **Office Web Add-in**, не баг установки. На PowerPoint **Desktop** поведение то же, что в браузере.

### Как сделать глобальные шорткаты (пути)

#### Путь A — официальный API Microsoft (рекомендуемый для продукта, **ещё не реализован**)

Microsoft поддерживает **custom keyboard shortcuts** для Office Add-ins в **PowerPoint Desktop on Windows**:

| Требование | Значение |
|------------|----------|
| PowerPoint Windows | **Version 2601** (Build 19628.20150) или новее |
| Requirement set | `KeyboardShortcuts 1.1` + **`SharedRuntime 1.1`** |
| Конфигурация | Расширения в манифесте + `shortcuts.json` (или `ExtendedOverrides`) |
| Код | `Office.actions.associate(actionId, handler)` + опционально `Office.actions.replaceShortcuts()` для синхронизации с UserSettings |

Документация: [Add custom keyboard shortcuts to your Office Add-ins](https://learn.microsoft.com/en-us/office/dev/add-ins/design/keyboard-shortcuts).

**Что потребует доработки в репозитории (кандидат Sprint 06):**

1. Миграция AddIn на **shared runtime** (сейчас task pane + отдельный `commands.html`).
2. Объявление action id для команд в `shortcuts.json` / manifest extensions.
3. Связка `Office.actions.associate` → вызов существующего `runCommand`.
4. При Save в Shortcut Manager — `Office.actions.replaceShortcuts()` из сохранённых bindings (где API доступен).
5. Проверка конфликтов: `Office.actions.areShortcutsInUse()`.

**Ограничения API (даже после реализации):**

- Сочетания только с **Ctrl / Alt / Shift** (как в guidelines Microsoft).
- Нужно избегать конфликтов с **встроенными** шорткатами PowerPoint.
- Каждое действие должно быть объявлено в манifest/JSON — не «любая клавиша на все 79 команд» без регистрации.
- На **PowerPoint Web** и **Mac** поддержка keyboard shortcuts **ограничена или отсутствует** (см. таблицы в MS docs) — Desktop Windows будет основной платформой для hotkeys.
- Фокус внутри task pane: часть комбинаций (Ctrl+Space и др.) зарезервирована для accessibility.

#### Путь B — VSTO legacy (не рекомендуется)

`src/PptPowerKeys.VstoLegacy*` — **заморожен**. Там задуман COM `ShortcutManager`, но полный перехват клавиш **не реализован** и не входит в активную разработку. Сборка только Windows + Visual Studio + VSTO.

Для production используйте Web Add-in (этот runbook), не VSTO.

#### Путь C — внешние утилиты (обходной, без поддержки)

AutoHotkey / PowerToys Keyboard Manager могут отправлять клавиши, но **не вызывают команды add-in** напрямую. Практичны только для открытия task pane или UI-автomation — хрупко, не документируем как официальный путь.

### Проверка версии PowerPoint (для будущих hotkeys)

PowerPoint → **File** → **Account** → **About PowerPoint** — смотрите **Version** и **Build**.  
Для Keyboard Shortcuts API нужен build **≥ 19628.20150** (канал Current / MEC 2604+).

---

## Mac Desktop

Тот же **`manifest.prod.xml`** через **Insert → Add-ins → Upload My Add-in**.  
`DesktopFormFactor` в манifest покрывает Mac. Глобальные шорткаты через Keyboard Shortcuts API на Mac для PowerPoint — см. [requirement sets](https://learn.microsoft.com/en-us/javascript/api/requirement-sets/common/keyboard-shortcuts-requirement-sets) (отдельные минимальные версии).

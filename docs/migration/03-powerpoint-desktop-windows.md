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
| Глобальные шорткаты | **76** hotkey-eligible на PP Desktop Win 2601+ (S06-002); Web — task pane | Замысел COM-перехвата (не реализован в legacy) |
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

> **LTSC / perpetual Office без «Upload My Add-in»:** см. **Track 0** —
> [`05-ltsc-web-addin-central-deploy.md`](./05-ltsc-web-addin-central-deploy.md) (Centralized Deployment, Trusted Catalog, on-prem)
> или Product Line B [`04-powerpoint-ltsc-windows-native.md`](./04-powerpoint-ltsc-windows-native.md) (`PptPowerKeys.Windows`, S07+).

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
| Alt+1 / Alt+D не работают | Web / Mac / PP &lt; 2601, или конфликт с native PP | На Desktop Win 2601+ — Tier 1 hotkeys; иначе task pane. См. [Глобальные шорткаты](#глобальные-шорткаты-windows) |

---

## Глобальные шорткаты (Windows)

### Как сейчас (после S06-002)

**76 hotkey-eligible команд** зарегистрированы в `shortcuts.json` и связаны через `Office.actions.associate` на **PowerPoint Desktop Windows 2601+** (build ≥ 19628.20150).

**Tier 1 default keys** (14 шорткатов) заданы в `shortcuts.json`; остальные 62 команды без default key в JSON — пользовательские привязки приходят через `Office.actions.replaceShortcuts()` после Save в Settings.

| Клавиши | Команда |
|---------|---------|
| Alt+1 … Alt+8 | Align left … Distribute vertically |
| Alt+B, Alt+H | Same width, Same height |
| Alt+G | Fill color |
| F1 | Toggle zoom fit (ограничение Office.js — см. «Not on Web») |
| Alt+D, Alt+A | Duplicate right, Sum numeric fields (McKinsey preset) |

Профили **McKinsey/BCG**: выберите профиль → **Save** → live hotkeys обновляются (например BCG `Ctrl+Alt+B` для Same width, `Alt+Shift+D` для Duplicate right).

Требования в манифесте: `SharedRuntime 1.1` + `KeyboardShortcuts 1.1`, `shortcuts.json` через `ExtendedOverrides`, shared runtime на `taskpane.html`.

**На PowerPoint Web / Mac / старом Desktop:** hotkeys **не активны** (graceful degradation) — используйте кнопки в task pane. Feature detection: `Office.context.requirements.isSetSupported('KeyboardShortcuts', '1.1')`.

**Shortcut Manager** хранит привязки в `UserSettings`; синхронизация live hotkeys — `syncKeyboardShortcuts()` → `replaceShortcuts` на bootstrap, Save и Reset.

### Как работать

1. Установите актуальный `manifest.prod.xml` (см. [Быстрая установка](#быстрая-установка-production)).
2. **Desktop Windows 2601+:** выделите фигуры → нажмите Alt+1, Alt+D и т.д.
3. **Web / иные платформы:** откройте task pane (**Home → PowerKeys**) → кликните команду в каталоге.
4. Профили McKinsey/BCG и snap-to-grid — в **Settings** внутри панели.

### История / ограничения

До Sprint 06 глобальные клавиши не перехватывались — только клики в task pane. Сейчас 76 команд зарегистрированы; Tier 1 defaults + пользовательские bindings через `replaceShortcuts`.

**Ограничения API:**

- Сочетания только с **Ctrl / Alt / Shift** (как в guidelines Microsoft); F1 — исключение для ToggleZoom.
- Возможны конфликты с **встроенными** шорткатами PowerPoint (диалог Microsoft).
- Каждое действие объявлено в `shortcuts.json` — не «любая клавиша на все 79 команд» без регистрации.
- Settings-команды (`OpenShortcutManager`, `OpenColorScheme`, `ResetToDefaults`) **не** имеют глобальных hotkeys.

### Как было (до S06-001)

Глобальные клавиши не перехватывались — только клики в task pane на всех платформах.

#### Дальнейшая работа (post Sprint 06)

- Расширенная диагностика конфликтов hotkeys (beyond `areShortcutsInUse` warning).
- Snap-to-nearest-object, slide sections hide/show — нет Office.js API (см. README backlog).

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

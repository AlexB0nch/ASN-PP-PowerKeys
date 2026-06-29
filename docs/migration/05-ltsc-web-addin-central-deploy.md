# Track 0 — Web Add-in на LTSC без «Upload My Add-in»

> Доставка **существующего Office Web Add-in** (Product Line A) на PowerPoint LTSC / perpetual Office,
> когда пользователь **не видит** кнопку Upload My Add-in.  
> Native line (Product Line B): [`04-powerpoint-ltsc-windows-native.md`](./04-powerpoint-ltsc-windows-native.md)

## Когда Track 0 достаточно

| Требование | Track 0 Web | PptPowerKeys.Windows (B) |
|------------|-------------|--------------------------|
| Корпоративный деплой без Upload UI | ✅ Centralized Deployment / Trusted Catalog | ✅ Signed MSI (S11) |
| Интернет к статике + API | **Обязателен** | Опционален (layout offline) |
| 9 команд `OfficeJsSupport.None` | ❌ | ✅ |
| Global hotkeys на PP &lt; 2601 | ❌ (нужен KeyboardShortcuts 1.1) | ✅ Native hook (S11) |
| Mac / Web / iPad | ✅ | ❌ |

**Decision tree:**

```
LTSC / perpetual Office на Windows?
├─ IT может развернуть Web manifest централизованно?
│  ├─ Да → нужны ли 9 None-команд / offline layout / global hotkeys на старом PP?
│  │  ├─ Нет → Track 0 (этот документ)
│  │  └─ Да → Product Line B (PptPowerKeys.Windows)
│  └─ Нет → Product Line B или смена политики IT
└─ M365 с Upload UI → [03-powerpoint-desktop-windows.md](./03-powerpoint-desktop-windows.md)
```

---

## Вариант A — Microsoft 365 Admin Center (Centralized Deployment)

Подходит для организаций с **Microsoft 365 admin access**, даже если десктоп — LTSC channel
(зависит от политик tenant; уточните у IT).

### Чеклист IT admin

1. **Подготовить manifest**
   - Файл: `src/PptPowerKeys.AddIn/manifest.prod.xml` из репозитория (production Id `5b0ca36f-...`).
   - Убедиться, что URL в манифесте доступны из корпоративной сети:
     - Task pane: `https://alexb0nch.github.io/ASN-PP-PowerKeys/...`
     - API: `https://95.140.152.103.sslip.io` (или ваш on-prem mirror — см. вариант C).

2. **M365 Admin Center**
   - [admin.microsoft.com](https://admin.microsoft.com) → **Settings** → **Integrated apps** → **Upload custom apps**
   - Upload `manifest.prod.xml`
   - Назначить пользователям / группам / всей организации

3. **Проверка на клиенте**
   - Перезапуск PowerPoint (или ожидание policy sync, до 24 ч)
   - **Insert** → **My Add-ins** → надстройка **PptPowerKeys (Web)** в списке
   - **Home** → **PowerKeys** → task pane открывается, каталог 79 команд

4. **Troubleshooting**
   - «Cannot reach backend» — firewall/proxy блокирует API URL
   - Пустой каталог — CORS / API down; проверить `GET /health`
   - Add-in не появился — проверить assignment в Admin Center, версию Office

---

## Вариант B — Trusted Catalog (SharePoint / file share)

Классический путь для **on-prem / air-gapped partial**: общая папка + registry.

### Чеклист IT admin

1. **Разместить manifest**
   - Скопировать `manifest.prod.xml` на **internal HTTPS** или trusted UNC share
   - Пример: `\\fileserver\OfficeAddins\PptPowerKeys\manifest.prod.xml`
   - Для HTTPS catalog: `https://intranet.company.com/addins/pptpowerkeys/manifest.prod.xml`

2. **Registry (per-machine или GPO)**
   ```
   HKLM\Software\Microsoft\Office\16.0\WEF\TrustedCatalogs
     └── {GUID}
           CatalogUrl = (URL или UNC к папке с manifest)
           Flags = 1
   ```
   Документация Microsoft: [Office Add-ins deployment](https://learn.microsoft.com/en-us/office/dev/add-ins/publish/publish)

3. **Office Cache**
   - При смене manifest: очистить `%LOCALAPPDATA%\Microsoft\Office\16.0\Wef\` (осторожно — сброс всех sideload)

4. **Проверка**
   - PowerPoint → **Insert** → **My Add-ins** → **SHARED FOLDER** / catalog tab
   - Загрузка **PptPowerKeys (Web)** без Upload UI

---

## Вариант C — On-prem static + API (полный air-gap для Web line)

Если GitHub Pages и публичный VDS **заблокированы**, но Web Add-in всё ещё предпочтителен:

| Компонент | Production default | On-prem |
|-----------|-------------------|---------|
| Task pane static | GitHub Pages | Internal IIS/nginx + TLS |
| API | VDS HTTPS | Internal `dotnet` host + reverse proxy |
| Manifest | `manifest.prod.xml` | Клон с заменой `<SourceLocation>` и ApiUrl в AddIn config |

Шаги (кратко):

1. `npm run build:prod` в `src/PptPowerKeys.AddIn`
2. Развернуть `dist/` на internal HTTPS
3. Развернуть `PptPowerKeys.Api` (Docker или IIS)
4. Сгенерировать manifest из `manifest.template.xml` с internal URL (`npm run build:manifest` + env)
5. Распространить manifest через вариант A или B

Подробности деплоя компонентов: [`02-powerpoint-web-deploy.md`](./02-powerpoint-web-deploy.md).

---

## Ограничения Track 0 (не исправляются manifest deploy)

1. **9 команд** с `OfficeJsSupport.None` — FormatPainter, PasteFormatted, Regroup, view toggles, PrintSlide и др. — остаются unsupported в Web line.
2. **Global keyboard shortcuts** — требуют PowerPoint Desktop **Version 2601+** и requirement set `KeyboardShortcuts 1.1`; на LTSC 2019/2021 без обновления — только шорткаты внутри task pane (shared runtime).
3. **Зависимость от сети** — без on-prem mirror (вариант C) task pane и ServerLayout-команды требуют доступ к API.
4. **Нет COM / VBA** — макросы и PPAM не являются Track 0.

---

## Связанные документы

- [03-powerpoint-desktop-windows.md](./03-powerpoint-desktop-windows.md) — sideload с Upload UI (M365 / Office 2016+)
- [04-powerpoint-ltsc-windows-native.md](./04-powerpoint-ltsc-windows-native.md) — Product Line B (`PptPowerKeys.Windows`)
- [02-powerpoint-web-deploy.md](./02-powerpoint-web-deploy.md) — production URLs, CI deploy
- [`docs/PRODUCT_CONTEXT.md`](../PRODUCT_CONTEXT.md) — dual product line journal

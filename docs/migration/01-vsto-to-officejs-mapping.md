# Story 1 — Аудит функционала и маппинг VSTO → Office.js

Этот документ — результат Story 1 эпика миграции. Он фиксирует **инвентарь команд**
текущей VSTO-надстройки и их соответствие [PowerPoint JavaScript API](https://learn.microsoft.com/en-us/javascript/api/powerpoint).

> Машиночитаемая версия этой таблицы живёт в коде:
> [`CommandCatalog`](../../src/PptPowerKeys.Core/Commands/CommandCatalog.cs) и доступна
> по REST: `GET /api/commands`. UI задней панели и эта таблица не расходятся, потому что
> обе берут данные из одного источника.

## Легенда

| Значок | Значение |
|--------|----------|
| ✅ Full | Прямой эквивалент в Office.js |
| ⚠️ Partial | Частичный эквивалент / workaround |
| ❌ None | Нет эквивалента (блокер) |

`Execution`:
- **ServerLayout** — геометрия считается в `LayoutEngine` (бэкенд), панель применяет результат.
- **HostScript** — выполняется целиком в панели через PowerPoint JS API.
- **Settings** — управление настройками панели.

## Состояние исходного VSTO-кода

Важный результат аудита: на момент миграции бизнес-логика во VSTO-проекте была
**в основном заглушками** (`*Commands.cs` пустые, `OnAction` логировал, `ColorSchemeReader`
возвращал пустые списки). Единственная содержательная логика — модель «якоря»
(`CommandContext.AnchorShape` = последняя выделенная фигура) и сериализация настроек.
Поэтому миграция фокусируется на **корректной целевой архитектуре и переносимой
бизнес-логике** (геометрия выравнивания/resize, агрегация чисел), а не на побайтовом
переносе несуществующих реализаций.

## Маппинг команд

### Alignment (выравнивание относительно якоря)

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| AlignLeft / Right / CenterHorizontal | `Shape.Left` относительно anchor | `shape.left` (setter) | ✅ Full | ServerLayout |
| AlignTop / Bottom / MiddleVertical | `Shape.Top` относительно anchor | `shape.top` | ✅ Full | ServerLayout |
| DistributeHorizontal / Vertical | ручной расчёт промежутков | `shape.left/top` | ✅ Full | ServerLayout |
| CopyAndAlignLeft/Right/Top/Bottom | Duplicate + Align | `copyTo` + setters | ⚠️ Partial | HostScript |
| AlignLeftToRight / RightToLeft / TopToBottom / BottomToTop | примыкание к ребру anchor | `shape.left/top` | ✅ Full | HostScript |
| CopyObjectPosition / PasteObjectPosition | сохранение/применение позиции | состояние панели + setters | ✅ Full | HostScript |

### Resize (изменение размера относительно якоря)

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| SameWidth / SameHeight | `Shape.Width/Height` | `shape.width/height` | ✅ Full | ServerLayout |
| SameWidthKeepAspect / SameHeightKeepAspect | масштаб с аспектом | `shape.width/height` | ✅ Full | ServerLayout |
| WidthEqualsAnchorHeight / HeightEqualsAnchorWidth | кросс-присваивание | `shape.width/height` | ✅ Full | ServerLayout |
| StretchWidth/HeightTo{Left,Right,Top,Bottom} | растяжение до ребра anchor | `shape.left/top/width/height` | ✅ Full | ServerLayout |
| Increase/Decrease Width/Height (Large/Small) | шаговое изменение | `shape.width/height` | ✅ Full | ServerLayout |
| Increase/DecreaseSizeKeepAspect | масштаб с аспектом | `shape.width/height` | ✅ Full | ServerLayout |

### Objects

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| InsertRectangle / Square / Ellipse | `Shapes.AddShape` | `shapes.addGeometricShape(...)` | ✅ Full | HostScript |
| InsertLine | `Shapes.AddLine` | `shapes.addLine()` | ✅ Full | HostScript |
| InsertArrow | `AddLine` + arrowhead | `addLine` + `lineFormat` | ⚠️ Partial | HostScript |
| InsertTextbox | `Shapes.AddTextbox` | `shapes.addTextBox()` | ✅ Full | HostScript |
| Duplicate {Right,Left,Down,Up} | `Shape.Duplicate` + сдвиг | `copyTo`/`duplicate` + `DuplicationEngine` | ⚠️ Partial | HostScript |
| Group / Ungroup | `ShapeRange.Group` | `shapes.addGroup` / `group.ungroup` | ✅/⚠️ | HostScript |
| Regroup | `ShapeRange.Regroup` | — | ❌ None | HostScript |
| BringToFront / SendToBack / Forward / Backward | `Shape.ZOrder` | `shape.setZOrder(...)` | ✅ Full | HostScript |

### Format

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| FillColor | `Shape.Fill.ForeColor` + Slide Master | `shape.fill.setSolidColor()` | ⚠️ Partial | HostScript |
| LineColor | `Shape.Line.ForeColor` | `shape.lineFormat.color` | ⚠️ Partial | HostScript |
| TextColor | `TextRange.Font.Color` | `textRange.font.color` | ✅ Full | HostScript |
| ToggleFillBlackWhite | переключение заливки | `shape.fill.setSolidColor()` | ✅ Full | HostScript |
| FormatPainter | `Shape.PickUp/Apply` | — | ❌ None | HostScript |

### Text

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| PasteUnformatted | clipboard | чтение текста в панели | ⚠️ Partial | HostScript |
| PasteFormatted | rich clipboard | — | ❌ None | HostScript |
| AddupTextFields | парсинг + сумма | `NumberAggregator` (бэкенд) | ✅ Full | HostScript |
| ReplaceWithEllipsis | замена текста | `textRange.text` | ✅ Full | HostScript |
| ToggleSuperscript / Subscript | `Font.Superscript` | `font.superscript/subscript` | ⚠️ Partial | HostScript |

### Slides

| Команда | VSTO (COM) | Office.js | Support | Execution |
|---|---|---|---|---|
| ToggleZoom | `View.Zoom` | — | ❌ None | HostScript |
| ToggleSlideSorter | `ViewType` | — | ❌ None | HostScript |
| StartSlideShow | `SlideShowSettings.Run` | — | ❌ None | HostScript |
| ToggleGrid / ToggleGuides | настройки вида | — | ❌ None | HostScript |
| CopySlide | `Slide.Duplicate` | `slides.insert/clone` (новые API) | ⚠️ Partial | HostScript |
| MoveSlidesToBackup | — (README parity) | `slide.moveTo` / export+insert+delete | ⚠️ Partial | HostScript |
| PrintSlide | `PrintOut` | — | ❌ None | HostScript |

### Settings

| Команда | Office.js | Support |
|---|---|---|
| OpenShortcutManager / OpenColorScheme / ResetToDefaults | task pane UI + `/api/settings` | ✅ Full |

## Блокеры (❌) и стратегия

| Блокер | Причина | Стратегия |
|---|---|---|
| ToggleZoom, ToggleSlideSorter, StartSlideShow, ToggleGrid/Guides, PrintSlide | Office.js не управляет view/zoom/печатью | Оставить во VSTO для desktop-only; в Web — скрыть/деградировать |
| FormatPainter | нет API копирования формата | эмуляция: считать набор свойств формата и применить вручную |
| Regroup | нет API regroup | хранить членство группы в состоянии панели |
| PasteFormatted | нет rich-clipboard API | только plain text |

Эти операции — именно те «low-level COM операции», о которых предупреждает раздел
«Риски» эпика. Они известны заранее (цель Story 1) и не блокируют приоритетные фичи
(alignment / resize / objects), которые имеют полный эквивалент.

# Sprint 05 — Retrospective

> **Статус:** завершён (2026-06-29). Все задачи Done (включая optional S05-005).

## Итоги

| ID | Issue | PR | Результат |
|----|-------|-----|-----------|
| S05-001 | — | #32 | Consulting profiles McKinsey/BCG → `ConsultingProfilePresets`, Settings dropdown |
| S05-002 | #33 | #34 | Snap-to-grid 0.1 cm — `GridSnap` в Core, `UserSettings.snapToGrid` |
| S05-003 | #35 | #36 | `MoveSlidesToBackup` (77-я команда) — HostScript move/export fallback |
| S05-004 | #37 | #39 | `PasteShapeToSelectedSlides` / `RemoveShapeFromSelectedSlides` (78–79-я команды) |
| S05-005 | #41 | #42 | Smart Duplicate gap memory — `InferGap` + `duplicateGapMemory.ts` |

## Definition of Done спринта — выполнено

- [x] **S05-001** — Consulting profiles (McKinsey/BCG presets → UserSettings)
- [x] **S05-002** — Snap-to-grid 0.1 cm (Core + layout apply)
- [x] **S05-003** — Slide Backup Manager (`MoveSlidesToBackup`)
- [x] **S05-004** — Multi-slide paste / remove (2 новых CommandIds)
- [x] (Optional) **S05-005** — Smart Duplicate gap memory
- [x] Трассировка `S05-0YY` → Issue → PR → merge
- [x] `dotnet test PptPowerKeys.sln` — зелёный
- [x] AddIn: `typecheck`, `validate:prod` — зелёные

## Ключевые решения

- **Consulting profiles (S05-001):** McKinsey/BCG presets в Core (`ConsultingProfilePresets`); `GET /api/settings/profile-presets`; Settings dropdown заменяет shortcuts в editor — **без** новых CommandIds.
- **GridSnap (S05-002):** 0.1 cm grid в points; post-process в `LayoutEngine.Apply` при `LayoutOptions.SnapToGrid`; `UserSettings.snapToGrid` persist; stateless API.
- **Slide Backup (S05-003):** `MoveSlidesToBackup` — `slide.moveTo` preferred, export→insert→delete fallback; **без** slide sections API на Web.
- **Multi-slide (S05-004):** cross-slide clone через `cloneShapeOnSlide(..., crossSlide=true)` recreate path; remove by exact `shape.name`; каталог 77 → **79** команд.
- **Smart Duplicate gap (S05-005):** per-`CommandId` in-memory gap в task pane (`duplicateGapMemory.ts`); `DuplicationEngine.InferGap` в Core; первый duplicate — gap 0, повторный — remembered gap; **без** localStorage/UserSettings.

## Метрики

- `dotnet test`: **114** passed (было 108 после S05-004)
- **79** команд в каталоге (без изменений в S05-005)
- AddIn: `typecheck`, `validate:prod` — зелёные

## Anti-scope (соблюдено)

- Snap-to-nearest-object при drag/move
- Slide sections (создание/именование «Backup» section) и hide/show backup block
- Import/export settings JSON
- Object Statistics MIN/MAX/AVG UI
- `VstoLegacy*` — frozen

## Следующий спринт

**TBD** — кандидаты из README stretch:

- Object Statistics MIN/MAX/AVG UI (Addup уже в Core)
- Import/export settings JSON
- Eyedropper / HEX input (deferred из Sprint 04)

См. `sprints/` — architect определит Sprint 06 при планировании.

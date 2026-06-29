# S07-004 — Track 0 LTSC deploy checklist (Web Add-in without Upload UI)

> Передача builder'у: `/builder выполни S07-004`

## Метаданные

| Поле | Значение |
|------|----------|
| **Task ID** | `S07-004` |
| **Спринт** | `sprint-07-ltsc-foundation` |
| **Компонент** | docs |
| **Статус** | Done |

## Цель

Документ **Track 0**: как доставить **существующий Web Add-in** на LTSC **без** «Upload My Add-in»
(Centralized Deployment, Trusted Catalog, on-prem static+API).

## Scope

| Item | Detail |
|------|--------|
| New section | `docs/migration/04-powerpoint-ltsc-windows-native.md` § Track 0 или отдельный `05-ltsc-web-addin-central-deploy.md` |
| Update | `03-powerpoint-desktop-windows.md` — ссылка + «если нет Upload UI» |
| Checklist | IT admin steps; when Track 0 enough vs when need PptPowerKeys.Windows |
| Limitations | 9 None commands; hotkeys require PP 2601+; internet required |

## Анти-scope

- Implementation code
- MSI native line

## Критерии приёмки

- [x] Checklist actionable for IT admin
- [x] Clear decision tree: Track 0 Web vs Product Line B native
- [x] Cross-links from README / PRODUCT_CONTEXT

## Зависимости

- None (parallel)

## Трассировка

Issue → docs-only PR

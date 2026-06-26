# /review-pr

Проверь изменения/PR перед мержем.

1. Запусти субагента `reviewer` (read-only).
2. Reviewer проверяет по `.github/review/CHECKLIST.md`: COM-утечки, edge cases, конфликты шорткатов, совместимость, слои, scope.
3. Вывод сгруппируй: Blocker / Major / Minor / Nit, по каждому — файл, строка, предложение.
4. Вердикт: Approve / Request changes.
5. На стороне GitHub PR дополнительно ревьюит Bugbot по `.cursor/BUGBOT.md`.

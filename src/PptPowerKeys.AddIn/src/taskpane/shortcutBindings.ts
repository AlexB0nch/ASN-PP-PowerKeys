import { ShortcutBinding } from "../services/types";

/** Mirrors Core ShortcutBindingValidator.NormalizeKeys. */
export function normalizeShortcutKeys(keys: string): string {
  return keys.trim();
}

export interface DuplicateKeyGroup {
  keys: string;
  commandIds: string[];
}

/** Mirrors Core ShortcutBindingValidator.FindDuplicateKeys (client-side). */
export function findDuplicateKeyGroups(shortcuts: ShortcutBinding[]): DuplicateKeyGroup[] {
  const byNormalized = new Map<string, { keys: string; commandIds: string[] }>();

  for (const binding of shortcuts) {
    const normalized = normalizeShortcutKeys(binding.keys);
    if (!normalized) {
      continue;
    }
    const lookup = normalized.toLowerCase();
    const existing = byNormalized.get(lookup);
    if (existing) {
      existing.commandIds.push(binding.commandId);
    } else {
      byNormalized.set(lookup, { keys: normalized, commandIds: [binding.commandId] });
    }
  }

  return [...byNormalized.values()].filter((g) => g.commandIds.length > 1);
}

export function isDuplicateKey(
  shortcuts: ShortcutBinding[],
  commandId: string,
  keys: string,
): boolean {
  const normalized = normalizeShortcutKeys(keys);
  if (!normalized) {
    return false;
  }
  const lookup = normalized.toLowerCase();
  return shortcuts.some(
    (b) =>
      b.commandId !== commandId &&
      normalizeShortcutKeys(b.keys).toLowerCase() === lookup,
  );
}

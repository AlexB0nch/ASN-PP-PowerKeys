import { ShortcutBinding } from "../services/types";

/** Mirrors Core ShortcutBindingValidator.NormalizeKeys. */
export function normalizeShortcutKeys(keys: string): string {
  return keys.trim();
}

const MODIFIER_NAMES: Record<string, string> = {
  ctrl: "Ctrl",
  control: "Ctrl",
  alt: "Alt",
  shift: "Shift",
};

/**
 * Converts user-entered shortcut text to Office Keyboard Shortcuts format
 * (e.g. "alt+shift+d" → "Alt+Shift+D").
 */
export function toOfficeShortcutKey(keys: string): string | null {
  const trimmed = normalizeShortcutKeys(keys);
  if (!trimmed) {
    return null;
  }

  const parts = trimmed.split("+").map((p) => p.trim()).filter(Boolean);
  if (parts.length === 0) {
    return null;
  }

  const normalized = parts.map((part) => {
    const modifier = MODIFIER_NAMES[part.toLowerCase()];
    if (modifier) {
      return modifier;
    }
    if (/^f\d+$/i.test(part)) {
      return part.toUpperCase();
    }
    if (part.length === 1) {
      return part.toUpperCase();
    }
    return part.charAt(0).toUpperCase() + part.slice(1).toLowerCase();
  });

  return normalized.join("+");
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

/**
 * Maps UserSettings bindings to Office replaceShortcuts payload.
 * Empty keys → null; duplicate keys → last binding wins.
 */
export function bindingsToOfficeMap(
  shortcuts: ShortcutBinding[],
): Record<string, string | null> {
  const result: Record<string, string | null> = {};
  const keyToCommand = new Map<string, string>();

  for (const binding of shortcuts) {
    const officeKey = toOfficeShortcutKey(binding.keys);
    if (!officeKey) {
      result[binding.commandId] = null;
      continue;
    }

    const lookup = officeKey.toLowerCase();
    const previous = keyToCommand.get(lookup);
    if (previous !== undefined) {
      result[previous] = null;
    }
    keyToCommand.set(lookup, binding.commandId);
    result[binding.commandId] = officeKey;
  }

  return result;
}

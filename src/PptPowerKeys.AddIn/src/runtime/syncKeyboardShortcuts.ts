import { UserSettings } from "../services/types";
import { bindingsToOfficeMap } from "../taskpane/shortcutBindings";

/* global Office */

function supportsKeyboardShortcuts(): boolean {
  return Office.context.requirements.isSetSupported("KeyboardShortcuts", "1.1");
}

/**
 * Applies UserSettings shortcut bindings to PowerPoint via Office.actions.replaceShortcuts.
 * No-op when KeyboardShortcuts 1.1 is unavailable (Web / older Desktop).
 */
export async function syncKeyboardShortcuts(settings: UserSettings): Promise<void> {
  if (!supportsKeyboardShortcuts()) {
    return;
  }

  const map = bindingsToOfficeMap(settings.shortcuts);
  if (Object.keys(map).length === 0) {
    return;
  }

  try {
    // Office docs allow null values to revert to shortcuts.json defaults; @types omit null.
    await Office.actions.replaceShortcuts(map as { [actionId: string]: string });
  } catch (err) {
    console.warn("replaceShortcuts failed:", err);
  }
}

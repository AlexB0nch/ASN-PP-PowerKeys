import { executeCommandById } from "./executeCommandById";
import { HOTKEY_ELIGIBLE_COMMAND_IDS } from "./hotkeyEligibleCommandIds";

/* global Office */

/**
 * Associates all hotkey-eligible action ids with handlers.
 * Call only when KeyboardShortcuts 1.1 is supported.
 */
export function registerCommandActions(): void {
  for (const commandId of HOTKEY_ELIGIBLE_COMMAND_IDS) {
    Office.actions.associate(commandId, async (event: Office.AddinCommands.Event) => {
      await executeCommandById(commandId);
      event.completed();
    });
  }
}

import { executeCommandById } from "./executeCommandById";
import { TIER1_COMMAND_IDS } from "./tier1Commands";

/* global Office */

/**
 * Associates Tier 1 action ids with handlers. Call only when KeyboardShortcuts 1.1 is supported.
 */
export function registerCommandActions(): void {
  for (const commandId of TIER1_COMMAND_IDS) {
    Office.actions.associate(commandId, async (event: Office.AddinCommands.Event) => {
      await executeCommandById(commandId);
      event.completed();
    });
  }
}

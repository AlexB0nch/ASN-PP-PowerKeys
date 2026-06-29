import { runCommand, CommandOutcome, outcomeError } from "../taskpane/runCommand";
import { findCommand, getLayoutOptions, getSettingsActions } from "./commandContext";

/**
 * Thin wrapper: resolve descriptor from cached catalog → existing runCommand path.
 */
export async function executeCommandById(commandId: string): Promise<CommandOutcome> {
  const descriptor = findCommand(commandId);
  if (!descriptor) {
    return outcomeError(`Unknown command: ${commandId}.`);
  }

  return runCommand(descriptor, getSettingsActions(), getLayoutOptions());
}

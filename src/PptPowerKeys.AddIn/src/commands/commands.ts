/* global Office */

// Function-file entry for ribbon commands. With shared runtime, this module is
// bundled into taskpane.js and registerRibbonActions() is called from bootstrap.

function showTaskpane(event: Office.AddinCommands.Event): void {
  event.completed();
}

/** Satisfies manifest FunctionFile contract; safe to call when KeyboardShortcuts API is absent. */
export function registerRibbonActions(): void {
  Office.actions.associate("showTaskpane", showTaskpane);
}

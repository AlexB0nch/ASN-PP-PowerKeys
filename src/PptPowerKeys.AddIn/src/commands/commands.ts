/* global Office */

// Function-file entry point for ribbon button commands. Currently the add-in is
// driven entirely from the task pane, but registering an action here keeps the
// manifest's FunctionFile contract satisfied and provides a home for future
// ribbon-triggered commands.

function showTaskpane(event: Office.AddinCommands.Event): void {
  // Placeholder ribbon command; the manifest points its button at the task pane
  // directly, so this simply completes.
  event.completed();
}

// Must run after Office.js is ready — calling associate earlier breaks PowerPoint Online.
Office.onReady(() => {
  Office.actions.associate("showTaskpane", showTaskpane);
});

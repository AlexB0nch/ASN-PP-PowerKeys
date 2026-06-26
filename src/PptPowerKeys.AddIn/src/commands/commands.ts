/* global Office */

// Function-file entry point for ribbon button commands. Currently the add-in is
// driven entirely from the task pane, but registering an action here keeps the
// manifest's FunctionFile contract satisfied and provides a home for future
// ribbon-triggered commands.

Office.onReady(() => {
  // No-op: ready for ribbon command registration.
});

function showTaskpane(event: Office.AddinCommands.Event): void {
  // Placeholder ribbon command; the manifest points its button at the task pane
  // directly, so this simply completes.
  event.completed();
}

// Office requires actions to be globally associated.
Office.actions.associate("showTaskpane", showTaskpane);

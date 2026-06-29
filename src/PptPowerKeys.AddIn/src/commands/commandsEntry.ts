import { registerRibbonActions } from "./commands";

/* global Office */

// Standalone entry for commands.html (legacy FunctionFile URL). Shared runtime
// uses taskpane.html; this keeps the commands chunk non-empty for old manifests.
Office.onReady(() => {
  registerRibbonActions();
});

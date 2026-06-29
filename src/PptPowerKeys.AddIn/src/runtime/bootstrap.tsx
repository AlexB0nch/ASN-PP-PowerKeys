import { createRoot } from "react-dom/client";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import { bootstrapThemeColors } from "../office/themeColors";
import { registerRibbonActions } from "../commands/commands";
import { App } from "../taskpane/App";
import { bootstrapCommandContext, getUserSettings } from "./commandContext";
import { registerCommandActions } from "./registerCommandActions";
import { syncKeyboardShortcuts } from "./syncKeyboardShortcuts";

/* global Office, document */

function supportsKeyboardShortcuts(): boolean {
  return Office.context.requirements.isSetSupported("KeyboardShortcuts", "1.1");
}

function mountReactApp(): void {
  const container = document.getElementById("root");
  if (!container) {
    return;
  }

  const root = createRoot(container);
  root.render(
    <FluentProvider theme={webLightTheme}>
      <App />
    </FluentProvider>,
  );
}

export function startAddIn(): void {
  Office.onReady(() => {
    void (async () => {
      await bootstrapThemeColors();
      try {
        await bootstrapCommandContext();
      } catch {
        // Catalog may be empty; App surfaces backend errors on mount.
      }

      registerRibbonActions();
      if (supportsKeyboardShortcuts()) {
        registerCommandActions();
        const settings = getUserSettings();
        if (settings) {
          await syncKeyboardShortcuts(settings);
        }
      }

      mountReactApp();
    })();
  });
}

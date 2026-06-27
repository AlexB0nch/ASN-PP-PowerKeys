import { createRoot } from "react-dom/client";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import { App } from "./App";

/* global Office, document */

Office.onReady(() => {
  const container = document.getElementById("root");
  if (!container) {
    return;
  }

  const root = createRoot(container);
  root.render(
    <FluentProvider theme={webLightTheme}>
      <App />
    </FluentProvider>
  );
});

/**
 * Central registry of HostScript commands that cannot run on PowerPoint Web.
 * Keys must stay in sync with CommandCatalog entries marked OfficeJsSupport.None.
 */
export const UNSUPPORTED_WEB_COMMANDS: Readonly<Record<string, string>> = {
  Regroup: "Regroup is not supported on PowerPoint Web.",
  FormatPainter: "Format painter is not supported on PowerPoint Web.",
  PasteFormatted: "Paste formatted is not supported on PowerPoint Web.",
  ToggleZoom: "Zoom is not available in PowerPoint Web. Use the host zoom controls.",
  ToggleSlideSorter: "Slide sorter view is not available in PowerPoint Web.",
  StartSlideShow:
    "Slide show cannot be started from the add-in on PowerPoint Web. Use Present / Slide Show in the host.",
  ToggleGrid: "Grid toggle is not available in PowerPoint Web.",
  ToggleGuides: "Guides toggle is not available in PowerPoint Web.",
  PrintSlide:
    "Printing is not available from the add-in. Use Ctrl+P (Cmd+P) or the host Print command.",
};

export function getUnsupportedWebMessage(commandId: string): string | undefined {
  return UNSUPPORTED_WEB_COMMANDS[commandId];
}

export function isUnsupportedWebCommand(commandId: string): boolean {
  return commandId in UNSUPPORTED_WEB_COMMANDS;
}

export function runUnsupportedWebCommand(commandId: string): {
  kind: "unsupported" | "error";
  message: string;
} {
  const message = getUnsupportedWebMessage(commandId);
  if (!message) {
    return { kind: "error", message: `Unknown unsupported command: ${commandId}.` };
  }
  return { kind: "unsupported", message };
}

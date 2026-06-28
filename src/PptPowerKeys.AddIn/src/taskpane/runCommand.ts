import { api } from "../services/api";
import { CommandDescriptor } from "../services/types";
import {
  applyFillColor,
  applyLineColor,
  applyShapeBounds,
  applyShapeBoundsOnSlide,
  applyTextColor,
  cloneSelectedShapesAtSourcePositions,
  copyObjectPosition,
  duplicateSelectedSlide,
  duplicateShapesAtPositions,
  getPositionClipboard,
  getSelectedShapeBounds,
  getSelectedShapeIds,
  getSelectedShapeTexts,
  groupSelectedShapes,
  insertShape,
  insertTextBox,
  pasteObjectPosition,
  pasteUnformattedText,
  replaceSelectedTextWithEllipsis,
  setZOrder,
  toggleFillBlackWhite,
  toggleSubscript,
  toggleSuperscript,
  ungroupSelectedShape,
} from "../office/powerpoint";
import {
  ColorCommandKind,
  nextPaletteColor,
  normalizeHex,
  recordRecentColor,
} from "../office/formatColorState";
import { isUnsupportedWebCommand, runUnsupportedWebCommand } from "./unsupportedWebCommands";

export type CommandOutcomeKind = "success" | "unsupported" | "error";

export interface CommandOutcome {
  kind: CommandOutcomeKind;
  message: string;
}

export function outcomeSuccess(message: string): CommandOutcome {
  return { kind: "success", message };
}

export function outcomeError(message: string): CommandOutcome {
  return { kind: "error", message };
}

export interface SettingsCommandActions {
  openShortcutManager: () => CommandOutcome | Promise<CommandOutcome>;
  resetToDefaults: () => CommandOutcome | Promise<CommandOutcome>;
  openColorScheme: () => CommandOutcome | Promise<CommandOutcome>;
}

/**
 * Executes a catalog command. ServerLayout commands round-trip geometry through
 * the backend layout engine; HostScript commands run directly against Office.js.
 */
export async function runCommand(
  descriptor: CommandDescriptor,
  settingsActions?: SettingsCommandActions,
): Promise<CommandOutcome> {
  try {
    switch (descriptor.execution) {
      case "ServerLayout":
        return await runServerLayout(descriptor);
      case "HostScript":
        return await runHostScript(descriptor);
      case "Settings":
        return await runSettingsCommand(descriptor, settingsActions);
      default:
        return outcomeError(`Unknown execution kind for ${descriptor.id}.`);
    }
  } catch (err) {
    return outcomeError(err instanceof Error ? err.message : String(err));
  }
}

async function runSettingsCommand(
  descriptor: CommandDescriptor,
  actions?: SettingsCommandActions,
): Promise<CommandOutcome> {
  if (!actions) {
    return outcomeError("Settings UI is not ready.");
  }

  switch (descriptor.id) {
    case "OpenShortcutManager":
      return await actions.openShortcutManager();
    case "ResetToDefaults":
      return await actions.resetToDefaults();
    case "OpenColorScheme":
      return await actions.openColorScheme();
    default:
      return outcomeError(`Unknown settings command: ${descriptor.id}.`);
  }
}

async function runServerLayout(descriptor: CommandDescriptor): Promise<CommandOutcome> {
  const shapes = await getSelectedShapeBounds();
  if (shapes.length === 0) {
    return outcomeError("Select one or more shapes first.");
  }

  const result = await api.applyLayout(descriptor.id, shapes);
  if (!result.changed) {
    return outcomeSuccess(result.message ?? "Nothing to change.");
  }

  await applyShapeBounds(result.shapes);
  return outcomeSuccess(`${descriptor.title} applied to ${result.shapes.length} shape(s).`);
}

async function runPaletteColorCommand(kind: ColorCommandKind): Promise<CommandOutcome> {
  const shapeIds = await getSelectedShapeIds();
  if (shapeIds.length === 0) {
    return outcomeError("Select one or more shapes first.");
  }

  const color = nextPaletteColor(kind, shapeIds);
  const normalized = normalizeHex(color);

  let count: number;
  switch (kind) {
    case "FillColor":
      count = await applyFillColor(normalized);
      break;
    case "LineColor":
      count = await applyLineColor(normalized);
      break;
    case "TextColor":
      count = await applyTextColor(normalized);
      break;
  }

  recordRecentColor(normalized);
  return outcomeSuccess(
    `${kind === "FillColor" ? "Fill" : kind === "LineColor" ? "Line" : "Text"} color ${normalized} applied to ${count} shape(s).`,
  );
}

const COPY_AND_ALIGN_LAYOUT: Record<
  "CopyAndAlignLeft" | "CopyAndAlignRight" | "CopyAndAlignTop" | "CopyAndAlignBottom",
  string
> = {
  CopyAndAlignLeft: "AlignLeft",
  CopyAndAlignRight: "AlignRight",
  CopyAndAlignTop: "AlignTop",
  CopyAndAlignBottom: "AlignBottom",
};

async function runCopyAndAlign(
  commandId: keyof typeof COPY_AND_ALIGN_LAYOUT,
): Promise<CommandOutcome> {
  const originals = await getSelectedShapeBounds();
  if (originals.length === 0) {
    return outcomeError("Select one or more shapes first.");
  }

  const clones = await cloneSelectedShapesAtSourcePositions();
  const combined = [...originals, ...clones];
  const anchorIndex = originals.length - 1;
  const layoutCommand = COPY_AND_ALIGN_LAYOUT[commandId];

  const result = await api.applyLayout(layoutCommand, combined, anchorIndex);
  if (!result.changed) {
    return outcomeSuccess(result.message ?? "Nothing to change.");
  }

  await applyShapeBoundsOnSlide(result.shapes);
  return outcomeSuccess(`Duplicated and aligned ${clones.length} shape(s).`);
}

async function runHostScript(descriptor: CommandDescriptor): Promise<CommandOutcome> {
  if (isUnsupportedWebCommand(descriptor.id)) {
    return runUnsupportedWebCommand(descriptor.id);
  }

  switch (descriptor.id) {
    case "InsertRectangle":
    case "InsertSquare":
      await insertShape("rectangle");
      return outcomeSuccess("Rectangle inserted.");
    case "InsertEllipse":
      await insertShape("ellipse");
      return outcomeSuccess("Ellipse inserted.");
    case "InsertLine":
    case "InsertArrow":
      await insertShape("line");
      return outcomeSuccess("Line inserted.");
    case "InsertTextbox":
      await insertTextBox();
      return outcomeSuccess("Text box inserted.");
    case "Group": {
      const count = await groupSelectedShapes();
      return outcomeSuccess(`Grouped ${count} shape(s).`);
    }
    case "Ungroup":
      await ungroupSelectedShape();
      return outcomeSuccess("Ungrouped.");
    case "DuplicateRight":
    case "DuplicateLeft":
    case "DuplicateDown":
    case "DuplicateUp": {
      const shapes = await getSelectedShapeBounds();
      if (shapes.length === 0) {
        return outcomeError("Select one or more shapes first.");
      }
      const targets = await Promise.all(
        shapes.map((source) => api.duplicateOffset(descriptor.id, source, 0)),
      );
      const count = await duplicateShapesAtPositions(shapes, targets);
      return outcomeSuccess(`Duplicated ${count} shape(s).`);
    }
    case "CopyObjectPosition": {
      const position = await copyObjectPosition();
      return outcomeSuccess(
        `Copied position (${position.left.toFixed(1)}, ${position.top.toFixed(1)}).`,
      );
    }
    case "PasteObjectPosition": {
      const position = getPositionClipboard();
      if (!position) {
        return outcomeError("Copy a position first (Copy object position).");
      }
      const count = await pasteObjectPosition(position);
      return outcomeSuccess(`Pasted position to ${count} shape(s).`);
    }
    case "BringToFront":
      await setZOrder("front");
      return outcomeSuccess("Brought to front.");
    case "SendToBack":
      await setZOrder("back");
      return outcomeSuccess("Sent to back.");
    case "BringForward":
      await setZOrder("forward");
      return outcomeSuccess("Brought forward.");
    case "SendBackward":
      await setZOrder("backward");
      return outcomeSuccess("Sent backward.");
    case "AddupTextFields": {
      const texts = await getSelectedShapeTexts();
      const stats = await api.addup(texts);
      return outcomeSuccess(
        `Sum ${stats.sum} · avg ${stats.average} · min ${stats.min} · max ${stats.max} (${stats.count} numbers).`,
      );
    }
    case "ToggleFillBlackWhite": {
      const count = await toggleFillBlackWhite();
      return outcomeSuccess(`Toggled fill on ${count} shape(s).`);
    }
    case "FillColor":
    case "LineColor":
    case "TextColor": {
      return await runPaletteColorCommand(descriptor.id as ColorCommandKind);
    }
    case "PasteUnformatted": {
      const count = await pasteUnformattedText();
      return outcomeSuccess(`Pasted plain text into ${count} shape(s).`);
    }
    case "ReplaceWithEllipsis": {
      const count = await replaceSelectedTextWithEllipsis();
      return outcomeSuccess(`Replaced text with "..." on ${count} shape(s).`);
    }
    case "ToggleSuperscript": {
      const count = await toggleSuperscript();
      return outcomeSuccess(`Toggled superscript on ${count} shape(s).`);
    }
    case "ToggleSubscript": {
      const count = await toggleSubscript();
      return outcomeSuccess(`Toggled subscript on ${count} shape(s).`);
    }
    case "CopyAndAlignLeft":
    case "CopyAndAlignRight":
    case "CopyAndAlignTop":
    case "CopyAndAlignBottom":
      return await runCopyAndAlign(descriptor.id as keyof typeof COPY_AND_ALIGN_LAYOUT);
    case "CopySlide":
      await duplicateSelectedSlide();
      return outcomeSuccess("Slide duplicated.");
    default:
      // Safety-net for unknown ids — should never fire for catalog commands after S02-005.
      return outcomeError(
        `'${descriptor.title}' is not wired up yet (Office.js support: ${descriptor.support}).`,
      );
  }
}

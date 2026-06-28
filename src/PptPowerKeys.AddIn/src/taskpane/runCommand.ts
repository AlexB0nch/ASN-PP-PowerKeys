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

export interface CommandOutcome {
  ok: boolean;
  message: string;
}

/**
 * Executes a catalog command. ServerLayout commands round-trip geometry through
 * the backend layout engine; HostScript commands run directly against Office.js.
 */
export async function runCommand(descriptor: CommandDescriptor): Promise<CommandOutcome> {
  try {
    switch (descriptor.execution) {
      case "ServerLayout":
        return await runServerLayout(descriptor);
      case "HostScript":
        return await runHostScript(descriptor);
      case "Settings":
        return { ok: true, message: "Open the settings panel below." };
      default:
        return { ok: false, message: `Unknown execution kind for ${descriptor.id}.` };
    }
  } catch (err) {
    return { ok: false, message: err instanceof Error ? err.message : String(err) };
  }
}

async function runServerLayout(descriptor: CommandDescriptor): Promise<CommandOutcome> {
  const shapes = await getSelectedShapeBounds();
  if (shapes.length === 0) {
    return { ok: false, message: "Select one or more shapes first." };
  }

  const result = await api.applyLayout(descriptor.id, shapes);
  if (!result.changed) {
    return { ok: true, message: result.message ?? "Nothing to change." };
  }

  await applyShapeBounds(result.shapes);
  return { ok: true, message: `${descriptor.title} applied to ${result.shapes.length} shape(s).` };
}

async function runPaletteColorCommand(kind: ColorCommandKind): Promise<CommandOutcome> {
  const shapeIds = await getSelectedShapeIds();
  if (shapeIds.length === 0) {
    return { ok: false, message: "Select one or more shapes first." };
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
  return {
    ok: true,
    message: `${kind === "FillColor" ? "Fill" : kind === "LineColor" ? "Line" : "Text"} color ${normalized} applied to ${count} shape(s).`,
  };
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
    return { ok: false, message: "Select one or more shapes first." };
  }

  const clones = await cloneSelectedShapesAtSourcePositions();
  const combined = [...originals, ...clones];
  const anchorIndex = originals.length - 1;
  const layoutCommand = COPY_AND_ALIGN_LAYOUT[commandId];

  const result = await api.applyLayout(layoutCommand, combined, anchorIndex);
  if (!result.changed) {
    return { ok: true, message: result.message ?? "Nothing to change." };
  }

  await applyShapeBoundsOnSlide(result.shapes);
  return {
    ok: true,
    message: `Duplicated and aligned ${clones.length} shape(s).`,
  };
}

async function runHostScript(descriptor: CommandDescriptor): Promise<CommandOutcome> {
  switch (descriptor.id) {
    case "InsertRectangle":
    case "InsertSquare":
      await insertShape("rectangle");
      return { ok: true, message: "Rectangle inserted." };
    case "InsertEllipse":
      await insertShape("ellipse");
      return { ok: true, message: "Ellipse inserted." };
    case "InsertLine":
    case "InsertArrow":
      await insertShape("line");
      return { ok: true, message: "Line inserted." };
    case "InsertTextbox":
      await insertTextBox();
      return { ok: true, message: "Text box inserted." };
    case "Group": {
      const count = await groupSelectedShapes();
      return { ok: true, message: `Grouped ${count} shape(s).` };
    }
    case "Ungroup":
      await ungroupSelectedShape();
      return { ok: true, message: "Ungrouped." };
    case "Regroup":
      return {
        ok: false,
        message: "Regroup is not supported on PowerPoint Web.",
      };
    case "DuplicateRight":
    case "DuplicateLeft":
    case "DuplicateDown":
    case "DuplicateUp": {
      const shapes = await getSelectedShapeBounds();
      if (shapes.length === 0) {
        return { ok: false, message: "Select one or more shapes first." };
      }
      const targets = await Promise.all(
        shapes.map((source) => api.duplicateOffset(descriptor.id, source, 0)),
      );
      const count = await duplicateShapesAtPositions(shapes, targets);
      return { ok: true, message: `Duplicated ${count} shape(s).` };
    }
    case "CopyObjectPosition": {
      const position = await copyObjectPosition();
      return {
        ok: true,
        message: `Copied position (${position.left.toFixed(1)}, ${position.top.toFixed(1)}).`,
      };
    }
    case "PasteObjectPosition": {
      const position = getPositionClipboard();
      if (!position) {
        return { ok: false, message: "Copy a position first (Copy object position)." };
      }
      const count = await pasteObjectPosition(position);
      return { ok: true, message: `Pasted position to ${count} shape(s).` };
    }
    case "BringToFront":
      await setZOrder("front");
      return { ok: true, message: "Brought to front." };
    case "SendToBack":
      await setZOrder("back");
      return { ok: true, message: "Sent to back." };
    case "BringForward":
      await setZOrder("forward");
      return { ok: true, message: "Brought forward." };
    case "SendBackward":
      await setZOrder("backward");
      return { ok: true, message: "Sent backward." };
    case "AddupTextFields": {
      const texts = await getSelectedShapeTexts();
      const stats = await api.addup(texts);
      return {
        ok: true,
        message: `Sum ${stats.sum} · avg ${stats.average} · min ${stats.min} · max ${stats.max} (${stats.count} numbers).`,
      };
    }
    case "ToggleFillBlackWhite": {
      const count = await toggleFillBlackWhite();
      return { ok: true, message: `Toggled fill on ${count} shape(s).` };
    }
    case "FillColor":
    case "LineColor":
    case "TextColor": {
      return await runPaletteColorCommand(descriptor.id as ColorCommandKind);
    }
    case "FormatPainter":
      return {
        ok: false,
        message: "Format painter is not supported on PowerPoint Web.",
      };
    case "PasteUnformatted": {
      const count = await pasteUnformattedText();
      return { ok: true, message: `Pasted plain text into ${count} shape(s).` };
    }
    case "PasteFormatted":
      return {
        ok: false,
        message: "Paste formatted is not supported on PowerPoint Web.",
      };
    case "ReplaceWithEllipsis": {
      const count = await replaceSelectedTextWithEllipsis();
      return { ok: true, message: `Replaced text with "..." on ${count} shape(s).` };
    }
    case "ToggleSuperscript": {
      const count = await toggleSuperscript();
      return { ok: true, message: `Toggled superscript on ${count} shape(s).` };
    }
    case "ToggleSubscript": {
      const count = await toggleSubscript();
      return { ok: true, message: `Toggled subscript on ${count} shape(s).` };
    }
    case "CopyAndAlignLeft":
    case "CopyAndAlignRight":
    case "CopyAndAlignTop":
    case "CopyAndAlignBottom":
      return await runCopyAndAlign(descriptor.id as keyof typeof COPY_AND_ALIGN_LAYOUT);
    default:
      return {
        ok: false,
        message: `'${descriptor.title}' is not wired up yet (Office.js support: ${descriptor.support}).`,
      };
  }
}

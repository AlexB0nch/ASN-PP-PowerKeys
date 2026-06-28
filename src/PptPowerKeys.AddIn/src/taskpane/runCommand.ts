import { api } from "../services/api";
import { CommandDescriptor } from "../services/types";
import {
  applyFillColor,
  applyLineColor,
  applyShapeBounds,
  applyTextColor,
  getSelectedShapeBounds,
  getSelectedShapeIds,
  getSelectedShapeTexts,
  insertShape,
  setZOrder,
  toggleFillBlackWhite,
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
    default:
      return {
        ok: false,
        message: `'${descriptor.title}' is not wired up yet (Office.js support: ${descriptor.support}).`,
      };
  }
}

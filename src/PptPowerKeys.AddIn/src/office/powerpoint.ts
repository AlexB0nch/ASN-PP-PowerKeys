import { ShapeBounds } from "../services/types";
import { getPositionClipboard, PositionSnapshot, setPositionClipboard } from "./positionClipboard";

/* global PowerPoint, Office */

type CloneCapableShape = PowerPoint.Shape & {
  copyTo?: () => PowerPoint.Shape;
  duplicate?: () => PowerPoint.Shape;
};

/** Clones a shape on the same slide using native APIs when available, otherwise recreates basic types. */
async function cloneShapeOnSlide(
  context: PowerPoint.RequestContext,
  source: PowerPoint.Shape,
  slide: PowerPoint.Slide,
): Promise<PowerPoint.Shape> {
  const cloneApi = source as CloneCapableShape;

  if (typeof cloneApi.copyTo === "function") {
    const copy = cloneApi.copyTo();
    copy.load("id");
    await context.sync();
    return copy;
  }

  if (typeof cloneApi.duplicate === "function") {
    const copy = cloneApi.duplicate();
    copy.load("id");
    await context.sync();
    return copy;
  }

  source.load("type,left,top,width,height");
  await context.sync();

  const shapeType = source.type;

  if (shapeType === PowerPoint.ShapeType.textBox) {
    source.textFrame.textRange.load("text");
    await context.sync();
    const text = source.textFrame.textRange.text ?? "";
    const copy = slide.shapes.addTextBox(text);
    copy.width = source.width;
    copy.height = source.height;
    copy.left = source.left;
    copy.top = source.top;
    copy.load("id");
    await context.sync();
    return copy;
  }

  if (shapeType === PowerPoint.ShapeType.line) {
    const copy = slide.shapes.addLine();
    copy.left = source.left;
    copy.top = source.top;
    copy.width = source.width;
    copy.height = source.height;
    copy.load("id");
    await context.sync();
    return copy;
  }

  if (shapeType === PowerPoint.ShapeType.geometricShape) {
    const copy = slide.shapes.addGeometricShape(PowerPoint.GeometricShapeType.rectangle);
    copy.left = source.left;
    copy.top = source.top;
    copy.width = source.width;
    copy.height = source.height;
    copy.load("id");
    await context.sync();
    return copy;
  }

  throw new Error(
    "Shape duplication is not supported for this shape type on PowerPoint Web. Try desktop PowerPoint or a simpler shape.",
  );
}

/**
 * Reads the geometry of the currently selected shapes, in selection order.
 * The last shape is treated as the alignment anchor by the layout engine.
 */
export async function getSelectedShapeBounds(): Promise<ShapeBounds[]> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id,items/left,items/top,items/width,items/height");
    await context.sync();

    return selected.items.map((s) => ({
      id: s.id,
      left: s.left,
      top: s.top,
      width: s.width,
      height: s.height,
    }));
  });
}

/**
 * Applies computed geometry back onto the live shapes, matching by id. Only
 * shapes present in {@link bounds} are touched.
 */
export async function applyShapeBounds(bounds: ShapeBounds[]): Promise<void> {
  const byId = new Map(bounds.map((b) => [b.id, b]));

  await PowerPoint.run(async (context) => {
    const shapes = context.presentation.getSelectedShapes();
    shapes.load("items/id");
    await context.sync();

    for (const shape of shapes.items) {
      const target = byId.get(shape.id);
      if (!target) {
        continue;
      }
      shape.left = target.left;
      shape.top = target.top;
      shape.width = target.width;
      shape.height = target.height;
    }

    await context.sync();
  });
}

/** Reads the text of every selected shape (empty string where there is none). */
export async function getSelectedShapeTexts(): Promise<string[]> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/textFrame/textRange/text");
    await context.sync();

    return selected.items.map((s) => {
      try {
        return s.textFrame.textRange.text ?? "";
      } catch {
        return "";
      }
    });
  });
}

export type GeometricKind = "rectangle" | "ellipse" | "line";

/** Inserts a basic shape near the top-left of the current slide. */
export async function insertShape(kind: GeometricKind): Promise<void> {
  await PowerPoint.run(async (context) => {
    const slide = context.presentation.getSelectedSlides().getItemAt(0);
    const shapes = slide.shapes;

    if (kind === "line") {
      shapes.addLine();
    } else {
      const type =
        kind === "rectangle"
          ? PowerPoint.GeometricShapeType.rectangle
          : PowerPoint.GeometricShapeType.ellipse;
      const shape = shapes.addGeometricShape(type);
      shape.left = 100;
      shape.top = 100;
      shape.width = 150;
      shape.height = 100;
    }

    await context.sync();
  });
}

export type ZOrder = "front" | "back" | "forward" | "backward";

const BLACK = "#000000";
const WHITE = "#FFFFFF";

/** Returns selected shape ids in selection order. */
export async function getSelectedShapeIds(): Promise<string[]> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();
    return selected.items.map((s) => s.id);
  });
}

function parseRgb(color: string): { r: number; g: number; b: number } | null {
  const normalized = color.trim().startsWith("#") ? color.trim() : `#${color.trim()}`;
  if (normalized.length === 4) {
    const r = parseInt(normalized[1] + normalized[1], 16);
    const g = parseInt(normalized[2] + normalized[2], 16);
    const b = parseInt(normalized[3] + normalized[3], 16);
    return { r, g, b };
  }
  if (normalized.length === 7) {
    const r = parseInt(normalized.slice(1, 3), 16);
    const g = parseInt(normalized.slice(3, 5), 16);
    const b = parseInt(normalized.slice(5, 7), 16);
    if (Number.isNaN(r) || Number.isNaN(g) || Number.isNaN(b)) {
      return null;
    }
    return { r, g, b };
  }
  return null;
}

function isNearBlack(color: string): boolean {
  const rgb = parseRgb(color);
  if (!rgb) {
    return false;
  }
  return rgb.r < 48 && rgb.g < 48 && rgb.b < 48;
}

function requireSelection(ids: string[]): void {
  if (ids.length === 0) {
    throw new Error("Select one or more shapes first.");
  }
}

/** Toggles fill between black and white for each selected shape. */
export async function toggleFillBlackWhite(): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id,items/fill/foregroundColor");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    for (const shape of selected.items) {
      const current = shape.fill.foregroundColor ?? "";
      const next = isNearBlack(current) ? WHITE : BLACK;
      shape.fill.setSolidColor(next);
    }

    await context.sync();
    return selected.items.length;
  });
}

/** Applies a solid fill color to all selected shapes. */
export async function applyFillColor(hex: string): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();
    requireSelection(selected.items.map((s) => s.id));

    for (const shape of selected.items) {
      shape.fill.setSolidColor(hex);
    }

    await context.sync();
    return selected.items.length;
  });
}

/** Applies a line color to all selected shapes. */
export async function applyLineColor(hex: string): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();
    requireSelection(selected.items.map((s) => s.id));

    for (const shape of selected.items) {
      shape.lineFormat.color = hex;
      shape.lineFormat.visible = true;
    }

    await context.sync();
    return selected.items.length;
  });
}

/** Applies a font color to selected shapes that have a text frame. */
export async function applyTextColor(hex: string): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();
    requireSelection(selected.items.map((s) => s.id));

    let applied = 0;
    for (const shape of selected.items) {
      try {
        shape.textFrame.textRange.font.color = hex;
        applied += 1;
      } catch {
        // Shape has no text frame — skip.
      }
    }

    if (applied === 0) {
      throw new Error("Selected shape(s) have no text to color.");
    }

    await context.sync();
    return applied;
  });
}

/** Changes the z-order of the selected shapes. */
export async function setZOrder(order: ZOrder): Promise<void> {
  const action: PowerPoint.ShapeZOrder =
    order === "front"
      ? PowerPoint.ShapeZOrder.bringToFront
      : order === "back"
        ? PowerPoint.ShapeZOrder.sendToBack
        : order === "forward"
          ? PowerPoint.ShapeZOrder.bringForward
          : PowerPoint.ShapeZOrder.sendBackward;

  await PowerPoint.run(async (context) => {
    const shapes = context.presentation.getSelectedShapes();
    shapes.load("items");
    await context.sync();
    for (const shape of shapes.items) {
      shape.setZOrder(action);
    }
    await context.sync();
  });
}

/** Inserts an empty text box near the top-left of the current slide. */
export async function insertTextBox(): Promise<void> {
  await PowerPoint.run(async (context) => {
    const slide = context.presentation.getSelectedSlides().getItemAt(0);
    const box = slide.shapes.addTextBox("");
    box.left = 100;
    box.top = 100;
    box.width = 200;
    box.height = 80;
    await context.sync();
  });
}

/** Groups the currently selected shapes (requires at least two). */
export async function groupSelectedShapes(): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();

    if (selected.items.length < 2) {
      throw new Error("Select at least two shapes to group.");
    }

    const slide = context.presentation.getSelectedSlides().getItemAt(0);
    slide.shapes.addGroup(selected.items);
    await context.sync();
    return selected.items.length;
  });
}

/** Ungroups the selected group shape (requires exactly one group). */
export async function ungroupSelectedShape(): Promise<void> {
  await PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id,items/type");
    await context.sync();

    if (selected.items.length !== 1) {
      throw new Error("Select exactly one group to ungroup.");
    }

    const shape = selected.items[0];
    if (shape.type !== PowerPoint.ShapeType.group) {
      throw new Error("Selected shape is not a group.");
    }

    shape.group.ungroup();
    await context.sync();
  });
}

/**
 * Clones each source shape and moves the copy to the target left/top from the layout API.
 * Width and height of the clone are preserved from the duplication source.
 */
export async function duplicateShapesAtPositions(
  sources: ShapeBounds[],
  targets: ShapeBounds[],
): Promise<number> {
  if (sources.length === 0) {
    throw new Error("Select one or more shapes first.");
  }
  if (sources.length !== targets.length) {
    throw new Error("Source and target geometry count mismatch.");
  }

  let duplicated = 0;

  await PowerPoint.run(async (context) => {
    const slide = context.presentation.getSelectedSlides().getItemAt(0);
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();

    const byId = new Map(selected.items.map((shape) => [shape.id, shape]));

    for (let i = 0; i < sources.length; i++) {
      const sourceShape = byId.get(sources[i].id);
      if (!sourceShape) {
        continue;
      }

      const copy = await cloneShapeOnSlide(context, sourceShape, slide);
      copy.left = targets[i].left;
      copy.top = targets[i].top;
      duplicated++;
    }

    await context.sync();
  });

  if (duplicated === 0) {
    throw new Error("Could not duplicate the selected shapes.");
  }

  return duplicated;
}

/**
 * Clones each selected shape on the current slide at the source position (offset 0).
 * Returns bounds for the new clones in selection order.
 */
export async function cloneSelectedShapesAtSourcePositions(): Promise<ShapeBounds[]> {
  const clones: ShapeBounds[] = [];

  await PowerPoint.run(async (context) => {
    const slide = context.presentation.getSelectedSlides().getItemAt(0);
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id,items/left,items/top,items/width,items/height");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    for (const sourceShape of selected.items) {
      const copy = await cloneShapeOnSlide(context, sourceShape, slide);
      copy.left = sourceShape.left;
      copy.top = sourceShape.top;
      copy.load("id,left,top,width,height");
      await context.sync();

      clones.push({
        id: copy.id,
        left: copy.left,
        top: copy.top,
        width: copy.width,
        height: copy.height,
      });
    }
  });

  if (clones.length === 0) {
    throw new Error("Could not duplicate the selected shapes.");
  }

  return clones;
}

/**
 * Applies computed geometry to shapes on the current slide by id (not limited to selection).
 */
export async function applyShapeBoundsOnSlide(bounds: ShapeBounds[]): Promise<void> {
  await PowerPoint.run(async (context) => {
    const slide = context.presentation.getSelectedSlides().getItemAt(0);

    for (const target of bounds) {
      const shape = slide.shapes.getItemOrNullObject(target.id);
      shape.left = target.left;
      shape.top = target.top;
      shape.width = target.width;
      shape.height = target.height;
    }

    await context.sync();
  });
}

/** Saves the anchor (last selected) shape position to the in-memory clipboard. */
export async function copyObjectPosition(): Promise<PositionSnapshot> {
  const shapes = await getSelectedShapeBounds();
  if (shapes.length === 0) {
    throw new Error("Select a shape first.");
  }

  const anchor = shapes[shapes.length - 1];
  const snapshot = { left: anchor.left, top: anchor.top };
  setPositionClipboard(snapshot);
  return snapshot;
}

/** Applies the clipboard position (left/top only) to all selected shapes. */
export async function pasteObjectPosition(position: PositionSnapshot): Promise<number> {
  let count = 0;

  await PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    count = selected.items.length;
    for (const shape of selected.items) {
      shape.left = position.left;
      shape.top = position.top;
    }

    await context.sync();
  });

  return count;
}

/** Re-export for callers that need to read clipboard state without mutating selection. */
export { getPositionClipboard };

const ELLIPSIS = "...";

async function readClipboardPlainText(): Promise<string> {
  if (!navigator.clipboard?.readText) {
    throw new Error("Clipboard API is not available in this browser.");
  }

  try {
    const text = await navigator.clipboard.readText();
    if (!text) {
      throw new Error("Clipboard is empty.");
    }
    return text;
  } catch (err) {
    if (err instanceof Error && err.message === "Clipboard is empty.") {
      throw err;
    }
    if (err instanceof DOMException && err.name === "NotAllowedError") {
      throw new Error("Clipboard access was denied. Allow clipboard permissions and try again.");
    }
    throw err instanceof Error ? err : new Error(String(err));
  }
}

/** Applies plain text from the system clipboard to every selected shape with a text frame. */
export async function pasteUnformattedText(): Promise<number> {
  const text = await readClipboardPlainText();

  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    let applied = 0;
    for (const shape of selected.items) {
      try {
        shape.textFrame.textRange.text = text;
        applied += 1;
      } catch {
        // Shape has no text frame — skip.
      }
    }

    if (applied === 0) {
      throw new Error("Selected shape(s) have no text frame to paste into.");
    }

    await context.sync();
    return applied;
  });
}

/** Replaces the text of each selected shape with three dots (`"..."`). */
export async function replaceSelectedTextWithEllipsis(): Promise<number> {
  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    let applied = 0;
    for (const shape of selected.items) {
      try {
        shape.textFrame.textRange.text = ELLIPSIS;
        applied += 1;
      } catch {
        // Shape has no text frame — skip.
      }
    }

    if (applied === 0) {
      throw new Error("Selected shape(s) have no text to replace.");
    }

    await context.sync();
    return applied;
  });
}

type ScriptAttribute = "superscript" | "subscript";

async function toggleScriptAttribute(attribute: ScriptAttribute): Promise<number> {
  const label = attribute === "superscript" ? "Superscript" : "Subscript";

  return PowerPoint.run(async (context) => {
    const selected = context.presentation.getSelectedShapes();
    selected.load("items/id");
    await context.sync();

    if (selected.items.length === 0) {
      throw new Error("Select one or more shapes first.");
    }

    const fonts: PowerPoint.ShapeFont[] = [];
    for (const shape of selected.items) {
      try {
        const font = shape.textFrame.textRange.font;
        font.load("superscript,subscript");
        fonts.push(font);
      } catch {
        // Shape has no text frame — skip.
      }
    }

    if (fonts.length === 0) {
      throw new Error("Selected shape(s) have no text to format.");
    }

    try {
      await context.sync();
    } catch {
      throw new Error(`${label} is not supported on this PowerPoint version.`);
    }

    for (const font of fonts) {
      const current = attribute === "superscript" ? font.superscript : font.subscript;
      const enable = current !== true;
      if (attribute === "superscript") {
        font.superscript = enable;
        if (enable) {
          font.subscript = false;
        }
      } else {
        font.subscript = enable;
        if (enable) {
          font.superscript = false;
        }
      }
    }

    try {
      await context.sync();
    } catch {
      throw new Error(`${label} is not supported on this PowerPoint version.`);
    }

    return fonts.length;
  });
}

/** Toggles superscript on selected shapes; enabling superscript disables subscript. */
export async function toggleSuperscript(): Promise<number> {
  return toggleScriptAttribute("superscript");
}

/** Toggles subscript on selected shapes; enabling subscript disables superscript. */
export async function toggleSubscript(): Promise<number> {
  return toggleScriptAttribute("subscript");
}

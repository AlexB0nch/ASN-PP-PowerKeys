import { ShapeBounds } from "../services/types";

/* global PowerPoint, Office */

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

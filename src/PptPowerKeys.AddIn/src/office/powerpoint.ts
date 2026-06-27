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

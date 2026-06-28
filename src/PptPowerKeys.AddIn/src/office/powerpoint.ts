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

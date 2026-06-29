#!/usr/bin/env node
/**
 * Generates shortcuts.json for Office Keyboard Shortcuts API (Tier 1 defaults).
 * Action id === CommandId; keys mirror CommandCatalog.DefaultShortcut + McKinsey extras.
 *
 * Output: dist/shortcuts.json (and shortcuts.json at add-in root for webpack copy in dev).
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.dirname(fileURLToPath(import.meta.url));
const addInDir = path.resolve(root, "..");

/** Tier 1: catalog DefaultShortcut entries + McKinsey preset keys (non-Settings). */
const TIER1 = [
  { id: "AlignLeft", name: "Align left", keys: "Alt+1" },
  { id: "AlignCenterHorizontal", name: "Align center (H)", keys: "Alt+2" },
  { id: "AlignRight", name: "Align right", keys: "Alt+3" },
  { id: "AlignTop", name: "Align top", keys: "Alt+4" },
  { id: "AlignMiddleVertical", name: "Align middle (V)", keys: "Alt+5" },
  { id: "AlignBottom", name: "Align bottom", keys: "Alt+6" },
  { id: "DistributeHorizontal", name: "Distribute horizontally", keys: "Alt+7" },
  { id: "DistributeVertical", name: "Distribute vertically", keys: "Alt+8" },
  { id: "SameWidth", name: "Same width", keys: "Alt+B" },
  { id: "SameHeight", name: "Same height", keys: "Alt+H" },
  { id: "FillColor", name: "Fill color", keys: "Alt+G" },
  { id: "ToggleZoom", name: "Toggle zoom fit", keys: "F1" },
  { id: "DuplicateRight", name: "Duplicate right", keys: "Alt+D" },
  { id: "AddupTextFields", name: "Sum numeric fields", keys: "Alt+A" },
];

const shortcutsJson = {
  actions: TIER1.map(({ id, name }) => ({
    id,
    type: "ExecuteFunction",
    name,
  })),
  shortcuts: TIER1.map(({ id, keys }) => ({
    action: id,
    key: { default: keys },
  })),
};

const serialized = `${JSON.stringify(shortcutsJson, null, 2)}\n`;

const distDir = path.join(addInDir, "dist");
fs.mkdirSync(distDir, { recursive: true });

const distPath = path.join(distDir, "shortcuts.json");
const rootPath = path.join(addInDir, "shortcuts.json");

fs.writeFileSync(distPath, serialized, "utf8");
fs.writeFileSync(rootPath, serialized, "utf8");

console.log(`Wrote ${distPath} (${TIER1.length} Tier 1 shortcuts)`);

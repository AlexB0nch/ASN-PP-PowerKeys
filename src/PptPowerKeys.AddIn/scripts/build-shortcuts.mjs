#!/usr/bin/env node
/**
 * Generates shortcuts.json for Office Keyboard Shortcuts API.
 * actions[] — all 76 hotkey-eligible commands (id === CommandId).
 * shortcuts[] — Tier 1 default keys only (14); user keys via replaceShortcuts (S06-002).
 *
 * Output: dist/shortcuts.json (and shortcuts.json at add-in root for webpack copy in dev).
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.dirname(fileURLToPath(import.meta.url));
const addInDir = path.resolve(root, "..");
const catalogPath = path.resolve(addInDir, "../PptPowerKeys.Core/Commands/CommandCatalog.cs");

/** Tier 1: catalog DefaultShortcut entries + McKinsey preset keys (non-Settings). */
const TIER1_DEFAULTS = [
  { id: "AlignLeft", keys: "Alt+1" },
  { id: "AlignCenterHorizontal", keys: "Alt+2" },
  { id: "AlignRight", keys: "Alt+3" },
  { id: "AlignTop", keys: "Alt+4" },
  { id: "AlignMiddleVertical", keys: "Alt+5" },
  { id: "AlignBottom", keys: "Alt+6" },
  { id: "DistributeHorizontal", keys: "Alt+7" },
  { id: "DistributeVertical", keys: "Alt+8" },
  { id: "SameWidth", keys: "Alt+B" },
  { id: "SameHeight", keys: "Alt+H" },
  { id: "FillColor", keys: "Alt+G" },
  { id: "ToggleZoom", keys: "F1" },
  { id: "DuplicateRight", keys: "Alt+D" },
  { id: "AddupTextFields", keys: "Alt+A" },
];

function loadHotkeyEligibleCommands() {
  const text = fs.readFileSync(catalogPath, "utf8");
  const commands = [];
  const re = /(?:Layout|Host)\(CommandIds\.(\w+),\s*"([^"]+)"/g;
  let match;
  while ((match = re.exec(text)) !== null) {
    commands.push({ id: match[1], name: match[2] });
  }
  if (commands.length !== 76) {
    throw new Error(`Expected 76 hotkey-eligible commands, found ${commands.length}`);
  }
  return commands;
}

const actions = loadHotkeyEligibleCommands();

const shortcutsJson = {
  actions: actions.map(({ id, name }) => ({
    id,
    type: "ExecuteFunction",
    name,
  })),
  shortcuts: TIER1_DEFAULTS.map(({ id, keys }) => ({
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

console.log(
  `Wrote ${distPath} (${actions.length} actions, ${TIER1_DEFAULTS.length} Tier 1 default shortcuts)`,
);

/** HostScript color commands that cycle a palette on repeat press. */
export type ColorCommandKind = "FillColor" | "LineColor" | "TextColor";

/** Theme-like defaults when Slide Master palette is unavailable on Web. */
export const DEFAULT_PALETTE: readonly string[] = [
  "#4472C4",
  "#ED7D31",
  "#A5A5A5",
  "#FFC000",
  "#5B9BD5",
  "#70AD47",
  "#264478",
  "#9E480E",
  "#636363",
  "#997300",
];

const MAX_RECENT = 5;

interface CycleState {
  fingerprint: string;
  index: number;
}

const cycleByCommand: Record<ColorCommandKind, CycleState> = {
  FillColor: { fingerprint: "", index: 0 },
  LineColor: { fingerprint: "", index: 0 },
  TextColor: { fingerprint: "", index: 0 },
};

let recentColors: string[] = [];

/** Normalizes a color string to uppercase `#RRGGBB`. */
export function normalizeHex(color: string): string {
  const trimmed = color.trim();
  if (!trimmed) {
    return "#000000";
  }

  const withHash = trimmed.startsWith("#") ? trimmed : `#${trimmed}`;
  if (withHash.length === 4) {
    const r = withHash[1];
    const g = withHash[2];
    const b = withHash[3];
    return `#${r}${r}${g}${g}${b}${b}`.toUpperCase();
  }

  if (withHash.length === 7) {
    return withHash.toUpperCase();
  }

  return withHash.toUpperCase();
}

/** Default palette plus in-memory recent colors (deduplicated, defaults first). */
export function getActivePalette(): string[] {
  const seen = new Set<string>();
  const palette: string[] = [];

  for (const color of DEFAULT_PALETTE) {
    const normalized = normalizeHex(color);
    if (!seen.has(normalized)) {
      seen.add(normalized);
      palette.push(normalized);
    }
  }

  for (const color of recentColors) {
    const normalized = normalizeHex(color);
    if (!seen.has(normalized)) {
      seen.add(normalized);
      palette.push(normalized);
    }
  }

  return palette;
}

/** Builds a stable fingerprint from selected shape ids in order. */
export function selectionFingerprint(shapeIds: string[]): string {
  return shapeIds.join("\u001f");
}

/**
 * Returns the next palette color for a command. Resets the cycle when the
 * selection fingerprint changes; advances on repeat press with the same selection.
 */
export function nextPaletteColor(kind: ColorCommandKind, shapeIds: string[]): string {
  const palette = getActivePalette();
  if (palette.length === 0) {
    return "#4472C4";
  }

  const fingerprint = selectionFingerprint(shapeIds);
  const state = cycleByCommand[kind];

  if (fingerprint !== state.fingerprint) {
    state.fingerprint = fingerprint;
    state.index = 0;
  }

  const color = palette[state.index % palette.length];
  state.index = (state.index + 1) % palette.length;
  return color;
}

/** Records an applied color in the recent list (FIFO, max 5, no duplicates). */
export function recordRecentColor(color: string): void {
  const normalized = normalizeHex(color);
  recentColors = recentColors.filter((c) => normalizeHex(c) !== normalized);
  recentColors.unshift(normalized);
  if (recentColors.length > MAX_RECENT) {
    recentColors = recentColors.slice(0, MAX_RECENT);
  }
}

/** Resets in-memory state (for tests or future settings reset). */
export function resetFormatColorState(): void {
  recentColors = [];
  for (const kind of Object.keys(cycleByCommand) as ColorCommandKind[]) {
    cycleByCommand[kind] = { fingerprint: "", index: 0 };
  }
}

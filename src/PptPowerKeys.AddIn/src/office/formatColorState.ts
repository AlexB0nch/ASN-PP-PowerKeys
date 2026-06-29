import { api } from "../services/api";

/** HostScript color commands that cycle a palette on repeat press. */
export type ColorCommandKind = "FillColor" | "LineColor" | "TextColor";

export type ThemeColorSource = "slideMaster" | "slide" | "fallback";

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

/** localStorage key for recent colors (per device; no Api round-trip). */
export const RECENT_COLORS_STORAGE_KEY = "ppt-powerkeys-recent-colors";

function isLocalStorageAvailable(): boolean {
  try {
    if (typeof localStorage === "undefined") {
      return false;
    }
    const probe = "__ppt_pk_ls_probe__";
    localStorage.setItem(probe, probe);
    localStorage.removeItem(probe);
    return true;
  } catch {
    return false;
  }
}

function persistRecentColors(): void {
  if (!isLocalStorageAvailable()) {
    return;
  }

  try {
    localStorage.setItem(RECENT_COLORS_STORAGE_KEY, JSON.stringify(recentColors));
  } catch {
    // Quota or privacy mode — in-memory recent still works for this session.
  }
}

/** Loads persisted recent colors from localStorage (call on task pane startup). */
export function loadPersistedRecentColors(): void {
  if (!isLocalStorageAvailable()) {
    return;
  }

  try {
    const raw = localStorage.getItem(RECENT_COLORS_STORAGE_KEY);
    if (!raw) {
      return;
    }

    const parsed: unknown = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return;
    }

    recentColors = parsed
      .filter((entry): entry is string => typeof entry === "string")
      .map((entry) => normalizeHex(entry))
      .slice(0, MAX_RECENT);
    activePaletteCache = null;
  } catch {
    // Corrupt payload — ignore and start fresh.
  }
}

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
let themeColors: string[] | null = null;
let themeColorSource: ThemeColorSource = "fallback";
let activePaletteCache: string[] | null = null;
let paletteRefreshPromise: Promise<void> | null = null;

/** Mirrors Core `ThemeColor.IsValidHex` — optional `#` plus six hex digits. */
export function isValidHex(hex: string | null | undefined): boolean {
  if (hex == null) {
    return false;
  }
  return /^#?[0-9a-fA-F]{6}$/.test(hex.trim());
}

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

/** Sets in-memory theme colors from the presentation (null = use API fallback theme). */
export function setThemeColors(colors: string[] | null, source: ThemeColorSource = "fallback"): void {
  themeColors = colors;
  themeColorSource = source;
  activePaletteCache = null;
  void refreshActivePalette();
}

/** Returns how the current theme colors were obtained. */
export function getThemeColorSource(): ThemeColorSource {
  return themeColorSource;
}

/** Rebuilds the cached palette via the API (Core merge). */
export async function refreshActivePalette(): Promise<void> {
  if (paletteRefreshPromise) {
    return paletteRefreshPromise;
  }

  paletteRefreshPromise = (async () => {
    try {
      const response = await api.buildPalette({
        themeColors: themeColors ?? undefined,
        recentColors,
        fallbackTheme: [...DEFAULT_PALETTE],
      });
      activePaletteCache = response.palette;
    } catch {
      activePaletteCache = buildSyncFallbackPalette();
    } finally {
      paletteRefreshPromise = null;
    }
  })();

  return paletteRefreshPromise;
}

function buildSyncFallbackPalette(): string[] {
  const seen = new Set<string>();
  const palette: string[] = [];

  const themeSource = themeColors ?? [...DEFAULT_PALETTE];
  for (const color of themeSource) {
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

/** Theme colors from the presentation (or {@link DEFAULT_PALETTE} when using fallback). */
export function getThemePaletteColors(): string[] {
  const source = themeColors ?? [...DEFAULT_PALETTE];
  const seen = new Set<string>();
  const result: string[] = [];

  for (const color of source) {
    const normalized = normalizeHex(color);
    if (!seen.has(normalized)) {
      seen.add(normalized);
      result.push(normalized);
    }
    if (result.length >= 10) {
      break;
    }
  }

  return result;
}

/** Recently applied colors (newest first, max 5). */
export function getRecentColors(): string[] {
  return recentColors.map((c) => normalizeHex(c));
}

/** Theme + recent colors merged via Core (cached after bootstrap / recent updates). */
export function getActivePalette(): string[] {
  if (activePaletteCache) {
    return activePaletteCache;
  }

  return buildSyncFallbackPalette();
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

  activePaletteCache = null;
  persistRecentColors();
  void refreshActivePalette();
}

/** Resets in-memory state (for tests or future settings reset). */
export function resetFormatColorState(): void {
  recentColors = [];
  themeColors = null;
  themeColorSource = "fallback";
  activePaletteCache = null;
  paletteRefreshPromise = null;
  if (isLocalStorageAvailable()) {
    try {
      localStorage.removeItem(RECENT_COLORS_STORAGE_KEY);
    } catch {
      // ignore
    }
  }
  for (const kind of Object.keys(cycleByCommand) as ColorCommandKind[]) {
    cycleByCommand[kind] = { fingerprint: "", index: 0 };
  }
}

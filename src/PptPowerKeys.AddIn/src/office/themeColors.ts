import {
  DEFAULT_PALETTE,
  loadPersistedRecentColors,
  normalizeHex,
  setThemeColors,
  type ThemeColorSource,
} from "./formatColorState";

/* global Office, PowerPoint */

export interface ThemeColorsResult {
  colors: string[];
  source: ThemeColorSource;
  warning?: string;
}

/** Theme slots read from the presentation (VSTO parity: 10 colors). */
const THEME_COLOR_SLOTS: PowerPoint.ThemeColor[] = [
  PowerPoint.ThemeColor.accent1,
  PowerPoint.ThemeColor.accent2,
  PowerPoint.ThemeColor.accent3,
  PowerPoint.ThemeColor.accent4,
  PowerPoint.ThemeColor.accent5,
  PowerPoint.ThemeColor.accent6,
  PowerPoint.ThemeColor.dark1,
  PowerPoint.ThemeColor.dark2,
  PowerPoint.ThemeColor.light1,
  PowerPoint.ThemeColor.light2,
];

function fallbackResult(warning?: string): ThemeColorsResult {
  return {
    colors: [...DEFAULT_PALETTE],
    source: "fallback",
    ...(warning ? { warning } : {}),
  };
}

/** Normalizes Office.js theme color values (may omit the leading `#`). */
function normalizeOfficeThemeHex(value: string): string {
  const trimmed = value.trim();
  if (!trimmed) {
    return "#000000";
  }

  const withHash = trimmed.startsWith("#") ? trimmed : `#${trimmed}`;
  return normalizeHex(withHash);
}

/**
 * Reads accent1–6 and dark1/2 + light1/2 from the presentation theme.
 * Falls back to {@link DEFAULT_PALETTE} when the API is unsupported or errors (e.g. Web GeneralException).
 */
export async function readPresentationThemeColors(): Promise<ThemeColorsResult> {
  if (!Office.context.requirements.isSetSupported("PowerPointApi", "1.10")) {
    return fallbackResult("PowerPointApi 1.10 is not supported on this host.");
  }

  try {
    return await PowerPoint.run(async (context) => {
      const presentation = context.presentation;
      presentation.slideMasters.load("items");
      await context.sync();

      let scheme: PowerPoint.ThemeColorScheme;
      let source: ThemeColorSource;

      if (presentation.slideMasters.items.length > 0) {
        scheme = presentation.slideMasters.items[0].themeColorScheme;
        source = "slideMaster";
      } else {
        const slides = presentation.getSelectedSlides();
        slides.load("items");
        await context.sync();

        if (slides.items.length === 0) {
          return fallbackResult("No slide master or selected slide available.");
        }

        scheme = slides.items[0].themeColorScheme;
        source = "slide";
      }

      const results = THEME_COLOR_SLOTS.map((slot) => scheme.getThemeColor(slot));
      await context.sync();

      const colors: string[] = [];
      for (const result of results) {
        const raw = result.value;
        if (raw) {
          colors.push(normalizeOfficeThemeHex(raw));
        }
      }

      if (colors.length === 0) {
        return fallbackResult("Theme color scheme returned no colors.");
      }

      return { colors, source };
    });
  } catch {
    return fallbackResult();
  }
}

/** Loads presentation theme colors into format-color state (called on task pane startup). */
export async function bootstrapThemeColors(): Promise<ThemeColorsResult> {
  loadPersistedRecentColors();
  const result = await readPresentationThemeColors();

  if (result.source === "fallback") {
    setThemeColors(null, "fallback");
    if (result.warning) {
      console.warn(`[PptPowerKeys] Theme colors: ${result.warning}`);
    }
  } else {
    setThemeColors(result.colors, result.source);
  }

  return result;
}

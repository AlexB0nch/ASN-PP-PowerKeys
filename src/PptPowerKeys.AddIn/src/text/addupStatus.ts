import { AddupStats } from "../services/types";

export type AddupDisplayMode = "all" | "sum" | "min" | "max" | "average";

const VALID_MODES = new Set<string>(["all", "sum", "min", "max", "average"]);

export const ADDUP_DISPLAY_MODE_DEFAULT: AddupDisplayMode = "all";

export const ADDUP_DISPLAY_MODE_OPTIONS: { value: AddupDisplayMode; label: string }[] = [
  { value: "all", label: "All metrics" },
  { value: "sum", label: "Sum only" },
  { value: "min", label: "Min only" },
  { value: "max", label: "Max only" },
  { value: "average", label: "Average only" },
];

export function normalizeAddupDisplayMode(mode?: string | null): AddupDisplayMode {
  if (!mode) {
    return ADDUP_DISPLAY_MODE_DEFAULT;
  }

  const lower = mode.trim().toLowerCase();
  return VALID_MODES.has(lower) ? (lower as AddupDisplayMode) : ADDUP_DISPLAY_MODE_DEFAULT;
}

function formatNumber(value: number): string {
  return String(value);
}

/**
 * Client mirror of Core AddupStatusFormatter — keep status strings in sync.
 */
export function formatAddupStatus(stats: AddupStats, mode?: string | null): string {
  if (stats.count === 0) {
    return "No numbers found in selection.";
  }

  const sum = formatNumber(stats.sum);
  const min = formatNumber(stats.min);
  const max = formatNumber(stats.max);
  const average = formatNumber(stats.average);
  const count = stats.count;

  switch (normalizeAddupDisplayMode(mode)) {
    case "sum":
      return `Sum ${sum} (${count} numbers).`;
    case "min":
      return `Min ${min} (${count} numbers).`;
    case "max":
      return `Max ${max} (${count} numbers).`;
    case "average":
      return `Avg ${average} (${count} numbers).`;
    default:
      return `Sum ${sum} · avg ${average} · min ${min} · max ${max} (${count} numbers).`;
  }
}

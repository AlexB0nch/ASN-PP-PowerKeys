// Mirrors the DTOs exposed by PptPowerKeys.Api (which serialize the
// PptPowerKeys.Core types). Kept in sync with the C# CommandCatalog via the
// /api/commands endpoint.

export type CommandCategory =
  | "Alignment"
  | "Resize"
  | "Objects"
  | "Format"
  | "Text"
  | "Slides"
  | "Settings";

export type ExecutionKind = "ServerLayout" | "HostScript" | "Settings";

export type OfficeJsSupport = "Full" | "Partial" | "None";

export interface CommandDescriptor {
  id: string;
  title: string;
  category: CommandCategory;
  execution: ExecutionKind;
  support: OfficeJsSupport;
  defaultShortcut?: string | null;
  notes?: string | null;
  key: string;
}

export interface ShapeBounds {
  id: string;
  left: number;
  top: number;
  width: number;
  height: number;
  // Computed fields are returned by the API but optional on the way in.
  right?: number;
  bottom?: number;
  centerX?: number;
  centerY?: number;
  area?: number;
}

export interface LayoutResult {
  changed: boolean;
  shapes: ShapeBounds[];
  message?: string | null;
}

export interface AddupStats {
  count: number;
  sum: number;
  min: number;
  max: number;
  average: number;
}

export interface ShortcutBinding {
  commandId: string;
  keys: string;
}

export interface UserSettings {
  profile: string;
  shortcuts: ShortcutBinding[];
  snapToGrid?: boolean;
}

export interface LayoutOptions {
  snapToGrid?: boolean;
  gridStepCm?: number;
}

export interface ProfilePresetEntry {
  profile: string;
  shortcuts: ShortcutBinding[];
}

export interface ProfilePresetsResponse {
  profiles: string[];
  presets: Record<string, ProfilePresetEntry>;
}

export interface BuildPaletteRequest {
  themeColors?: string[];
  recentColors?: string[];
  fallbackTheme?: string[];
}

export interface BuildPaletteResponse {
  palette: string[];
}

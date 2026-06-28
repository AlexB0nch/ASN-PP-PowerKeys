import { API_BASE_URL } from "../config";
import {
  AddupStats,
  CommandDescriptor,
  LayoutResult,
  ShapeBounds,
  UserSettings,
} from "./types";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });

  if (!response.ok) {
    let detail = "";
    try {
      detail = await response.text();
    } catch {
      /* ignore */
    }
    throw new Error(`API ${path} failed: ${response.status} ${detail}`);
  }

  return (await response.json()) as T;
}

export const api = {
  getCommands: () => request<CommandDescriptor[]>("/api/commands"),

  applyLayout: (command: string, shapes: ShapeBounds[], anchorIndex?: number) =>
    request<LayoutResult>("/api/layout/apply", {
      method: "POST",
      body: JSON.stringify({
        command,
        shapes,
        ...(anchorIndex !== undefined ? { anchorIndex } : {}),
      }),
    }),

  duplicateOffset: (command: string, source: ShapeBounds, gap = 0) =>
    request<ShapeBounds>("/api/objects/duplicate-offset", {
      method: "POST",
      body: JSON.stringify({ command, source, gap }),
    }),

  addup: (texts: (string | null)[]) =>
    request<AddupStats>("/api/text/addup", {
      method: "POST",
      body: JSON.stringify({ texts }),
    }),

  getSettings: () => request<UserSettings>("/api/settings"),

  saveSettings: (settings: UserSettings) =>
    request<UserSettings>("/api/settings", {
      method: "PUT",
      body: JSON.stringify(settings),
    }),
};

import { api } from "../services/api";
import { CommandDescriptor, LayoutOptions, UserSettings } from "../services/types";
import { SettingsCommandActions } from "../taskpane/runCommand";

let catalog: CommandDescriptor[] = [];
let userSettings: UserSettings | null = null;
let settingsActions: SettingsCommandActions | undefined;

export async function bootstrapCommandContext(): Promise<void> {
  const [commands, settings] = await Promise.all([api.getCommands(), api.getSettings()]);
  catalog = commands;
  userSettings = settings;
}

export function getCatalog(): readonly CommandDescriptor[] {
  return catalog;
}

export function findCommand(commandId: string): CommandDescriptor | undefined {
  return catalog.find((c) => c.id === commandId);
}

export function getUserSettings(): UserSettings | null {
  return userSettings;
}

export function updateUserSettings(settings: UserSettings): void {
  userSettings = settings;
}

export function getLayoutOptions(): LayoutOptions {
  return { snapToGrid: userSettings?.snapToGrid ?? false };
}

export function setSettingsActions(actions: SettingsCommandActions): void {
  settingsActions = actions;
}

export function getSettingsActions(): SettingsCommandActions | undefined {
  return settingsActions;
}

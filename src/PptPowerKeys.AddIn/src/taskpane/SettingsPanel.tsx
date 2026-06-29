import * as React from "react";
import {
  Button,
  Caption1,
  Checkbox,
  Dropdown,
  MessageBar,
  MessageBarBody,
  Option,
  Spinner,
  Subtitle2,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { api } from "../services/api";
import {
  CommandDescriptor,
  ProfilePresetsResponse,
  ShortcutBinding,
  UserSettings,
} from "../services/types";
import { CommandOutcome, outcomeError, outcomeSuccess } from "./runCommand";
import { syncKeyboardShortcuts } from "../runtime/syncKeyboardShortcuts";
import { ShortcutManager } from "./ShortcutManager";

const CUSTOM_PROFILE = "Custom";
const SETTINGS_SCHEMA_VERSION = 1;

function sanitizeProfileForFilename(profile: string): string {
  const trimmed = profile.trim();
  if (!trimmed || trimmed === CUSTOM_PROFILE) {
    return "";
  }

  const sanitized = trimmed.replace(/[^a-zA-Z0-9._-]+/g, "-").replace(/^-+|-+$/g, "");
  return sanitized;
}

function buildSettingsExportFilename(profile: string): string {
  const slug = sanitizeProfileForFilename(profile);
  return slug ? `ppt-powerkeys-settings-${slug}.json` : "ppt-powerkeys-settings.json";
}

function buildSettingsExportPayload(settings: UserSettings): string {
  return JSON.stringify(
    {
      schemaVersion: SETTINGS_SCHEMA_VERSION,
      profile: settings.profile,
      snapToGrid: settings.snapToGrid ?? false,
      shortcuts: settings.shortcuts,
    },
    null,
    2,
  );
}

const useStyles = makeStyles({
  root: {
    display: "flex",
    flexDirection: "column",
    gap: "10px",
    marginTop: "10px",
    paddingTop: "10px",
    borderTop: `1px solid ${tokens.colorNeutralStroke2}`,
  },
  row: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },
  actions: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
  },
});

export interface SettingsPanelHandle {
  reload: () => Promise<void>;
  scrollToShortcuts: () => void;
}

interface SettingsPanelProps {
  commands: CommandDescriptor[];
  onFeedback?: (outcome: CommandOutcome) => void;
  onSettingsUpdated?: (settings: UserSettings) => void;
}

function isPresetProfile(profile: string, presets: ProfilePresetsResponse | null): boolean {
  return presets !== null && profile in presets.presets;
}

function normalizeProfile(profile: string, presets: ProfilePresetsResponse | null): string {
  if (presets?.profiles.includes(profile)) {
    return profile;
  }
  return CUSTOM_PROFILE;
}

function cloneShortcuts(shortcuts: ShortcutBinding[]): ShortcutBinding[] {
  return shortcuts.map((s) => ({ ...s }));
}

export const SettingsPanel = React.forwardRef<SettingsPanelHandle, SettingsPanelProps>(
  function SettingsPanel({ commands, onFeedback, onSettingsUpdated }, ref) {
    const styles = useStyles();
    const shortcutsRef = React.useRef<HTMLDivElement>(null);
    const importInputRef = React.useRef<HTMLInputElement>(null);
    const [settings, setSettings] = React.useState<UserSettings | null>(null);
    const [presets, setPresets] = React.useState<ProfilePresetsResponse | null>(null);
    const [loading, setLoading] = React.useState(true);
    const [error, setError] = React.useState<string | null>(null);
    const [busy, setBusy] = React.useState<"save" | "reset" | "import" | null>(null);
    const [presetWarning, setPresetWarning] = React.useState(false);
    const [importWarning, setImportWarning] = React.useState(false);
    const [importWarnings, setImportWarnings] = React.useState<string[]>([]);

    const loadSettings = React.useCallback(async () => {
      setError(null);
      try {
        const [data, presetData] = await Promise.all([
          api.getSettings(),
          api.getProfilePresets(),
        ]);
        setSettings(data);
        setPresets(presetData);
        setPresetWarning(false);
        setImportWarning(false);
        setImportWarnings([]);
        onSettingsUpdated?.(data);
        await syncKeyboardShortcuts(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    }, [onSettingsUpdated]);

    React.useEffect(() => {
      void loadSettings();
    }, [loadSettings]);

    React.useImperativeHandle(
      ref,
      () => ({
        reload: async () => {
          setLoading(true);
          await loadSettings();
        },
        scrollToShortcuts: () => {
          shortcutsRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
        },
      }),
      [loadSettings],
    );

    const onProfileChange = (profile: string) => {
      if (!settings || !presets) {
        return;
      }

      if (profile === CUSTOM_PROFILE) {
        setSettings((prev) => (prev ? { ...prev, profile: CUSTOM_PROFILE } : prev));
        setPresetWarning(false);
        return;
      }

      const preset = presets.presets[profile];
      if (!preset) {
        setSettings((prev) => (prev ? { ...prev, profile } : prev));
        return;
      }

      setSettings({
        profile,
        shortcuts: cloneShortcuts(preset.shortcuts),
        snapToGrid: settings.snapToGrid,
      });
      setPresetWarning(true);
      setImportWarning(false);
      setImportWarnings([]);
    };

    const onExportJson = () => {
      if (!settings) {
        return;
      }

      const payload = buildSettingsExportPayload(settings);
      const blob = new Blob([payload], { type: "application/json" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = buildSettingsExportFilename(settings.profile);
      link.click();
      URL.revokeObjectURL(url);
    };

    const onImportJsonClick = () => {
      importInputRef.current?.click();
    };

    const onImportFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      event.target.value = "";
      if (!file) {
        return;
      }

      setBusy("import");
      try {
        const jsonText = await file.text();
        const result = await api.importSettings(jsonText);
        setSettings(result.settings);
        setImportWarnings(result.warnings);
        setImportWarning(true);
        setPresetWarning(false);
        onFeedback?.(outcomeSuccess("Settings imported into the editor."));
      } catch (err) {
        onFeedback?.(
          outcomeError(err instanceof Error ? err.message : String(err)),
        );
      } finally {
        setBusy(null);
      }
    };

    const onSave = async () => {
      if (!settings) {
        return;
      }
      setBusy("save");
      try {
        const saved = await api.saveSettings(settings);
        setSettings(saved);
        setPresetWarning(false);
        setImportWarning(false);
        setImportWarnings([]);
        onSettingsUpdated?.(saved);
        await syncKeyboardShortcuts(saved);
        onFeedback?.(outcomeSuccess("Settings saved."));
      } catch (err) {
        onFeedback?.(
          outcomeError(err instanceof Error ? err.message : String(err)),
        );
      } finally {
        setBusy(null);
      }
    };

    const onReset = async () => {
      setBusy("reset");
      try {
        const reset = await api.resetSettings();
        setSettings(reset);
        setPresetWarning(false);
        setImportWarning(false);
        setImportWarnings([]);
        onSettingsUpdated?.(reset);
        await syncKeyboardShortcuts(reset);
        onFeedback?.(outcomeSuccess("Settings reset to defaults."));
      } catch (err) {
        onFeedback?.(
          outcomeError(err instanceof Error ? err.message : String(err)),
        );
      } finally {
        setBusy(null);
      }
    };

    if (loading && !settings) {
      return (
        <div className={styles.root}>
          <Spinner size="tiny" label="Loading settings…" />
        </div>
      );
    }

    if (error && !settings) {
      return (
        <div className={styles.root}>
          <Subtitle2>Settings unavailable</Subtitle2>
          <Caption1>{error}</Caption1>
          <Button size="small" appearance="secondary" onClick={() => void loadSettings()}>
            Retry
          </Button>
        </div>
      );
    }

    if (!settings || !presets) {
      return null;
    }

    const profileOptions = presets.profiles;
    const selectedProfile = normalizeProfile(settings.profile, presets);

    return (
      <div className={styles.root} id="settings-panel">
        <Subtitle2>Settings</Subtitle2>

        <MessageBar intent="info">
          <MessageBarBody>
            Edit shortcuts below and click Save to apply hotkeys on Desktop Windows (PowerPoint
            2601+). On Web, use task pane buttons.
          </MessageBarBody>
        </MessageBar>

        {presetWarning && isPresetProfile(settings.profile, presets) ? (
          <MessageBar intent="warning">
            <MessageBarBody>
              Applying preset replaces current shortcuts. Click Save to persist.
            </MessageBarBody>
          </MessageBar>
        ) : null}

        {importWarning ? (
          <MessageBar intent="warning">
            <MessageBarBody>
              Imported — click Save to persist.
              {importWarnings.length > 0
                ? ` ${importWarnings.join(" ")}`
                : null}
            </MessageBarBody>
          </MessageBar>
        ) : null}

        <div className={styles.row}>
          <Caption1>Profile</Caption1>
          <Dropdown
            size="small"
            selectedOptions={[selectedProfile]}
            value={selectedProfile}
            onOptionSelect={(_e, data) => {
              const profile = data.optionValue;
              if (profile) {
                onProfileChange(profile);
              }
            }}
          >
            {profileOptions.map((profile) => (
              <Option key={profile} value={profile} text={profile}>
                {profile}
              </Option>
            ))}
          </Dropdown>
        </div>

        <div className={styles.row}>
          <Checkbox
            label="Snap to grid (0.1 cm)"
            checked={settings.snapToGrid ?? false}
            onChange={(_e, data) =>
              setSettings((prev) =>
                prev ? { ...prev, snapToGrid: Boolean(data.checked) } : prev,
              )
            }
          />
        </div>

        <div className={styles.row} ref={shortcutsRef} id="settings-shortcuts">
          <Caption1>Shortcuts</Caption1>
          <ShortcutManager
            shortcuts={settings.shortcuts}
            commands={commands}
            onChange={(shortcuts) =>
              setSettings((prev) => (prev ? { ...prev, shortcuts } : prev))
            }
          />
        </div>

        <div className={styles.actions}>
          <Button
            size="small"
            appearance="primary"
            disabled={busy !== null}
            onClick={() => void onSave()}
          >
            {busy === "save" ? "Saving…" : "Save"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={busy !== null}
            onClick={() => void onReset()}
          >
            {busy === "reset" ? "Resetting…" : "Reset to defaults"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={busy !== null}
            onClick={onExportJson}
          >
            Export JSON
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={busy !== null}
            onClick={onImportJsonClick}
          >
            {busy === "import" ? "Importing…" : "Import JSON"}
          </Button>
          <input
            ref={importInputRef}
            type="file"
            accept=".json,application/json"
            style={{ display: "none" }}
            onChange={(event) => void onImportFileSelected(event)}
          />
        </div>
      </div>
    );
  },
);

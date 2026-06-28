import * as React from "react";
import {
  Button,
  Caption1,
  Input,
  MessageBar,
  MessageBarBody,
  Spinner,
  Subtitle2,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { api } from "../services/api";
import { UserSettings } from "../services/types";
import { CommandOutcome, outcomeError, outcomeSuccess } from "./runCommand";

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
  shortcuts: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
    maxHeight: "200px",
    overflowY: "auto",
    padding: "6px",
    borderRadius: tokens.borderRadiusMedium,
    backgroundColor: tokens.colorNeutralBackground2,
  },
  shortcutRow: {
    display: "grid",
    gridTemplateColumns: "1fr auto",
    gap: "8px",
    alignItems: "center",
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
  onFeedback?: (outcome: CommandOutcome) => void;
}

export const SettingsPanel = React.forwardRef<SettingsPanelHandle, SettingsPanelProps>(
  function SettingsPanel({ onFeedback }, ref) {
    const styles = useStyles();
    const shortcutsRef = React.useRef<HTMLDivElement>(null);
    const [settings, setSettings] = React.useState<UserSettings | null>(null);
    const [loading, setLoading] = React.useState(true);
    const [error, setError] = React.useState<string | null>(null);
    const [busy, setBusy] = React.useState<"save" | "reset" | null>(null);

    const loadSettings = React.useCallback(async () => {
      setError(null);
      try {
        const data = await api.getSettings();
        setSettings(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    }, []);

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

    const onSave = async () => {
      if (!settings) {
        return;
      }
      setBusy("save");
      try {
        const saved = await api.saveSettings(settings);
        setSettings(saved);
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

    if (!settings) {
      return null;
    }

    return (
      <div className={styles.root} id="settings-panel">
        <Subtitle2>Settings</Subtitle2>

        <MessageBar intent="info">
          <MessageBarBody>
            Office Web Add-ins cannot capture global keyboard shortcuts like the desktop
            VSTO add-in. Shortcuts listed below are for reference — use the task pane
            buttons while editing in the browser.
          </MessageBarBody>
        </MessageBar>

        <div className={styles.row}>
          <Caption1>Profile</Caption1>
          <Input
            size="small"
            value={settings.profile}
            onChange={(_e, data) =>
              setSettings((prev) => (prev ? { ...prev, profile: data.value } : prev))
            }
          />
        </div>

        <div className={styles.row} ref={shortcutsRef} id="settings-shortcuts">
          <Caption1>Shortcuts (read-only)</Caption1>
          <div className={styles.shortcuts}>
            {settings.shortcuts.length === 0 ? (
              <Caption1>No shortcut bindings configured.</Caption1>
            ) : (
              settings.shortcuts.map((binding) => (
                <div className={styles.shortcutRow} key={binding.commandId}>
                  <Caption1>{binding.commandId}</Caption1>
                  <Caption1>{binding.keys}</Caption1>
                </div>
              ))
            )}
          </div>
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
        </div>
      </div>
    );
  },
);

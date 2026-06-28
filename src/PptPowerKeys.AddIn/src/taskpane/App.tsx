import * as React from "react";
import {
  Accordion,
  AccordionHeader,
  AccordionItem,
  AccordionPanel,
  Badge,
  Button,
  Caption1,
  Spinner,
  Subtitle2,
  Title3,
  Tooltip,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { api } from "../services/api";
import { CommandCategory, CommandDescriptor, OfficeJsSupport } from "../services/types";
import { runCommand, CommandOutcome, outcomeSuccess } from "./runCommand";
import { ColorPickerPanel, ColorPickerPanelHandle } from "./ColorPickerPanel";
import { SettingsPanel, SettingsPanelHandle } from "./SettingsPanel";

const useStyles = makeStyles({
  root: {
    padding: "12px",
    display: "flex",
    flexDirection: "column",
    gap: "8px",
    height: "100vh",
    boxSizing: "border-box",
  },
  legend: {
    display: "flex",
    flexWrap: "wrap",
    gap: "10px",
    alignItems: "center",
  },
  legendItem: {
    display: "inline-flex",
    alignItems: "center",
    gap: "4px",
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "1fr 1fr",
    gap: "6px",
    paddingTop: "6px",
  },
  status: {
    padding: "8px",
    borderRadius: tokens.borderRadiusMedium,
    backgroundColor: tokens.colorNeutralBackground3,
  },
  cmdButton: {
    justifyContent: "flex-start",
    minHeight: "40px",
  },
});

const CATEGORY_ORDER: CommandCategory[] = [
  "Alignment",
  "Resize",
  "Objects",
  "Format",
  "Text",
  "Slides",
  "Settings",
];

function supportColor(support: OfficeJsSupport): "success" | "warning" | "danger" {
  return support === "Full" ? "success" : support === "Partial" ? "warning" : "danger";
}

function supportLegendLabel(support: OfficeJsSupport): string {
  return support === "Full" ? "Full" : support === "Partial" ? "Partial" : "Not on Web";
}

function outcomeBadge(outcome: CommandOutcome): { color: "success" | "warning" | "danger"; label: string } {
  switch (outcome.kind) {
    case "success":
      return { color: "success", label: "OK" };
    case "unsupported":
      return { color: "warning", label: "Not on Web" };
    case "error":
      return { color: "danger", label: "Error" };
  }
}

function commandTooltip(cmd: CommandDescriptor): string {
  const shortcut = cmd.defaultShortcut ? ` · ${cmd.defaultShortcut}` : "";
  if (cmd.support === "None") {
    const reason = cmd.notes ? ` ${cmd.notes}` : "";
    return `Not available on PowerPoint Web.${reason}${shortcut}`;
  }
  return `${cmd.notes ?? cmd.title}${shortcut}`;
}

export const App: React.FC = () => {
  const styles = useStyles();
  const settingsPanelRef = React.useRef<SettingsPanelHandle>(null);
  const colorPickerPanelRef = React.useRef<ColorPickerPanelHandle>(null);
  const [commands, setCommands] = React.useState<CommandDescriptor[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [status, setStatus] = React.useState<CommandOutcome | null>(null);
  const [busy, setBusy] = React.useState<string | null>(null);
  const [openAccordionItems, setOpenAccordionItems] = React.useState<CommandCategory[]>([
    "Alignment",
    "Resize",
  ]);

  React.useEffect(() => {
    api
      .getCommands()
      .then((c) => setCommands(c))
      .catch((e) => setError(e instanceof Error ? e.message : String(e)))
      .finally(() => setLoading(false));
  }, []);

  const settingsActions = React.useMemo(
    () => ({
      openShortcutManager: async () => {
        setOpenAccordionItems((prev) =>
          prev.includes("Settings") ? prev : [...prev, "Settings"],
        );
        window.setTimeout(() => settingsPanelRef.current?.scrollToShortcuts(), 150);
        return outcomeSuccess("Shortcut manager opened.");
      },
      resetToDefaults: async () => {
        await api.resetSettings();
        await settingsPanelRef.current?.reload();
        return outcomeSuccess("Settings reset to defaults.");
      },
      openColorScheme: async () => {
        setOpenAccordionItems((prev) =>
          prev.includes("Settings") ? prev : [...prev, "Settings"],
        );
        window.setTimeout(() => {
          void colorPickerPanelRef.current?.reload();
          colorPickerPanelRef.current?.focus();
        }, 150);
        return outcomeSuccess("Color picker opened.");
      },
    }),
    [],
  );

  const onRun = React.useCallback(
    async (descriptor: CommandDescriptor) => {
      setBusy(descriptor.id);
      const outcome = await runCommand(descriptor, settingsActions);
      setStatus(outcome);
      setBusy(null);
    },
    [settingsActions],
  );

  const byCategory = React.useMemo(() => {
    const map = new Map<CommandCategory, CommandDescriptor[]>();
    for (const cmd of commands) {
      const list = map.get(cmd.category) ?? [];
      list.push(cmd);
      map.set(cmd.category, list);
    }
    return map;
  }, [commands]);

  if (loading) {
    return (
      <div className={styles.root}>
        <Spinner label="Loading commands…" />
      </div>
    );
  }

  const statusBadge = status ? outcomeBadge(status) : null;

  return (
    <div className={styles.root}>
      <Title3>PptPowerKeys</Title3>
      <Caption1>
        Alignment, resize and object tools relative to the last-selected anchor — now
        cross-platform via Office.js.
      </Caption1>

      <div className={styles.legend}>
        {(["Full", "Partial", "None"] as OfficeJsSupport[]).map((support) => (
          <span className={styles.legendItem} key={support}>
            <Badge size="tiny" color={supportColor(support)} />
            <Caption1>{supportLegendLabel(support)}</Caption1>
          </span>
        ))}
      </div>

      {error && (
        <div className={styles.status}>
          <Subtitle2>Cannot reach backend</Subtitle2>
          <Caption1>{error}</Caption1>
        </div>
      )}

      {status && statusBadge && (
        <div className={styles.status}>
          <Badge appearance="filled" color={statusBadge.color}>
            {statusBadge.label}
          </Badge>{" "}
          <Caption1>{status.message}</Caption1>
        </div>
      )}

      <Accordion
        multiple
        collapsible
        openItems={openAccordionItems}
        onToggle={(_e, data) => setOpenAccordionItems(data.openItems as CommandCategory[])}
      >
        {CATEGORY_ORDER.filter((cat) => byCategory.has(cat)).map((category) => (
          <AccordionItem value={category} key={category}>
            <AccordionHeader>{category}</AccordionHeader>
            <AccordionPanel>
              <div className={styles.grid}>
                {(byCategory.get(category) ?? []).map((cmd) => (
                  <Tooltip
                    key={cmd.id}
                    relationship="description"
                    content={commandTooltip(cmd)}
                  >
                    <Button
                      className={styles.cmdButton}
                      size="small"
                      appearance="secondary"
                      disabled={busy !== null}
                      onClick={() => onRun(cmd)}
                    >
                      <Badge size="tiny" color={supportColor(cmd.support)} /> {cmd.title}
                    </Button>
                  </Tooltip>
                ))}
              </div>
              {category === "Settings" && (
                <>
                  <ColorPickerPanel ref={colorPickerPanelRef} onFeedback={setStatus} />
                  <SettingsPanel
                    ref={settingsPanelRef}
                    commands={commands}
                    onFeedback={setStatus}
                  />
                </>
              )}
            </AccordionPanel>
          </AccordionItem>
        ))}
      </Accordion>
    </div>
  );
};

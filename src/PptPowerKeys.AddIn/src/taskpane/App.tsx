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
import { runCommand, CommandOutcome } from "./runCommand";

const useStyles = makeStyles({
  root: {
    padding: "12px",
    display: "flex",
    flexDirection: "column",
    gap: "8px",
    height: "100vh",
    boxSizing: "border-box",
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

export const App: React.FC = () => {
  const styles = useStyles();
  const [commands, setCommands] = React.useState<CommandDescriptor[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [status, setStatus] = React.useState<CommandOutcome | null>(null);
  const [busy, setBusy] = React.useState<string | null>(null);

  React.useEffect(() => {
    api
      .getCommands()
      .then((c) => setCommands(c))
      .catch((e) => setError(e instanceof Error ? e.message : String(e)))
      .finally(() => setLoading(false));
  }, []);

  const onRun = React.useCallback(async (descriptor: CommandDescriptor) => {
    setBusy(descriptor.id);
    const outcome = await runCommand(descriptor);
    setStatus(outcome);
    setBusy(null);
  }, []);

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

  return (
    <div className={styles.root}>
      <Title3>PptPowerKeys</Title3>
      <Caption1>
        Alignment, resize and object tools relative to the last-selected anchor — now
        cross-platform via Office.js.
      </Caption1>

      {error && (
        <div className={styles.status}>
          <Subtitle2>Cannot reach backend</Subtitle2>
          <Caption1>{error}</Caption1>
        </div>
      )}

      {status && (
        <div className={styles.status}>
          <Badge appearance="filled" color={status.ok ? "success" : "danger"}>
            {status.ok ? "OK" : "Error"}
          </Badge>{" "}
          <Caption1>{status.message}</Caption1>
        </div>
      )}

      <Accordion multiple collapsible defaultOpenItems={["Alignment", "Resize"]}>
        {CATEGORY_ORDER.filter((cat) => byCategory.has(cat)).map((category) => (
          <AccordionItem value={category} key={category}>
            <AccordionHeader>{category}</AccordionHeader>
            <AccordionPanel>
              <div className={styles.grid}>
                {(byCategory.get(category) ?? []).map((cmd) => (
                  <Tooltip
                    key={cmd.id}
                    relationship="description"
                    content={`${cmd.notes ?? cmd.title}${
                      cmd.defaultShortcut ? ` · ${cmd.defaultShortcut}` : ""
                    }`}
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
            </AccordionPanel>
          </AccordionItem>
        ))}
      </Accordion>
    </div>
  );
};

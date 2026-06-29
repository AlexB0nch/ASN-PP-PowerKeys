import * as React from "react";
import {
  Button,
  Caption1,
  Dropdown,
  Input,
  MessageBar,
  MessageBarBody,
  Option,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import { DeleteRegular } from "@fluentui/react-icons";
import { CommandDescriptor, ShortcutBinding } from "../services/types";
import {
  findDuplicateKeyGroups,
  isDuplicateKey,
  normalizeShortcutKeys,
  toOfficeShortcutKey,
} from "./shortcutBindings";

/* global Office */

const useStyles = makeStyles({
  root: {
    display: "flex",
    flexDirection: "column",
    gap: "8px",
  },
  list: {
    display: "flex",
    flexDirection: "column",
    gap: "6px",
    maxHeight: "280px",
    overflowY: "auto",
    padding: "6px",
    borderRadius: tokens.borderRadiusMedium,
    backgroundColor: tokens.colorNeutralBackground2,
  },
  row: {
    display: "grid",
    gridTemplateColumns: "1fr minmax(100px, 140px) auto",
    gap: "8px",
    alignItems: "center",
  },
  title: {
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },
  addRow: {
    display: "grid",
    gridTemplateColumns: "1fr minmax(100px, 140px) auto",
    gap: "8px",
    alignItems: "end",
  },
  addCommand: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
    minWidth: 0,
  },
  addKeys: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },
});

interface ShortcutManagerProps {
  shortcuts: ShortcutBinding[];
  commands: CommandDescriptor[];
  onChange: (shortcuts: ShortcutBinding[]) => void;
}

function commandTitle(commands: CommandDescriptor[], commandId: string): string {
  return commands.find((c) => c.id === commandId)?.title ?? commandId;
}

function sortCommandsForDropdown(commands: CommandDescriptor[]): CommandDescriptor[] {
  return [...commands].sort((a, b) => a.title.localeCompare(b.title));
}

export const ShortcutManager: React.FC<ShortcutManagerProps> = ({
  shortcuts,
  commands,
  onChange,
}) => {
  const styles = useStyles();
  const sortedCommands = React.useMemo(() => sortCommandsForDropdown(commands), [commands]);
  const titleById = React.useMemo(() => {
    const map = new Map<string, string>();
    for (const cmd of commands) {
      map.set(cmd.id, cmd.title);
    }
    return map;
  }, [commands]);

  const [newCommandId, setNewCommandId] = React.useState("");
  const [newKeys, setNewKeys] = React.useState("");
  const [hostConflictWarning, setHostConflictWarning] = React.useState<string | null>(null);

  const supportsKeyboardShortcuts = React.useMemo(
    () => Office.context.requirements.isSetSupported("KeyboardShortcuts", "1.1"),
    [],
  );

  const checkHostShortcutConflict = React.useCallback(
    async (keys: string) => {
      if (!supportsKeyboardShortcuts) {
        setHostConflictWarning(null);
        return;
      }
      const officeKey = toOfficeShortcutKey(keys);
      if (!officeKey) {
        setHostConflictWarning(null);
        return;
      }
      try {
        const results = await Office.actions.areShortcutsInUse([officeKey]);
        const conflict = results.find((r) => r.shortcut === officeKey && r.inUse);
        setHostConflictWarning(
          conflict
            ? `"${officeKey}" may conflict with PowerPoint or another add-in (save is still allowed).`
            : null,
        );
      } catch {
        setHostConflictWarning(null);
      }
    },
    [supportsKeyboardShortcuts],
  );

  const duplicateGroups = React.useMemo(
    () => findDuplicateKeyGroups(shortcuts),
    [shortcuts],
  );

  const duplicateCommandIds = React.useMemo(() => {
    const ids = new Set<string>();
    for (const group of duplicateGroups) {
      for (const id of group.commandIds) {
        ids.add(id);
      }
    }
    return ids;
  }, [duplicateGroups]);

  const updateKeys = (commandId: string, keys: string) => {
    onChange(
      shortcuts.map((b) => (b.commandId === commandId ? { ...b, keys } : b)),
    );
  };

  const removeBinding = (commandId: string) => {
    onChange(shortcuts.filter((b) => b.commandId !== commandId));
  };

  const addBinding = () => {
    const commandId = newCommandId.trim();
    const keys = newKeys.trim();
    if (!commandId || !keys) {
      return;
    }

    const withoutExisting = shortcuts.filter((b) => b.commandId !== commandId);
    onChange([...withoutExisting, { commandId, keys }]);
    setNewKeys("");
  };

  const duplicateWarning =
    duplicateGroups.length > 0
      ? duplicateGroups
          .map((g) => {
            const names = g.commandIds
              .map((id) => titleById.get(id) ?? id)
              .join(", ");
            return `"${g.keys}" → ${names}`;
          })
          .join("; ")
      : null;

  return (
    <div className={styles.root}>
      {duplicateWarning && (
        <MessageBar intent="warning">
          <MessageBarBody>
            Duplicate shortcut keys (save is still allowed): {duplicateWarning}
          </MessageBarBody>
        </MessageBar>
      )}

      {hostConflictWarning && (
        <MessageBar intent="warning">
          <MessageBarBody>{hostConflictWarning}</MessageBarBody>
        </MessageBar>
      )}

      <div className={styles.list}>
        {shortcuts.length === 0 ? (
          <Caption1>No shortcut bindings configured.</Caption1>
        ) : (
          shortcuts.map((binding) => {
            const duplicate = duplicateCommandIds.has(binding.commandId);
            return (
              <div
                className={styles.row}
                key={binding.commandId}
                style={
                  duplicate
                    ? {
                        outline: `1px solid ${tokens.colorPaletteDarkOrangeBorder2}`,
                        borderRadius: tokens.borderRadiusSmall,
                        padding: "2px",
                      }
                    : undefined
                }
              >
                <Caption1 className={styles.title} title={binding.commandId}>
                  {commandTitle(commands, binding.commandId)}
                </Caption1>
                <Input
                  size="small"
                  value={binding.keys}
                  aria-label={`Keys for ${commandTitle(commands, binding.commandId)}`}
                  onChange={(_e, data) => updateKeys(binding.commandId, data.value)}
                  onBlur={(e) => void checkHostShortcutConflict(e.target.value)}
                />
                <Button
                  size="small"
                  appearance="subtle"
                  icon={<DeleteRegular />}
                  aria-label={`Remove ${commandTitle(commands, binding.commandId)}`}
                  onClick={() => removeBinding(binding.commandId)}
                />
              </div>
            );
          })
        )}
      </div>

      <div className={styles.addRow}>
        <div className={styles.addCommand}>
          <Caption1>Command</Caption1>
          <Dropdown
            size="small"
            placeholder="Select command"
            selectedOptions={newCommandId ? [newCommandId] : []}
            value={
              newCommandId
                ? (titleById.get(newCommandId) ?? newCommandId)
                : undefined
            }
            onOptionSelect={(_e, data) => setNewCommandId(data.optionValue ?? "")}
          >
            {sortedCommands.map((cmd) => (
              <Option key={cmd.id} value={cmd.id} text={cmd.title}>
                {cmd.title}
              </Option>
            ))}
          </Dropdown>
        </div>
        <div className={styles.addKeys}>
          <Caption1>Keys</Caption1>
          <Input
            size="small"
            placeholder="e.g. Alt+1"
            value={newKeys}
            onChange={(_e, data) => setNewKeys(data.value)}
            onBlur={(e) => void checkHostShortcutConflict(e.target.value)}
          />
        </div>
        <Button
          size="small"
          appearance="secondary"
          disabled={!newCommandId.trim() || !normalizeShortcutKeys(newKeys)}
          onClick={addBinding}
        >
          Add
        </Button>
      </div>

      {newCommandId &&
        newKeys &&
        isDuplicateKey(shortcuts, newCommandId, newKeys) && (
          <Caption1>
            This key is already assigned to another command; adding will create a
            duplicate.
          </Caption1>
        )}

      {newCommandId && shortcuts.some((b) => b.commandId === newCommandId) && (
        <Caption1>
          &quot;{titleById.get(newCommandId) ?? newCommandId}&quot; already has a
          binding — Add will replace it.
        </Caption1>
      )}
    </div>
  );
};

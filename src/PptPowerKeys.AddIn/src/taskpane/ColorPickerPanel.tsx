import * as React from "react";
import {
  Button,
  Caption1,
  MessageBar,
  MessageBarBody,
  Spinner,
  Subtitle2,
  makeStyles,
  tokens,
} from "@fluentui/react-components";
import {
  getRecentColors,
  getThemeColorSource,
  getThemePaletteColors,
  normalizeHex,
  recordRecentColor,
  refreshActivePalette,
} from "../office/formatColorState";
import {
  applyFillColor,
  applyLineColor,
  applyTextColor,
  getSelectedShapeIds,
} from "../office/powerpoint";
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
  section: {
    display: "flex",
    flexDirection: "column",
    gap: "6px",
  },
  swatchGrid: {
    display: "grid",
    gridTemplateColumns: "repeat(5, 1fr)",
    gap: "6px",
  },
  swatch: {
    width: "100%",
    aspectRatio: "1",
    minHeight: "28px",
    borderRadius: tokens.borderRadiusSmall,
    border: `1px solid ${tokens.colorNeutralStroke1}`,
    cursor: "pointer",
    padding: 0,
    boxSizing: "border-box",
  },
  swatchSelected: {
    outline: `2px solid ${tokens.colorBrandForeground1}`,
    outlineOffset: "1px",
  },
  preview: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
  },
  previewSwatch: {
    width: "24px",
    height: "24px",
    borderRadius: tokens.borderRadiusSmall,
    border: `1px solid ${tokens.colorNeutralStroke1}`,
    flexShrink: 0,
  },
  actions: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
  },
});

export interface ColorPickerPanelHandle {
  focus: () => void;
  reload: () => Promise<void>;
}

interface ColorPickerPanelProps {
  onFeedback?: (outcome: CommandOutcome) => void;
}

export const ColorPickerPanel = React.forwardRef<ColorPickerPanelHandle, ColorPickerPanelProps>(
  function ColorPickerPanel({ onFeedback }, ref) {
    const styles = useStyles();
    const rootRef = React.useRef<HTMLDivElement>(null);
    const [loading, setLoading] = React.useState(true);
    const [paletteTick, setPaletteTick] = React.useState(0);
    const [selectedColor, setSelectedColor] = React.useState<string | null>(null);
    const [busy, setBusy] = React.useState<"fill" | "line" | "text" | null>(null);

    const reloadPalette = React.useCallback(async () => {
      setLoading(true);
      try {
        await refreshActivePalette();
        setPaletteTick((t) => t + 1);
      } finally {
        setLoading(false);
      }
    }, []);

    React.useEffect(() => {
      void reloadPalette();
    }, [reloadPalette]);

    React.useImperativeHandle(
      ref,
      () => ({
        focus: () => {
          rootRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
        },
        reload: reloadPalette,
      }),
      [reloadPalette],
    );

    const themeColors = React.useMemo(() => getThemePaletteColors(), [paletteTick]);
    const recentColors = React.useMemo(() => getRecentColors(), [paletteTick]);
    const themeSource = getThemeColorSource();

    React.useEffect(() => {
      if (selectedColor === null && themeColors.length > 0) {
        setSelectedColor(themeColors[0]);
      }
    }, [selectedColor, themeColors]);

    const applyColor = async (kind: "fill" | "line" | "text") => {
      if (!selectedColor) {
        onFeedback?.(outcomeError("Select a color first."));
        return;
      }

      const normalized = normalizeHex(selectedColor);
      setBusy(kind);

      try {
        const shapeIds = await getSelectedShapeIds();
        if (shapeIds.length === 0) {
          onFeedback?.(outcomeError("Select one or more shapes first."));
          return;
        }

        let count: number;
        switch (kind) {
          case "fill":
            count = await applyFillColor(normalized);
            break;
          case "line":
            count = await applyLineColor(normalized);
            break;
          case "text":
            count = await applyTextColor(normalized);
            break;
        }

        recordRecentColor(normalized);
        await reloadPalette();
        onFeedback?.(
          outcomeSuccess(
            `${kind === "fill" ? "Fill" : kind === "line" ? "Line" : "Text"} color ${normalized} applied to ${count} shape(s).`,
          ),
        );
      } catch (err) {
        onFeedback?.(outcomeError(err instanceof Error ? err.message : String(err)));
      } finally {
        setBusy(null);
      }
    };

    const renderSwatch = (color: string) => {
      const normalized = normalizeHex(color);
      const isSelected = selectedColor !== null && normalizeHex(selectedColor) === normalized;

      return (
        <button
          key={normalized}
          type="button"
          className={`${styles.swatch} ${isSelected ? styles.swatchSelected : ""}`}
          style={{ backgroundColor: normalized }}
          aria-label={`Color ${normalized}`}
          aria-pressed={isSelected}
          onClick={() => setSelectedColor(normalized)}
        />
      );
    };

    if (loading && paletteTick === 0) {
      return (
        <div className={styles.root} id="color-picker-panel" ref={rootRef}>
          <Spinner size="tiny" label="Loading palette…" />
        </div>
      );
    }

    return (
      <div className={styles.root} id="color-picker-panel" ref={rootRef}>
        <Subtitle2>Smart Color Picker</Subtitle2>

        {themeSource === "fallback" && (
          <MessageBar intent="warning">
            <MessageBarBody>
              Theme colors could not be read from this presentation. Showing default palette
              colors instead.
            </MessageBarBody>
          </MessageBar>
        )}

        <div className={styles.section}>
          <Caption1>Theme colors</Caption1>
          <div className={styles.swatchGrid}>{themeColors.map(renderSwatch)}</div>
        </div>

        {recentColors.length > 0 && (
          <div className={styles.section}>
            <Caption1>Recent</Caption1>
            <div className={styles.swatchGrid}>{recentColors.map(renderSwatch)}</div>
          </div>
        )}

        {selectedColor && (
          <div className={styles.preview}>
            <span
              className={styles.previewSwatch}
              style={{ backgroundColor: normalizeHex(selectedColor) }}
            />
            <Caption1>{normalizeHex(selectedColor)}</Caption1>
          </div>
        )}

        <div className={styles.actions}>
          <Button
            size="small"
            appearance="primary"
            disabled={busy !== null || !selectedColor}
            onClick={() => void applyColor("fill")}
          >
            {busy === "fill" ? "Applying…" : "Apply Fill"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={busy !== null || !selectedColor}
            onClick={() => void applyColor("line")}
          >
            {busy === "line" ? "Applying…" : "Apply Line"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={busy !== null || !selectedColor}
            onClick={() => void applyColor("text")}
          >
            {busy === "text" ? "Applying…" : "Apply Text"}
          </Button>
        </div>
      </div>
    );
  },
);

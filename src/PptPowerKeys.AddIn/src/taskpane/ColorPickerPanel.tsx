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
import {
  getRecentColors,
  getThemeColorSource,
  getThemePaletteColors,
  isValidHex,
  normalizeHex,
  recordRecentColor,
  refreshActivePalette,
  setThemeColors,
} from "../office/formatColorState";
import { readPresentationThemeColors } from "../office/themeColors";
import {
  applyFillColor,
  applyLineColor,
  applyTextColor,
  ColorPickSource,
  getSelectedShapeIds,
  readColorFromSelection,
} from "../office/powerpoint";
import { CommandOutcome, outcomeError, outcomeSuccess } from "./runCommand";

interface EyeDropperResult {
  sRGBHex: string;
}

interface EyeDropperInstance {
  open(): Promise<EyeDropperResult>;
}

type EyeDropperConstructor = new () => EyeDropperInstance;

function isEyeDropperSupported(): boolean {
  return typeof window !== "undefined" && "EyeDropper" in window;
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
  hexRow: {
    display: "flex",
    gap: "8px",
    alignItems: "flex-start",
    flexWrap: "wrap",
  },
  hexInput: {
    flex: "1 1 120px",
    minWidth: "100px",
  },
  pickRow: {
    display: "flex",
    gap: "6px",
    flexWrap: "wrap",
    alignItems: "center",
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
    const [hexInput, setHexInput] = React.useState("");
    const [hexError, setHexError] = React.useState<string | null>(null);
    const [busy, setBusy] = React.useState<
      "fill" | "line" | "text" | "pick-fill" | "pick-line" | "pick-text" | "screen" | null
    >(null);

    const reloadPalette = React.useCallback(async () => {
      setLoading(true);
      try {
        const themeResult = await readPresentationThemeColors();
        if (themeResult.source === "fallback") {
          setThemeColors(null, "fallback");
        } else {
          setThemeColors(themeResult.colors, themeResult.source);
        }
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

    React.useEffect(() => {
      if (selectedColor) {
        setHexInput(normalizeHex(selectedColor));
        setHexError(null);
      }
    }, [selectedColor]);

    const applyHexInput = React.useCallback(() => {
      const trimmed = hexInput.trim();
      if (!trimmed) {
        setHexError("Enter a HEX color (#RRGGBB or RRGGBB).");
        return;
      }
      if (!isValidHex(trimmed)) {
        setHexError("Invalid HEX. Use #RRGGBB or RRGGBB (6 hex digits).");
        return;
      }
      setHexError(null);
      setSelectedColor(normalizeHex(trimmed));
    }, [hexInput]);

    const onHexChange = React.useCallback((_e: unknown, data: { value: string }) => {
      const value = data.value;
      setHexInput(value);
      const trimmed = value.trim();
      if (!trimmed) {
        setHexError(null);
        return;
      }
      if (isValidHex(trimmed)) {
        setHexError(null);
        setSelectedColor(normalizeHex(trimmed));
      } else {
        setHexError("Invalid HEX. Use #RRGGBB or RRGGBB (6 hex digits).");
      }
    }, []);

    const pickColorFromShape = async (source: ColorPickSource) => {
      const busyKey = `pick-${source}` as const;
      setBusy(busyKey);
      try {
        const color = await readColorFromSelection(source);
        setSelectedColor(color);
        recordRecentColor(color);
        await reloadPalette();
        onFeedback?.(outcomeSuccess(`Picked ${source} color ${color} from shape.`));
      } catch (err) {
        onFeedback?.(outcomeError(err instanceof Error ? err.message : String(err)));
      } finally {
        setBusy(null);
      }
    };

    const pickColorFromScreen = async () => {
      if (!isEyeDropperSupported()) {
        return;
      }

      setBusy("screen");
      try {
        const EyeDropperCtor = (window as unknown as { EyeDropper: EyeDropperConstructor })
          .EyeDropper;
        const dropper = new EyeDropperCtor();
        const result = await dropper.open();
        const trimmed = result.sRGBHex.trim();
        if (!isValidHex(trimmed)) {
          onFeedback?.(outcomeError("Screen pick returned an invalid color."));
          return;
        }
        const color = normalizeHex(trimmed);
        setSelectedColor(color);
        recordRecentColor(color);
        await reloadPalette();
        onFeedback?.(outcomeSuccess(`Picked screen color ${color}.`));
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
          return;
        }
        onFeedback?.(outcomeError(err instanceof Error ? err.message : String(err)));
      } finally {
        setBusy(null);
      }
    };

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

    const isBusy = busy !== null;
    const eyeDropperSupported = isEyeDropperSupported();

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
            <MessageBarBody>Theme colors unavailable — using default palette.</MessageBarBody>
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

        <div className={styles.section}>
          <Caption1>Custom HEX</Caption1>
          <div className={styles.hexRow}>
            <Input
              className={styles.hexInput}
              size="small"
              placeholder="#RRGGBB"
              value={hexInput}
              aria-label="HEX color"
              disabled={isBusy}
              onChange={onHexChange}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  applyHexInput();
                }
              }}
            />
            <Button
              size="small"
              appearance="secondary"
              disabled={isBusy}
              onClick={applyHexInput}
            >
              Set
            </Button>
          </div>
          {hexError && (
            <MessageBar intent="error">
              <MessageBarBody>{hexError}</MessageBarBody>
            </MessageBar>
          )}
        </div>

        <div className={styles.section}>
          <Caption1>Pick from shape</Caption1>
          <div className={styles.pickRow}>
            <Button
              size="small"
              appearance="secondary"
              disabled={isBusy}
              onClick={() => void pickColorFromShape("fill")}
            >
              {busy === "pick-fill" ? "Picking…" : "Fill"}
            </Button>
            <Button
              size="small"
              appearance="secondary"
              disabled={isBusy}
              onClick={() => void pickColorFromShape("line")}
            >
              {busy === "pick-line" ? "Picking…" : "Line"}
            </Button>
            <Button
              size="small"
              appearance="secondary"
              disabled={isBusy}
              onClick={() => void pickColorFromShape("text")}
            >
              {busy === "pick-text" ? "Picking…" : "Text"}
            </Button>
          </div>
        </div>

        <div className={styles.section}>
          <Caption1>Screen pick</Caption1>
          <div className={styles.pickRow}>
            <Button
              size="small"
              appearance="secondary"
              disabled={isBusy || !eyeDropperSupported}
              onClick={() => void pickColorFromScreen()}
            >
              {busy === "screen" ? "Picking…" : "Screen pick"}
            </Button>
          </div>
          {!eyeDropperSupported && (
            <Caption1>Screen color picker is not available in this browser.</Caption1>
          )}
        </div>

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
            disabled={isBusy || !selectedColor}
            onClick={() => void applyColor("fill")}
          >
            {busy === "fill" ? "Applying…" : "Apply Fill"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={isBusy || !selectedColor}
            onClick={() => void applyColor("line")}
          >
            {busy === "line" ? "Applying…" : "Apply Line"}
          </Button>
          <Button
            size="small"
            appearance="secondary"
            disabled={isBusy || !selectedColor}
            onClick={() => void applyColor("text")}
          >
            {busy === "text" ? "Applying…" : "Apply Text"}
          </Button>
        </div>
      </div>
    );
  },
);

/** Tier 1 command IDs registered via Office.actions.associate (S06-001). */
export const TIER1_COMMAND_IDS = [
  "AlignLeft",
  "AlignCenterHorizontal",
  "AlignRight",
  "AlignTop",
  "AlignMiddleVertical",
  "AlignBottom",
  "DistributeHorizontal",
  "DistributeVertical",
  "SameWidth",
  "SameHeight",
  "FillColor",
  "ToggleZoom",
  "DuplicateRight",
  "AddupTextFields",
] as const;

export type Tier1CommandId = (typeof TIER1_COMMAND_IDS)[number];

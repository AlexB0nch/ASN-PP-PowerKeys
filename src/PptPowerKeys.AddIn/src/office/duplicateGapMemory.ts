/** In-memory gap memory for Smart Duplicate (per CommandId, task pane scope). */

const gaps = new Map<string, number>();

export function getDuplicateGap(commandId: string): number {
  return gaps.get(commandId) ?? 0;
}

export function setDuplicateGap(commandId: string, gap: number): void {
  gaps.set(commandId, Math.max(0, gap));
}

export function clearDuplicateGapMemory(): void {
  gaps.clear();
}

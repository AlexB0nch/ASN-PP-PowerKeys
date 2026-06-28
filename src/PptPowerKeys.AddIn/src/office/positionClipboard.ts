/** In-memory position clipboard for Copy/Paste object position (task pane scope). */

export interface PositionSnapshot {
  left: number;
  top: number;
}

let clipboard: PositionSnapshot | null = null;

export function setPositionClipboard(position: PositionSnapshot): void {
  clipboard = position;
}

export function getPositionClipboard(): PositionSnapshot | null {
  return clipboard;
}

export function clearPositionClipboard(): void {
  clipboard = null;
}

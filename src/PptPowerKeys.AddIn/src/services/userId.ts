const STORAGE_KEY = "pptpowerkeys-user-id";

/**
 * Stable anonymous user id for settings API calls until SSO is available.
 * Persisted in localStorage across sessions.
 */
export function getUserId(): string {
  let id = localStorage.getItem(STORAGE_KEY);
  if (!id) {
    id = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEY, id);
  }
  return id;
}

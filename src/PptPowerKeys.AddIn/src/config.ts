// Base URL of the PptPowerKeys ASP.NET Core backend. Overridable at build time
// via the API_BASE_URL define; defaults to the local dev server.
declare const process: { env: Record<string, string | undefined> };

export const API_BASE_URL: string =
  (typeof process !== "undefined" && process.env && process.env.API_BASE_URL) ||
  "https://localhost:7168";

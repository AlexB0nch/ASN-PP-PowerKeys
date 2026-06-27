// Base URL of the PptPowerKeys ASP.NET Core backend. Injected at build time by
// webpack DefinePlugin from the API_BASE_URL environment variable.
declare const process: { env: { API_BASE_URL?: string } };

export const API_BASE_URL: string = process.env.API_BASE_URL ?? "https://localhost:7168";

# PptPowerKeys.VstoLegacy — FROZEN

This is the original Windows-only **VSTO** add-in (C# / .NET Framework 4.8).

It is **frozen** as part of the migration to the Office Web Add-in architecture
(see [`docs/migration/`](../../docs/migration/)). No new features are added here.

- Builds only on Windows with Visual Studio + the VSTO workload.
- Does **not** build on Linux/CI and is intentionally excluded from the root
  `PptPowerKeys.sln` and the CI pipeline.
- Kept for reference and for the coexistence period (Phase 0–4) during which the
  desktop VSTO add-in and the new Web Add-in run in parallel.

New work belongs in:

- `src/PptPowerKeys.Core` — business logic (.NET 8, pure C#)
- `src/PptPowerKeys.Api` — ASP.NET Core backend
- `src/PptPowerKeys.AddIn` — Office Web Add-in (TypeScript + React)

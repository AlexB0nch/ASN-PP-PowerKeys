# AGENTS.md

## Cursor Cloud specific instructions

### What this repo is
PPT PowerKeys is a **Windows-only VSTO PowerPoint COM add-in** (C# / .NET Framework 4.8).
The product runs *inside* Microsoft PowerPoint on Windows — it has no servers, databases,
ports, or web UI. See `README.md` and `setup/environment/README.md` (Russian) for the
full Windows/Visual Studio workflow.

### Hard platform constraint (read before trying to build/run)
- **The full add-in cannot be built or run on the Linux cloud VM.** It references
  `Microsoft.Office.Interop.PowerPoint`, `Microsoft.Office.Tools.*` (VSTO), `office`,
  `System.Drawing`, and `System.Windows.Forms`, and imports the VSTO MSBuild targets
  (`Microsoft.VisualStudio.Tools.Office.targets`). These exist only on Windows with
  Visual Studio 2022 ("Office/SharePoint development" workload) + Office + the VSTO Runtime.
- Real build/debug/test is done on Windows: open `src/PptPowerKeys.sln` in VS 2022,
  build (Ctrl+Shift+B), run with F5 (launches PowerPoint with the add-in), and run the
  `PptPowerKeys.Tests` xUnit project via Test Explorer / `dotnet test`.
- There is **no linter** configured and **no CI workflow**; do not invent one.

### What CAN be verified on the Linux VM
The platform-independent business logic that the xUnit tests cover
(`src/PptPowerKeys/Commands/CommandIds.cs` enum and
`src/PptPowerKeys/Settings/UserSettings.cs` JSON round-trip) compiles and runs under
**Mono** (`mcs` / `mono`, provided by the VM snapshot). `UserSettings` uses
`System.Web.Script.Serialization.JavaScriptSerializer` (from `System.Web.Extensions`),
which Mono's net48 profile provides but the modern .NET SDK does not — so use Mono, not
`dotnet`, for any Linux compile attempt.

Quick smoke check (does not modify the repo; compiles only the portable sources):
```bash
mcs -sdk:4.8 -r:System.Web.Extensions.dll \
  src/PptPowerKeys/Commands/CommandIds.cs \
  src/PptPowerKeys/Settings/UserSettings.cs \
  /path/to/a/small/Main.cs -out:/tmp/harness.exe && mono /tmp/harness.exe
```
The rest of the sources (anything referencing `Microsoft.Office.*`, `IRibbonExtensibility`,
`System.Drawing`, etc.) will fail with `CS0234`/`CS0246` on Linux — that is expected, not a bug.

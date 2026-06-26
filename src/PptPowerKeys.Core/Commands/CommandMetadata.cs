namespace PptPowerKeys.Core.Commands;

public enum CommandCategory
{
    Alignment,
    Resize,
    Objects,
    Format,
    Text,
    Slides,
    Settings
}

/// <summary>How a command is executed in the new Office Web Add-in architecture.</summary>
public enum ExecutionKind
{
    /// <summary>Geometry computed by <c>LayoutEngine</c> on the backend, applied by the task pane.</summary>
    ServerLayout,

    /// <summary>Runs entirely in the task pane against the PowerPoint JS API (no backend math).</summary>
    HostScript,

    /// <summary>Opens task pane settings UI / mutates settings.</summary>
    Settings
}

/// <summary>Office.js feasibility classification produced by the Story 1 audit.</summary>
public enum OfficeJsSupport
{
    /// <summary>✅ Direct equivalent in the PowerPoint JavaScript API.</summary>
    Full,

    /// <summary>⚠️ Partial equivalent or workaround required.</summary>
    Partial,

    /// <summary>❌ No equivalent yet — blocker.</summary>
    None
}

/// <summary>
/// Static description of a single command: how it maps onto Office.js, which
/// category it belongs to and its default keyboard shortcut. This is the
/// machine-readable form of the VSTO → Office.js mapping table.
/// </summary>
public sealed record CommandDescriptor
{
    public required CommandIds Id { get; init; }

    public required string Title { get; init; }

    public required CommandCategory Category { get; init; }

    public required ExecutionKind Execution { get; init; }

    public required OfficeJsSupport Support { get; init; }

    public string? DefaultShortcut { get; init; }

    public string? Notes { get; init; }

    public string Key => Id.ToString();
}

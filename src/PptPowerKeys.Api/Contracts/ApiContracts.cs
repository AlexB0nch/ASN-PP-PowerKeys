using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;

namespace PptPowerKeys.Api.Contracts;

/// <summary>Request body for <c>POST /api/layout/apply</c>.</summary>
public sealed class LayoutApiRequest
{
    /// <summary>The command to run, e.g. <c>AlignLeft</c> or <c>SameWidth</c>.</summary>
    public CommandIds Command { get; set; } = CommandIds.None;

    /// <summary>Selected shapes in selection order (last shape is the anchor).</summary>
    public List<ShapeBounds> Shapes { get; set; } = new();

    public LayoutOptions? Options { get; set; }

    /// <summary>Optional explicit anchor index; defaults to the last shape.</summary>
    public int? AnchorIndex { get; set; }

    public LayoutRequest ToCore() => new()
    {
        Command = Command,
        Shapes = Shapes,
        Options = Options ?? LayoutOptions.Default,
        AnchorIndex = AnchorIndex,
    };
}

/// <summary>Request body for <c>POST /api/text/addup</c>.</summary>
public sealed class AddupApiRequest
{
    /// <summary>Text content of the selected shapes.</summary>
    public List<string?> Texts { get; set; } = new();
}

/// <summary>Request body for <c>POST /api/objects/duplicate-offset</c>.</summary>
public sealed class DuplicateApiRequest
{
    public CommandIds Command { get; set; } = CommandIds.None;

    public ShapeBounds Source { get; set; }

    public double Gap { get; set; }
}

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Tunable parameters for layout operations. All distances are in points.
/// Defaults mirror the "large" / "small" nudge increments used by the legacy
/// keyboard shortcuts.
/// </summary>
public sealed record LayoutOptions
{
    /// <summary>Step applied by the "large" increase/decrease commands.</summary>
    public double LargeStep { get; init; } = 10.0;

    /// <summary>Step applied by the "small" increase/decrease commands.</summary>
    public double SmallStep { get; init; } = 1.0;

    /// <summary>Minimum width/height a shape is allowed to shrink to.</summary>
    public double MinSize { get; init; } = 1.0;

    public static LayoutOptions Default { get; } = new();
}

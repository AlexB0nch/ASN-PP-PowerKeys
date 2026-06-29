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

    /// <summary>When true, layout results are snapped to <see cref="GridStepCm"/>.</summary>
    public bool SnapToGrid { get; init; } = false;

    /// <summary>Grid step in centimetres (default 0.1 cm Consulting Mode).</summary>
    public double GridStepCm { get; init; } = GridSnap.GridStepCm;

    public static LayoutOptions Default { get; } = new();
}

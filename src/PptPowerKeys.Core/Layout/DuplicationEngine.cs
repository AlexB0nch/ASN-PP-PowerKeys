using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;

namespace PptPowerKeys.Core.Layout;

/// <summary>
/// Computes the target position for "smart duplicate" commands. The host is still
/// responsible for actually cloning the shape via Office.js; this engine only
/// decides where the clone should land, keeping that decision testable.
/// </summary>
public static class DuplicationEngine
{
    /// <summary>
    /// Returns the bounds a duplicate of <paramref name="source"/> should occupy,
    /// offset one full shape extent (plus an optional gap) in the command's
    /// direction. Returns <c>null</c> for non-duplicate commands.
    /// </summary>
    public static ShapeBounds? ComputeDuplicate(CommandIds command, ShapeBounds source, double gap = 0.0)
    {
        return command switch
        {
            CommandIds.DuplicateRight => source.Offset(source.Width + gap, 0),
            CommandIds.DuplicateLeft => source.Offset(-(source.Width + gap), 0),
            CommandIds.DuplicateDown => source.Offset(0, source.Height + gap),
            CommandIds.DuplicateUp => source.Offset(0, -(source.Height + gap)),
            _ => null,
        };
    }

    public static bool IsDuplicateCommand(CommandIds command) => command switch
    {
        CommandIds.DuplicateRight or CommandIds.DuplicateLeft
            or CommandIds.DuplicateDown or CommandIds.DuplicateUp => true,
        _ => false,
    };

    /// <summary>
    /// Infers the gap between <paramref name="source"/> and a duplicate at
    /// <paramref name="target"/> for duplicate commands. Returns <c>null</c> for
    /// non-duplicate commands. Inverse of <see cref="ComputeDuplicate"/>.
    /// </summary>
    public static double? InferGap(CommandIds command, ShapeBounds source, ShapeBounds target)
    {
        return command switch
        {
            CommandIds.DuplicateRight => target.Left - (source.Left + source.Width),
            CommandIds.DuplicateLeft => (source.Left - target.Left) - source.Width,
            CommandIds.DuplicateDown => target.Top - (source.Top + source.Height),
            CommandIds.DuplicateUp => (source.Top - target.Top) - source.Height,
            _ => null,
        };
    }
}

using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Core.Settings;

/// <summary>
/// Fixed shortcut presets for consulting-firm profiles (McKinsey / BCG).
/// Custom profile has no preset — users keep their own bindings.
/// </summary>
public static class ConsultingProfilePresets
{
    public const string McKinsey = "McKinsey";
    public const string BCG = "BCG";
    public const string Custom = "Custom";

    private static readonly IReadOnlyList<string> KnownProfilesList =
        new[] { McKinsey, BCG, Custom };

    public static IReadOnlyList<string> KnownProfiles => KnownProfilesList;

    public static bool IsKnownProfile(string profile) =>
        KnownProfilesList.Contains(profile, StringComparer.Ordinal);

    /// <summary>
    /// Returns the shortcut bindings for a consulting profile.
    /// Custom returns an empty list; unknown profiles throw.
    /// </summary>
    public static IReadOnlyList<ShortcutBinding> GetShortcuts(string profile) =>
        profile switch
        {
            McKinsey => BuildMcKinsey(),
            BCG => BuildBcg(),
            Custom => Array.Empty<ShortcutBinding>(),
            _ => throw new ArgumentException($"Unknown profile '{profile}'.", nameof(profile)),
        };

    private static IReadOnlyList<ShortcutBinding> BuildMcKinsey() => Normalize(
        AlignBindings().Concat(
        [
            (nameof(CommandIds.SameWidth), "Alt+B"),
            (nameof(CommandIds.SameHeight), "Alt+H"),
            (nameof(CommandIds.FillColor), "Alt+G"),
            (nameof(CommandIds.OpenColorScheme), "Alt+L"),
            (nameof(CommandIds.DuplicateRight), "Alt+D"),
            (nameof(CommandIds.AddupTextFields), "Alt+A"),
        ]));

    private static IReadOnlyList<ShortcutBinding> BuildBcg() => Normalize(
        AlignBindings().Concat(
        [
            (nameof(CommandIds.SameWidth), "Ctrl+Alt+B"),
            (nameof(CommandIds.SameHeight), "Ctrl+Alt+H"),
            (nameof(CommandIds.FillColor), "Alt+G"),
            (nameof(CommandIds.LineColor), "Alt+L"),
            (nameof(CommandIds.DuplicateDown), "Alt+D"),
            (nameof(CommandIds.DuplicateRight), "Alt+Shift+D"),
            (nameof(CommandIds.AddupTextFields), "Ctrl+Alt+A"),
        ]));

    private static IEnumerable<(string CommandId, string Keys)> AlignBindings() =>
    [
        (nameof(CommandIds.AlignLeft), "Alt+1"),
        (nameof(CommandIds.AlignCenterHorizontal), "Alt+2"),
        (nameof(CommandIds.AlignRight), "Alt+3"),
        (nameof(CommandIds.AlignTop), "Alt+4"),
        (nameof(CommandIds.AlignMiddleVertical), "Alt+5"),
        (nameof(CommandIds.AlignBottom), "Alt+6"),
        (nameof(CommandIds.DistributeHorizontal), "Alt+7"),
        (nameof(CommandIds.DistributeVertical), "Alt+8"),
    ];

    private static IReadOnlyList<ShortcutBinding> Normalize(
        IEnumerable<(string CommandId, string Keys)> bindings) =>
        bindings.Select(b => new ShortcutBinding
        {
            CommandId = b.CommandId,
            Keys = ShortcutBindingValidator.NormalizeKeys(b.Keys),
        }).ToList();
}

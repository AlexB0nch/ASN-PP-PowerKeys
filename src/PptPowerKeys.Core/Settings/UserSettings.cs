using System.Text.Json;
using System.Text.Json.Serialization;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Text;

namespace PptPowerKeys.Core.Settings;

/// <summary>
/// User-configurable settings: the active profile and the list of keyboard
/// shortcut bindings. Ported from the legacy VSTO <c>UserSettings</c> but stripped
/// of all file-system access (which now lives in the API/storage layer) and
/// switched from <c>JavaScriptSerializer</c> to <c>System.Text.Json</c>.
/// The on-disk JSON shape is preserved so existing files keep working.
/// </summary>
public sealed class UserSettings
{
    public string Profile { get; set; } = "Custom";

    public bool SnapToGrid { get; set; } = false;

    public string AddupDisplayMode { get; set; } = AddupDisplayModeDefault;

    public const string AddupDisplayModeDefault = "all";

    public List<ShortcutBinding> Shortcuts { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(UserSettings settings) =>
        JsonSerializer.Serialize(settings, JsonOptions);

    public static UserSettings? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Builds the shipped defaults by reading every command in
    /// <see cref="CommandCatalog"/> that declares a default shortcut. This keeps
    /// the defaults in lock-step with the catalog instead of a hand-maintained list.
    /// </summary>
    public static UserSettings CreateDefaults() => new()
    {
        Profile = "Custom",
        SnapToGrid = false,
        AddupDisplayMode = AddupDisplayModeDefault,
        Shortcuts = CommandCatalog.All
            .Where(c => !string.IsNullOrWhiteSpace(c.DefaultShortcut))
            .Select(c => new ShortcutBinding { CommandId = c.Key, Keys = c.DefaultShortcut! })
            .ToList(),
    };
}

public sealed class ShortcutBinding
{
    public string CommandId { get; set; } = string.Empty;

    public string Keys { get; set; } = string.Empty;
}

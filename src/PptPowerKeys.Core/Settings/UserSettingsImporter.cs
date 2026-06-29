using System.Text.Json;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Text;

namespace PptPowerKeys.Core.Settings;

public sealed record SettingsImportResult(
    UserSettings? Settings,
    IReadOnlyList<string> Warnings,
    string? Error = null)
{
    public static SettingsImportResult Failed(string error) =>
        new(null, Array.Empty<string>(), error);
}

/// <summary>
/// Parses and validates user-settings JSON for import. Does not persist; callers
/// (API / task pane) apply the result after explicit user confirmation.
/// </summary>
public static class UserSettingsImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static SettingsImportResult Import(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return SettingsImportResult.Failed("Invalid JSON");
        }

        SettingsImportDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<SettingsImportDocument>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return SettingsImportResult.Failed("Invalid JSON");
        }

        if (document is null)
        {
            return SettingsImportResult.Failed("Invalid JSON");
        }

        var warnings = new List<string>();
        var shortcuts = new List<ShortcutBinding>();

        foreach (var entry in document.Shortcuts ?? new List<SettingsImportShortcut>())
        {
            var commandId = entry.CommandId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(commandId))
            {
                continue;
            }

            if (CommandCatalog.Find(commandId) is null)
            {
                warnings.Add($"Unknown commandId '{commandId}' — binding skipped.");
                continue;
            }

            var keys = entry.Keys ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keys))
            {
                continue;
            }

            var normalizedKeys = ShortcutBindingValidator.NormalizeKeys(keys);
            shortcuts.RemoveAll(b =>
                string.Equals(b.CommandId, commandId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    ShortcutBindingValidator.NormalizeKeys(b.Keys),
                    normalizedKeys,
                    StringComparison.OrdinalIgnoreCase));

            shortcuts.Add(new ShortcutBinding
            {
                CommandId = commandId,
                Keys = normalizedKeys,
            });
        }

        var addupDisplayMode = AddupStatusFormatter.ModeAll;
        if (!string.IsNullOrWhiteSpace(document.AddupDisplayMode))
        {
            var rawMode = document.AddupDisplayMode.Trim();
            if (AddupStatusFormatter.IsValidMode(rawMode))
            {
                addupDisplayMode = AddupStatusFormatter.NormalizeMode(rawMode);
            }
            else
            {
                warnings.Add(AddupStatusFormatter.UnknownModeWarning);
            }
        }

        var settings = new UserSettings
        {
            Profile = string.IsNullOrWhiteSpace(document.Profile) ? "Custom" : document.Profile.Trim(),
            SnapToGrid = document.SnapToGrid,
            AddupDisplayMode = addupDisplayMode,
            Shortcuts = shortcuts,
        };

        return new SettingsImportResult(settings, warnings);
    }

    private sealed class SettingsImportDocument
    {
        public int? SchemaVersion { get; set; }

        public string? Profile { get; set; }

        public bool SnapToGrid { get; set; }

        public string? AddupDisplayMode { get; set; }

        public List<SettingsImportShortcut>? Shortcuts { get; set; }
    }

    private sealed class SettingsImportShortcut
    {
        public string? CommandId { get; set; }

        public string? Keys { get; set; }
    }
}

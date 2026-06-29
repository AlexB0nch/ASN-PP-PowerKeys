using PptPowerKeys.Core.Settings;

namespace PptPowerKeys.Api.Contracts;

/// <summary>Response body for <c>GET /api/settings/profile-presets</c>.</summary>
public sealed class ProfilePresetsResponse
{
    public IReadOnlyList<string> Profiles { get; init; } = Array.Empty<string>();

    public IReadOnlyDictionary<string, ProfilePresetEntry> Presets { get; init; } =
        new Dictionary<string, ProfilePresetEntry>();
}

public sealed class ProfilePresetEntry
{
    public string Profile { get; init; } = string.Empty;

    public IReadOnlyList<ShortcutBinding> Shortcuts { get; init; } = Array.Empty<ShortcutBinding>();
}

/// <summary>Response body for <c>POST /api/settings/import</c> (validate-only, does not persist).</summary>
public sealed class SettingsImportResponse
{
    public UserSettings Settings { get; init; } = new();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

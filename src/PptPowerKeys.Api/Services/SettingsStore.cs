using System.Collections.Concurrent;
using PptPowerKeys.Core.Settings;

namespace PptPowerKeys.Api.Services;

/// <summary>
/// Minimal per-user settings store. This is an in-memory placeholder so the API is
/// runnable end-to-end without external infrastructure; a production deployment
/// would swap this for a database or per-user roaming storage keyed off the SSO
/// identity (see <c>getAccessToken()</c> in the task pane).
/// </summary>
public sealed class SettingsStore
{
    private const string AnonymousUser = "__anonymous__";

    private readonly ConcurrentDictionary<string, UserSettings> _byUser = new();

    public UserSettings Get(string? userId)
    {
        string key = string.IsNullOrWhiteSpace(userId) ? AnonymousUser : userId;
        return _byUser.GetOrAdd(key, _ => UserSettings.CreateDefaults());
    }

    public UserSettings Save(string? userId, UserSettings settings)
    {
        string key = string.IsNullOrWhiteSpace(userId) ? AnonymousUser : userId;
        _byUser[key] = settings;
        return settings;
    }

    public UserSettings Reset(string? userId)
    {
        string key = string.IsNullOrWhiteSpace(userId) ? AnonymousUser : userId;
        var defaults = UserSettings.CreateDefaults();
        _byUser[key] = defaults;
        return defaults;
    }
}

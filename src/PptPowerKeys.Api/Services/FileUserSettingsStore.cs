using PptPowerKeys.Core.Settings;

namespace PptPowerKeys.Api.Services;

/// <summary>
/// File-backed per-user settings store. Each user gets one JSON file under
/// <paramref name="dataPath"/>; writes are atomic (temp file + move).
/// </summary>
public sealed class FileUserSettingsStore : IUserSettingsStore
{
    internal const string AnonymousUser = "__anonymous__";

    private readonly string _dataPath;
    private readonly object _sync = new();

    public FileUserSettingsStore(string dataPath)
    {
        if (string.IsNullOrWhiteSpace(dataPath))
        {
            throw new ArgumentException("Settings data path is required.", nameof(dataPath));
        }

        _dataPath = dataPath;
    }

    public UserSettings Get(string? userId)
    {
        lock (_sync)
        {
            string key = NormalizeUserId(userId);
            string filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                var defaults = UserSettings.CreateDefaults();
                WriteAtomically(filePath, UserSettings.Serialize(defaults));
                return defaults;
            }

            string json = File.ReadAllText(filePath);
            var settings = UserSettings.Deserialize(json);
            if (settings is null)
            {
                settings = UserSettings.CreateDefaults();
                WriteAtomically(filePath, UserSettings.Serialize(settings));
            }

            return settings;
        }
    }

    public UserSettings Save(string? userId, UserSettings settings)
    {
        lock (_sync)
        {
            string key = NormalizeUserId(userId);
            WriteAtomically(GetFilePath(key), UserSettings.Serialize(settings));
            return settings;
        }
    }

    public UserSettings Reset(string? userId)
    {
        lock (_sync)
        {
            string key = NormalizeUserId(userId);
            var defaults = UserSettings.CreateDefaults();
            WriteAtomically(GetFilePath(key), UserSettings.Serialize(defaults));
            return defaults;
        }
    }

    internal static string NormalizeUserId(string? userId) =>
        string.IsNullOrWhiteSpace(userId) ? AnonymousUser : userId;

    internal static string SanitizeUserIdForFilename(string userId)
    {
        var invalid = Path.GetInvalidFileNameChars()
            .Append('/')
            .Append('\\')
            .Distinct()
            .ToArray();
        var sanitized = new string(userId.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrEmpty(sanitized) ? AnonymousUser : sanitized;
    }

    private string GetFilePath(string userKey) =>
        Path.Combine(_dataPath, SanitizeUserIdForFilename(userKey) + ".json");

    private void WriteAtomically(string filePath, string content)
    {
        Directory.CreateDirectory(_dataPath);
        string tempPath = filePath + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(tempPath, content);
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Best-effort cleanup of the temp file.
                }
            }

            throw;
        }
    }
}

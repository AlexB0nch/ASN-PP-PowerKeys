using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PptPowerKeys.Core.Settings;

namespace PptPowerKeys.Windows.Settings
{
    /// <summary>
    /// Loads and saves <see cref="UserSettings"/> to
    /// <c>%AppData%/PptPowerKeys/UserSettings.json</c> (Roaming). JSON uses
    /// camelCase property names for Web export/import compatibility.
    /// </summary>
    public sealed class WindowsUserSettingsStore
    {
        private static readonly JsonSerializerOptions SaveOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly string _filePath;
        private readonly object _sync = new();
        private UserSettings _settings;

        public WindowsUserSettingsStore(string? settingsDirectory = null)
        {
            string directory = settingsDirectory
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PptPowerKeys");
            _filePath = Path.Combine(directory, "UserSettings.json");
            _settings = LoadFromDisk();
        }

        /// <summary>Current in-memory settings (live reference).</summary>
        public UserSettings Current
        {
            get
            {
                lock (_sync)
                {
                    return _settings;
                }
            }
        }

        public void SetSnapToGrid(bool value)
        {
            lock (_sync)
            {
                if (_settings.SnapToGrid == value)
                {
                    return;
                }

                _settings.SnapToGrid = value;
                WriteAtomically(_filePath, Serialize(_settings));
            }
        }

        internal static string Serialize(UserSettings settings) =>
            JsonSerializer.Serialize(settings, SaveOptions);

        private UserSettings LoadFromDisk()
        {
            lock (_sync)
            {
                if (!File.Exists(_filePath))
                {
                    var defaults = UserSettings.CreateDefaults();
                    Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                    WriteAtomically(_filePath, Serialize(defaults));
                    return defaults;
                }

                string json = File.ReadAllText(_filePath);
                var settings = UserSettings.Deserialize(json);
                if (settings is null)
                {
                    settings = UserSettings.CreateDefaults();
                    WriteAtomically(_filePath, Serialize(settings));
                }

                return settings;
            }
        }

        private static void WriteAtomically(string filePath, string content)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = filePath + ".tmp." + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(tempPath, content);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
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
                        // Best-effort cleanup.
                    }
                }

                throw;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PptPowerKeys.Core.Colors;
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

        /// <summary>
        /// Persists full settings from the task pane. <see cref="UserSettings.RecentColors"/> on disk
        /// are preserved from the current store (not overwritten by pane edits).
        /// </summary>
        public void Save(UserSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            lock (_sync)
            {
                var recentColors = _settings.RecentColors?.ToList() ?? new List<string>();
                _settings = new UserSettings
                {
                    Profile = settings.Profile ?? UserSettings.CreateDefaults().Profile,
                    SnapToGrid = settings.SnapToGrid,
                    AddupDisplayMode = string.IsNullOrWhiteSpace(settings.AddupDisplayMode)
                        ? UserSettings.AddupDisplayModeDefault
                        : settings.AddupDisplayMode,
                    Shortcuts = settings.Shortcuts?
                        .Select(s => new ShortcutBinding
                        {
                            CommandId = s.CommandId ?? string.Empty,
                            Keys = s.Keys ?? string.Empty,
                        })
                        .ToList() ?? new List<ShortcutBinding>(),
                    RecentColors = recentColors,
                };
                WriteAtomically(_filePath, Serialize(_settings));
            }
        }

        /// <summary>Resets to catalog defaults; preserves <see cref="UserSettings.RecentColors"/>.</summary>
        public void Reset()
        {
            lock (_sync)
            {
                var recentColors = _settings.RecentColors?.ToList() ?? new List<string>();
                var defaults = UserSettings.CreateDefaults();
                defaults.RecentColors = recentColors;
                _settings = defaults;
                WriteAtomically(_filePath, Serialize(_settings));
            }
        }

        /// <summary>
        /// Validates import JSON via Core <see cref="UserSettingsImporter"/>; does not persist until
        /// <see cref="Save"/>.
        /// </summary>
        public SettingsImportResult Import(string json) =>
            UserSettingsImporter.Import(json);

        /// <summary>Returns persisted recent format colors (newest first).</summary>
        public IReadOnlyList<string> GetRecentColors()
        {
            lock (_sync)
            {
                return _settings.RecentColors?.ToList() ?? new List<string>();
            }
        }

        /// <summary>Records an applied color in recent list (FIFO, max 5, dedupe).</summary>
        public void RecordRecentColor(string hex)
        {
            if (!ThemeColor.IsValidHex(hex))
            {
                return;
            }

            string normalized;
            try
            {
                normalized = ThemeColor.NormalizeHex(hex);
            }
            catch (FormatException)
            {
                return;
            }

            lock (_sync)
            {
                _settings.RecentColors ??= new List<string>();
                _settings.RecentColors.RemoveAll(existing =>
                    ThemeColor.IsValidHex(existing)
                    && string.Equals(
                        ThemeColor.NormalizeHex(existing),
                        normalized,
                        StringComparison.OrdinalIgnoreCase));
                _settings.RecentColors.Insert(0, normalized);
                if (_settings.RecentColors.Count > ColorPaletteBuilder.MaxRecentColors)
                {
                    _settings.RecentColors = _settings.RecentColors
                        .Take(ColorPaletteBuilder.MaxRecentColors)
                        .ToList();
                }

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

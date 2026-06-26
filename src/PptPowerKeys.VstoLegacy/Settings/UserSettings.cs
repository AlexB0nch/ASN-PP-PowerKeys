using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace PptPowerKeys.Settings
{
    public class UserSettings
    {
        public string Profile { get; set; } = "Custom";

        public List<ShortcutBinding> Shortcuts { get; set; } = new List<ShortcutBinding>();

        private static string UserSettingsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "UserSettings.json");

        private static string DefaultShortcutsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "default-shortcuts.json");

        public static UserSettings LoadDefaults()
        {
            if (!File.Exists(DefaultShortcutsPath))
            {
                return CreateFallbackDefaults();
            }

            string json = File.ReadAllText(DefaultShortcutsPath);
            return Deserialize(json) ?? CreateFallbackDefaults();
        }

        public static UserSettings Load()
        {
            if (File.Exists(UserSettingsPath))
            {
                string json = File.ReadAllText(UserSettingsPath);
                UserSettings settings = Deserialize(json);
                if (settings != null)
                {
                    return settings;
                }
            }

            return LoadDefaults();
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(UserSettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(this);
            File.WriteAllText(UserSettingsPath, json);
        }

        public static string Serialize(UserSettings settings)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(settings);
        }

        public static UserSettings Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<UserSettings>(json);
        }

        private static UserSettings CreateFallbackDefaults()
        {
            return new UserSettings
            {
                Profile = "Custom",
                Shortcuts = new List<ShortcutBinding>
                {
                    new ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
                    new ShortcutBinding { CommandId = "AlignCenterHorizontal", Keys = "Alt+2" },
                    new ShortcutBinding { CommandId = "SameWidth", Keys = "Alt+B" },
                    new ShortcutBinding { CommandId = "FillColor", Keys = "Alt+G" },
                    new ShortcutBinding { CommandId = "ToggleZoom", Keys = "F1" }
                }
            };
        }
    }

    public class ShortcutBinding
    {
        public string CommandId { get; set; }

        public string Keys { get; set; }
    }
}

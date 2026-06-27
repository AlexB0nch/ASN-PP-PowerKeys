using System.Collections.Generic;
using PptPowerKeys.Settings;
using Xunit;

namespace PptPowerKeys.Tests
{
    public class UserSettingsTests
    {
        [Fact]
        public void Serialize_RoundTripsSettings()
        {
            var original = new UserSettings
            {
                Profile = "McKinsey",
                Shortcuts = new List<ShortcutBinding>
                {
                    new ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
                    new ShortcutBinding { CommandId = "FillColor", Keys = "Alt+G" }
                }
            };

            string json = UserSettings.Serialize(original);
            UserSettings restored = UserSettings.Deserialize(json);

            Assert.NotNull(restored);
            Assert.Equal("McKinsey", restored.Profile);
            Assert.Equal(2, restored.Shortcuts.Count);
            Assert.Equal("Alt+1", restored.Shortcuts[0].Keys);
        }

        [Fact]
        public void Deserialize_NullJson_ReturnsNull()
        {
            Assert.Null(UserSettings.Deserialize(null));
            Assert.Null(UserSettings.Deserialize("   "));
        }
    }
}

using PptPowerKeys.Core.Colors;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class SettingsAndCatalogTests
{
    [Fact]
    public void UserSettings_RoundTripsThroughJson()
    {
        var original = new UserSettings
        {
            Profile = "McKinsey",
            Shortcuts =
            {
                new ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
                new ShortcutBinding { CommandId = "FillColor", Keys = "Alt+G" },
            },
        };

        string json = UserSettings.Serialize(original);
        var restored = UserSettings.Deserialize(json);

        Assert.NotNull(restored);
        Assert.Equal("McKinsey", restored!.Profile);
        Assert.Equal(2, restored.Shortcuts.Count);
        Assert.Equal("Alt+1", restored.Shortcuts[0].Keys);
    }

    [Fact]
    public void UserSettings_DeserializeInvalid_ReturnsNull()
    {
        Assert.Null(UserSettings.Deserialize(null));
        Assert.Null(UserSettings.Deserialize("   "));
        Assert.Null(UserSettings.Deserialize("{ not json"));
    }

    [Fact]
    public void CreateDefaults_DerivesBindingsFromCatalog()
    {
        var defaults = UserSettings.CreateDefaults();

        Assert.NotEmpty(defaults.Shortcuts);
        // Every default binding must reference a real, known command.
        foreach (var binding in defaults.Shortcuts)
        {
            Assert.NotNull(CommandCatalog.Find(binding.CommandId));
            Assert.False(string.IsNullOrWhiteSpace(binding.Keys));
        }
    }

    [Fact]
    public void Catalog_CoversEveryCommandIdExceptNone()
    {
        foreach (CommandIds id in Enum.GetValues<CommandIds>())
        {
            if (id == CommandIds.None)
            {
                continue;
            }

            Assert.True(CommandCatalog.Find(id) is not null, $"Missing catalog entry for {id}");
        }
    }

    [Fact]
    public void Catalog_LayoutCommandsAreMarkedServerLayout()
    {
        foreach (var descriptor in CommandCatalog.All)
        {
            bool isLayout = PptPowerKeys.Core.Layout.LayoutEngine.IsLayoutCommand(descriptor.Id);
            if (isLayout)
            {
                Assert.Equal(ExecutionKind.ServerLayout, descriptor.Execution);
            }
        }
    }

    [Fact]
    public void Catalog_FindByKey_IsCaseInsensitive()
    {
        Assert.NotNull(CommandCatalog.Find("alignleft"));
        Assert.NotNull(CommandCatalog.Find("ALIGNLEFT"));
    }

    [Fact]
    public void Catalog_NoneSupportCommands_AreExactlyNineKnownIds()
    {
        var noneIds = CommandCatalog.All
            .Where(c => c.Support == OfficeJsSupport.None)
            .Select(c => c.Id.ToString())
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        string[] expected =
        [
            "FormatPainter",
            "PasteFormatted",
            "PrintSlide",
            "Regroup",
            "StartSlideShow",
            "ToggleGrid",
            "ToggleGuides",
            "ToggleSlideSorter",
            "ToggleZoom",
        ];

        Assert.Equal(expected.Length, noneIds.Length);
        Assert.Equal(expected, noneIds);
    }

    [Theory]
    [InlineData("#ffffff", "#000000")] // white bg -> black text
    [InlineData("#000000", "#FFFFFF")] // black bg -> white text
    public void ThemeColor_PicksContrastingText(string bg, string expectedText)
    {
        var color = ThemeColor.Create("c", bg);
        Assert.Equal(expectedText, color.ContrastingTextHex());
    }

    [Fact]
    public void ThemeColor_NormalizesHex()
    {
        Assert.Equal("#AABBCC", ThemeColor.Create("c", "aabbcc").Hex);
        Assert.True(ThemeColor.IsValidHex("#A1B2C3"));
        Assert.False(ThemeColor.IsValidHex("nope"));
    }
}

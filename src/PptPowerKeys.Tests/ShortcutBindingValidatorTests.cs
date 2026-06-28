using PptPowerKeys.Core.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class ShortcutBindingValidatorTests
{
    [Theory]
    [InlineData("Alt+1", "Alt+1")]
    [InlineData("  Alt+1  ", "Alt+1")]
    public void NormalizeKeys_TrimsWhitespace(string input, string expected)
    {
        Assert.Equal(expected, ShortcutBindingValidator.NormalizeKeys(input));
    }

    [Fact]
    public void FindDuplicateKeys_IgnoresEmptyKeys()
    {
        var bindings = new[]
        {
            new ShortcutBinding { CommandId = "AlignLeft", Keys = "" },
            new ShortcutBinding { CommandId = "AlignRight", Keys = "   " },
            new ShortcutBinding { CommandId = "FillColor", Keys = "Alt+G" },
        };

        Assert.Empty(ShortcutBindingValidator.FindDuplicateKeys(bindings));
    }

    [Fact]
    public void FindDuplicateKeys_DetectsCaseInsensitiveDuplicates()
    {
        var bindings = new[]
        {
            new ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
            new ShortcutBinding { CommandId = "FillColor", Keys = " alt+1 " },
            new ShortcutBinding { CommandId = "SameWidth", Keys = "Alt+B" },
        };

        var duplicates = ShortcutBindingValidator.FindDuplicateKeys(bindings);

        Assert.Single(duplicates);
        Assert.Equal("Alt+1", duplicates[0].Keys);
        Assert.Equal(new[] { "AlignLeft", "FillColor" }, duplicates[0].CommandIds);
    }

    [Fact]
    public void FindDuplicateKeys_ReturnsMultipleGroups()
    {
        var bindings = new[]
        {
            new ShortcutBinding { CommandId = "AlignLeft", Keys = "F1" },
            new ShortcutBinding { CommandId = "AlignRight", Keys = "F1" },
            new ShortcutBinding { CommandId = "FillColor", Keys = "F2" },
            new ShortcutBinding { CommandId = "SameWidth", Keys = "f2" },
        };

        var duplicates = ShortcutBindingValidator.FindDuplicateKeys(bindings);

        Assert.Equal(2, duplicates.Count);
    }

    [Fact]
    public void FindDuplicateKeys_NoDuplicates_ReturnsEmpty()
    {
        var bindings = new[]
        {
            new ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
            new ShortcutBinding { CommandId = "FillColor", Keys = "Alt+G" },
        };

        Assert.Empty(ShortcutBindingValidator.FindDuplicateKeys(bindings));
    }
}

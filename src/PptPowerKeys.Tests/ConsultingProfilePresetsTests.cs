using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class ConsultingProfilePresetsTests
{
    [Fact]
    public void KnownProfiles_ContainsMcKinseyBcgAndCustom_InOrder()
    {
        Assert.Equal(
            new[] { ConsultingProfilePresets.McKinsey, ConsultingProfilePresets.BCG, ConsultingProfilePresets.Custom },
            ConsultingProfilePresets.KnownProfiles);
    }

    [Theory]
    [InlineData(ConsultingProfilePresets.McKinsey, true)]
    [InlineData(ConsultingProfilePresets.BCG, true)]
    [InlineData(ConsultingProfilePresets.Custom, true)]
    [InlineData("Team", false)]
    [InlineData("mckinsey", false)]
    public void IsKnownProfile_IsCaseSensitive(string profile, bool expected) =>
        Assert.Equal(expected, ConsultingProfilePresets.IsKnownProfile(profile));

    [Fact]
    public void GetShortcuts_Custom_ReturnsEmpty()
    {
        var shortcuts = ConsultingProfilePresets.GetShortcuts(ConsultingProfilePresets.Custom);
        Assert.Empty(shortcuts);
    }

    [Fact]
    public void GetShortcuts_Unknown_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ConsultingProfilePresets.GetShortcuts("Unknown"));
        Assert.Equal("profile", ex.ParamName);
    }

    [Theory]
    [InlineData(ConsultingProfilePresets.McKinsey)]
    [InlineData(ConsultingProfilePresets.BCG)]
    public void Preset_HasAtLeastFiveBindings_AllCommandIdsInCatalog(string profile)
    {
        var shortcuts = ConsultingProfilePresets.GetShortcuts(profile);

        Assert.True(shortcuts.Count >= 5);
        foreach (var binding in shortcuts)
        {
            Assert.NotNull(CommandCatalog.Find(binding.CommandId));
            Assert.False(string.IsNullOrWhiteSpace(binding.Keys));
        }
    }

    [Theory]
    [InlineData(ConsultingProfilePresets.McKinsey)]
    [InlineData(ConsultingProfilePresets.BCG)]
    public void Preset_HasNoDuplicateKeys(string profile)
    {
        var shortcuts = ConsultingProfilePresets.GetShortcuts(profile);
        var duplicates = ShortcutBindingValidator.FindDuplicateKeys(shortcuts);
        Assert.Empty(duplicates);
    }

    [Fact]
    public void McKinseyAndBcg_DifferInAtLeastOneBinding()
    {
        var mckinsey = ConsultingProfilePresets.GetShortcuts(ConsultingProfilePresets.McKinsey);
        var bcg = ConsultingProfilePresets.GetShortcuts(ConsultingProfilePresets.BCG);

        bool identical = mckinsey.Count == bcg.Count
            && mckinsey.All(m => bcg.Any(b =>
                b.CommandId == m.CommandId
                && string.Equals(b.Keys, m.Keys, StringComparison.OrdinalIgnoreCase)));

        Assert.False(identical);
    }

    [Fact]
    public void McKinsey_IncludesConsultingSpecificBindings()
    {
        var shortcuts = ConsultingProfilePresets.GetShortcuts(ConsultingProfilePresets.McKinsey);

        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.OpenColorScheme) && b.Keys == "Alt+L");
        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.DuplicateRight) && b.Keys == "Alt+D");
        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.AddupTextFields) && b.Keys == "Alt+A");
    }

    [Fact]
    public void Bcg_UsesDistinctResizeAndDuplicateBindings()
    {
        var shortcuts = ConsultingProfilePresets.GetShortcuts(ConsultingProfilePresets.BCG);

        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.SameWidth) && b.Keys == "Ctrl+Alt+B");
        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.LineColor) && b.Keys == "Alt+L");
        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.DuplicateDown) && b.Keys == "Alt+D");
        Assert.Contains(shortcuts, b => b.CommandId == nameof(CommandIds.DuplicateRight) && b.Keys == "Alt+Shift+D");
    }
}

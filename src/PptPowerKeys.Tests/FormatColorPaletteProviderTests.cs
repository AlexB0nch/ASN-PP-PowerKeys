using PptPowerKeys.Core.Colors;
using PptPowerKeys.Windows.Host;
using PptPowerKeys.Windows.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class FormatColorPaletteProviderTests
{
    [Fact]
    public void BuildCyclingPalette_merges_theme_and_recent()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pptpowerkeys-palette", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            store.RecordRecentColor("#ABCDEF");
            var host = new ThemedStubHost(["#111111", "#222222"]);
            var provider = new FormatColorPaletteProvider(host, store);

            var palette = provider.BuildCyclingPalette();

            Assert.Contains("#111111", palette);
            Assert.Contains("#222222", palette);
            Assert.Contains("#ABCDEF", palette);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void GetThemeSwatches_uses_fallback_when_theme_empty()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pptpowerkeys-palette", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            var host = new ThemedStubHost(Array.Empty<string>());
            var provider = new FormatColorPaletteProvider(host, store);

            Assert.True(provider.UsesFallbackTheme);
            var swatches = provider.GetThemeSwatches();
            Assert.Equal(DefaultColorPalette.FallbackTheme.Length, swatches.Count);
            Assert.Equal(ThemeColor.NormalizeHex(DefaultColorPalette.FallbackTheme[0]), swatches[0]);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void GetRecentSwatches_returns_normalized_recent_colors()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pptpowerkeys-palette", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            store.RecordRecentColor("aabbcc");
            var host = new ThemedStubHost(["#111111"]);
            var provider = new FormatColorPaletteProvider(host, store);

            var recent = provider.GetRecentSwatches();
            Assert.Single(recent);
            Assert.Equal("#AABBCC", recent[0]);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class ThemedStubHost : StubComHostAdapter
    {
        private readonly IReadOnlyList<string> _theme;

        public ThemedStubHost(IReadOnlyList<string> theme) => _theme = theme;

        public override IReadOnlyList<string> ReadPresentationThemeColors() => _theme;
    }
}

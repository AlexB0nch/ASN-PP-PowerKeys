using System.IO;
using PptPowerKeys.Windows.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class WindowsRecentColorsTests
{
    [Fact]
    public void RecordRecentColor_persists_deduped_fifo_list()
    {
        string directory = Path.Combine(Path.GetTempPath(), "pptpk-test-" + Guid.NewGuid().ToString("N"));
        var store = new WindowsUserSettingsStore(directory);

        store.RecordRecentColor("#FF0000");
        store.RecordRecentColor("#00FF00");
        store.RecordRecentColor("#ff0000");

        var recent = store.GetRecentColors();
        Assert.Equal(["#FF0000", "#00FF00"], recent);

        for (int i = 1; i <= 6; i++)
        {
            store.RecordRecentColor($"#{i:X2}{i:X2}{i:X2}");
        }

        Assert.Equal(5, store.GetRecentColors().Count);
    }
}

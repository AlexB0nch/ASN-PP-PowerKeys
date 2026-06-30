using PptPowerKeys.Windows.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class WindowsUserSettingsStoreTests
{
    [Fact]
    public void SetSnapToGrid_PersistsAcrossNewInstance()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store1 = new WindowsUserSettingsStore(tempDir);
            store1.SetSnapToGrid(true);

            var store2 = new WindowsUserSettingsStore(tempDir);
            Assert.True(store2.Current.SnapToGrid);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Save_WritesSnapToGridCamelCase()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            store.SetSnapToGrid(true);

            string json = File.ReadAllText(Path.Combine(tempDir, "UserSettings.json"));
            Assert.Contains("\"snapToGrid\": true", json);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingFile_CreatesDefaultsWithSnapOff()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            Assert.False(store.Current.SnapToGrid);
            Assert.True(File.Exists(Path.Combine(tempDir, "UserSettings.json")));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTempDir() =>
        Path.Combine(Path.GetTempPath(), "pptpowerkeys-windows-settings", Guid.NewGuid().ToString("N"));
}

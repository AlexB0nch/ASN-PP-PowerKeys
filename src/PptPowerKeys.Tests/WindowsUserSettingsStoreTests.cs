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

    [Fact]
    public void Save_PersistsProfileShortcutsSnapAndAddupMode()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            store.RecordRecentColor("#FF0000");

            var toSave = new Core.Settings.UserSettings
            {
                Profile = "McKinsey",
                SnapToGrid = true,
                AddupDisplayMode = "sum",
                Shortcuts =
                [
                    new Core.Settings.ShortcutBinding { CommandId = "AlignLeft", Keys = "Alt+1" },
                ],
            };

            store.Save(toSave);

            var reloaded = new WindowsUserSettingsStore(tempDir);
            Assert.Equal("McKinsey", reloaded.Current.Profile);
            Assert.True(reloaded.Current.SnapToGrid);
            Assert.Equal("sum", reloaded.Current.AddupDisplayMode);
            Assert.Single(reloaded.Current.Shortcuts);
            Assert.Equal("AlignLeft", reloaded.Current.Shortcuts[0].CommandId);
            Assert.Contains("#FF0000", reloaded.GetRecentColors());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Reset_RestoresCatalogDefaults_PreservesRecentColors()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            store.Save(new Core.Settings.UserSettings
            {
                Profile = "BCG",
                SnapToGrid = true,
                Shortcuts = [new Core.Settings.ShortcutBinding { CommandId = "AlignLeft", Keys = "X" }],
            });
            store.RecordRecentColor("#00FF00");

            store.Reset();

            Assert.Equal("Custom", store.Current.Profile);
            Assert.False(store.Current.SnapToGrid);
            Assert.Contains("#00FF00", store.GetRecentColors());
            Assert.True(store.Current.Shortcuts.Count > 1);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Import_ValidJson_ReturnsSettingsWithoutPersistingUntilSave()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            const string json = """
                {
                  "schemaVersion": 1,
                  "profile": "Custom",
                  "snapToGrid": true,
                  "addupDisplayMode": "max",
                  "shortcuts": [
                    { "commandId": "AlignLeft", "keys": "Alt+1" }
                  ]
                }
                """;

            var result = store.Import(json);

            Assert.Null(result.Error);
            Assert.NotNull(result.Settings);
            Assert.True(result.Settings!.SnapToGrid);
            Assert.Equal("max", result.Settings.AddupDisplayMode);
            Assert.False(store.Current.SnapToGrid);

            store.Save(result.Settings);
            Assert.True(new WindowsUserSettingsStore(tempDir).Current.SnapToGrid);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTempDir() =>
        Path.Combine(Path.GetTempPath(), "pptpowerkeys-windows-settings", Guid.NewGuid().ToString("N"));
}

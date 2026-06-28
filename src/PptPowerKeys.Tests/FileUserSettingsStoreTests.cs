using PptPowerKeys.Api.Services;
using PptPowerKeys.Core.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class FileUserSettingsStoreTests
{
    [Fact]
    public void SaveThenNewInstance_ReturnsPersistedData()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store1 = new FileUserSettingsStore(tempDir);
            var custom = new UserSettings
            {
                Profile = "Persisted",
                Shortcuts =
                {
                    new ShortcutBinding { CommandId = "AlignLeft", Keys = "Ctrl+1" },
                },
            };
            store1.Save("user-123", custom);

            var store2 = new FileUserSettingsStore(tempDir);
            var loaded = store2.Get("user-123");

            Assert.Equal("Persisted", loaded.Profile);
            Assert.Single(loaded.Shortcuts);
            Assert.Equal("Ctrl+1", loaded.Shortcuts[0].Keys);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Get_WhenFileMissing_CreatesDefaultsOnDisk()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new FileUserSettingsStore(tempDir);
            var settings = store.Get(null);

            Assert.NotEmpty(settings.Shortcuts);
            Assert.True(File.Exists(Path.Combine(tempDir, "__anonymous__.json")));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Reset_OverwritesFileWithDefaults()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new FileUserSettingsStore(tempDir);
            store.Save("alice", new UserSettings
            {
                Profile = "CustomTeam",
                Shortcuts = { new ShortcutBinding { CommandId = "AlignLeft", Keys = "X" } },
            });

            var reset = store.Reset("alice");
            var loaded = new FileUserSettingsStore(tempDir).Get("alice");

            Assert.Equal(reset.Profile, loaded.Profile);
            Assert.NotEqual("CustomTeam", loaded.Profile);
            Assert.True(loaded.Shortcuts.Count > 1);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Save_SanitizesUnsafeUserIdInFilename()
    {
        string tempDir = CreateTempDir();
        try
        {
            var store = new FileUserSettingsStore(tempDir);
            store.Save("user/with\\slashes", new UserSettings { Profile = "SafeName" });

            Assert.True(File.Exists(Path.Combine(tempDir, "user_with_slashes.json")));
            Assert.Equal("SafeName", new FileUserSettingsStore(tempDir).Get("user/with\\slashes").Profile);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTempDir() =>
        Path.Combine(Path.GetTempPath(), "pptpowerkeys-store-test", Guid.NewGuid().ToString("N"));
}

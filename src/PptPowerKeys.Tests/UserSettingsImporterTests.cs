using PptPowerKeys.Core.Settings;
using PptPowerKeys.Core.Text;
using Xunit;

namespace PptPowerKeys.Tests;

public class UserSettingsImporterTests
{
    [Fact]
    public void Import_ValidJson_ReturnsSettings()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "profile": "McKinsey",
              "snapToGrid": false,
              "shortcuts": [
                { "commandId": "AlignLeft", "keys": "Alt+1" },
                { "commandId": "AlignRight", "keys": "Alt+3" }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Empty(result.Warnings);
        Assert.Equal("McKinsey", result.Settings!.Profile);
        Assert.False(result.Settings.SnapToGrid);
        Assert.Equal(2, result.Settings.Shortcuts.Count);
        Assert.Equal("AlignLeft", result.Settings.Shortcuts[0].CommandId);
        Assert.Equal("Alt+1", result.Settings.Shortcuts[0].Keys);
    }

    [Fact]
    public void Import_InvalidJson_ReturnsFailed()
    {
        var result = UserSettingsImporter.Import("{ not json");

        Assert.Null(result.Settings);
        Assert.Equal("Invalid JSON", result.Error);
    }

    [Fact]
    public void Import_UnknownCommandId_ReturnsWarningAndSkipsBinding()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": [
                { "commandId": "AlignLeft", "keys": "Alt+1" },
                { "commandId": "NotARealCommand", "keys": "Alt+9" }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Single(result.Settings!.Shortcuts);
        Assert.Equal("AlignLeft", result.Settings.Shortcuts[0].CommandId);
        Assert.Single(result.Warnings);
        Assert.Contains("NotARealCommand", result.Warnings[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Import_SnapToGrid_RoundTrips()
    {
        const string json = """
            {
              "profile": "Custom",
              "snapToGrid": true,
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.True(result.Settings!.SnapToGrid);
    }

    [Fact]
    public void Import_MissingSnapToGrid_DefaultsFalse()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.False(result.Settings!.SnapToGrid);
    }

    [Fact]
    public void Import_AddupDisplayMode_RoundTrips()
    {
        const string json = """
            {
              "profile": "Custom",
              "addupDisplayMode": "min",
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Equal("min", result.Settings!.AddupDisplayMode);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Import_MissingAddupDisplayMode_DefaultsAll()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Equal("all", result.Settings!.AddupDisplayMode);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Import_InvalidAddupDisplayMode_DefaultsAllWithWarning()
    {
        const string json = """
            {
              "profile": "Custom",
              "addupDisplayMode": "median",
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Equal("all", result.Settings!.AddupDisplayMode);
        Assert.Single(result.Warnings);
        Assert.Equal(AddupStatusFormatter.UnknownModeWarning, result.Warnings[0]);
    }

    [Fact]
    public void Import_AddupDisplayMode_NormalizesCase()
    {
        const string json = """
            {
              "profile": "Custom",
              "addupDisplayMode": "MAX",
              "shortcuts": []
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Equal("max", result.Settings!.AddupDisplayMode);
    }

    [Fact]
    public void Import_DuplicateKeys_LastWins()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": [
                { "commandId": "AlignLeft", "keys": "Alt+1" },
                { "commandId": "AlignRight", "keys": "Alt+1" }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Single(result.Settings!.Shortcuts);
        Assert.Equal("AlignRight", result.Settings.Shortcuts[0].CommandId);
        Assert.Equal("Alt+1", result.Settings.Shortcuts[0].Keys);
    }

    [Fact]
    public void Import_DuplicateCommandId_LastWins()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": [
                { "commandId": "AlignLeft", "keys": "Alt+1" },
                { "commandId": "AlignLeft", "keys": "Alt+2" }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Single(result.Settings!.Shortcuts);
        Assert.Equal("AlignLeft", result.Settings.Shortcuts[0].CommandId);
        Assert.Equal("Alt+2", result.Settings.Shortcuts[0].Keys);
    }

    [Fact]
    public void Import_WhitespaceOnlyKeys_SkipsBinding()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": [
                { "commandId": "AlignLeft", "keys": "   " }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Empty(result.Settings!.Shortcuts);
    }

    [Fact]
    public void Import_EmptyCommandId_SkipsBinding()
    {
        const string json = """
            {
              "profile": "Custom",
              "shortcuts": [
                { "commandId": "", "keys": "Alt+1" },
                { "commandId": "AlignLeft", "keys": "Alt+1" }
              ]
            }
            """;

        var result = UserSettingsImporter.Import(json);

        Assert.Null(result.Error);
        Assert.NotNull(result.Settings);
        Assert.Single(result.Settings!.Shortcuts);
        Assert.Equal("AlignLeft", result.Settings.Shortcuts[0].CommandId);
    }
}

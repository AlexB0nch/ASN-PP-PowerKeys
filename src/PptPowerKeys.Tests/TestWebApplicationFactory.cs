using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PptPowerKeys.Tests;

/// <summary>
/// Isolates integration tests with a unique temp settings directory per fixture instance.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string? _previousSettingsDataPath;

    public string SettingsDataPath { get; }

    public TestWebApplicationFactory()
    {
        SettingsDataPath = Path.Combine(Path.GetTempPath(), "pptpowerkeys-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(SettingsDataPath);
        _previousSettingsDataPath = Environment.GetEnvironmentVariable("SETTINGS_DATA_PATH");
        Environment.SetEnvironmentVariable("SETTINGS_DATA_PATH", SettingsDataPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Settings:DataPath"] = SettingsDataPath,
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Environment.SetEnvironmentVariable("SETTINGS_DATA_PATH", _previousSettingsDataPath);

            if (Directory.Exists(SettingsDataPath))
            {
                try
                {
                    Directory.Delete(SettingsDataPath, recursive: true);
                }
                catch
                {
                    // Best-effort cleanup; temp dir may still be locked on Windows CI.
                }
            }
        }

        base.Dispose(disposing);
    }
}

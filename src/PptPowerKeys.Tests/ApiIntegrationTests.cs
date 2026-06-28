using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using PptPowerKeys.Core.Geometry;
using Xunit;

namespace PptPowerKeys.Tests;

public class ApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ApiIntegrationTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetCommands_ReturnsCatalog()
    {
        var client = _factory.CreateClient();
        var doc = await client.GetFromJsonAsync<JsonElement>("/api/commands");
        Assert.True(doc.GetArrayLength() > 50);
    }

    [Fact]
    public async Task ApplyLayout_AlignLeft_MovesShape()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            command = "AlignLeft",
            shapes = new[]
            {
                new { id = "a", left = 100.0, top = 10.0, width = 50.0, height = 20.0 },
                new { id = "b", left = 30.0, top = 200.0, width = 80.0, height = 40.0 },
            },
        };

        var response = await client.PostAsJsonAsync("/api/layout/apply", body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.GetProperty("changed").GetBoolean());
        Assert.Equal(30.0, result.GetProperty("shapes")[0].GetProperty("left").GetDouble(), 6);
    }

    [Fact]
    public async Task ApplyLayout_NoneCommand_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/layout/apply", new { command = "None", shapes = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Addup_SumsNumbers()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/text/addup", new { texts = new[] { "10", "5.5" } });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(15.5, result.GetProperty("sum").GetDouble(), 6);
    }

    [Fact]
    public async Task Settings_DefaultsThenSaveRoundTrips()
    {
        var client = _factory.CreateClient();

        var defaults = await client.GetFromJsonAsync<JsonElement>("/api/settings");
        Assert.True(defaults.GetProperty("shortcuts").GetArrayLength() > 0);

        var updated = new
        {
            profile = "Team",
            shortcuts = new[] { new { commandId = "AlignLeft", keys = "Ctrl+1" } },
        };
        var put = await client.PutAsJsonAsync("/api/settings", updated);
        put.EnsureSuccessStatusCode();

        var saved = await put.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Team", saved.GetProperty("profile").GetString());
    }

    [Fact]
    public async Task Settings_PersistAcrossFactoryRestart()
    {
        string dataPath = _factory.SettingsDataPath;
        var client1 = _factory.CreateClient();
        var updated = new
        {
            profile = "SurvivesRestart",
            shortcuts = new[] { new { commandId = "AlignLeft", keys = "Ctrl+9" } },
        };
        var put = await client1.PutAsJsonAsync("/api/settings", updated);
        put.EnsureSuccessStatusCode();

        Environment.SetEnvironmentVariable("SETTINGS_DATA_PATH", dataPath);
        await using var factory2 = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Settings:DataPath"] = dataPath,
                });
            });
        });

        var client2 = factory2.CreateClient();
        var loaded = await client2.GetFromJsonAsync<JsonElement>("/api/settings");
        Assert.Equal("SurvivesRestart", loaded.GetProperty("profile").GetString());
    }

    [Fact]
    public async Task Cors_AllowsGitHubPagesOrigin()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/commands");
        request.Headers.Add("Origin", "https://alexb0nch.github.io");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains("https://alexb0nch.github.io", values);
    }

    [Fact]
    public async Task BuildPalette_MergesThemeAndRecent_Deduplicates()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            themeColors = new[] { "#FF0000", "#00FF00" },
            recentColors = new[] { "#ff0000", "#0000FF" },
            fallbackTheme = new[] { "#AABBCC" },
        };

        var response = await client.PostAsJsonAsync("/api/colors/build-palette", body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var palette = result.GetProperty("palette");
        Assert.Equal(3, palette.GetArrayLength());
        Assert.Equal("#FF0000", palette[0].GetString());
        Assert.Equal("#00FF00", palette[1].GetString());
        Assert.Equal("#0000FF", palette[2].GetString());
    }

    [Fact]
    public async Task BuildPalette_EmptyTheme_UsesFallback()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            themeColors = Array.Empty<string>(),
            recentColors = new[] { "#123456" },
            fallbackTheme = new[] { "#4472C4", "#ED7D31" },
        };

        var response = await client.PostAsJsonAsync("/api/colors/build-palette", body);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var palette = result.GetProperty("palette");
        Assert.Equal(3, palette.GetArrayLength());
        Assert.Equal("#4472C4", palette[0].GetString());
        Assert.Equal("#ED7D31", palette[1].GetString());
        Assert.Equal("#123456", palette[2].GetString());
    }
}

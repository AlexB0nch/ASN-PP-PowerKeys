using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PptPowerKeys.Core.Geometry;
using Xunit;

namespace PptPowerKeys.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ApiIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

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
}

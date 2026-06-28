using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PptPowerKeys.Api.Contracts;
using PptPowerKeys.Api.Services;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Settings;
using PptPowerKeys.Core.Layout;
using PptPowerKeys.Core.Text;

var builder = WebApplication.CreateBuilder(args);

// Container hosts (Render, Fly, Railway, …) inject the listen port via $PORT
// rather than ASPNETCORE_URLS. Honour it so the same image runs anywhere; the
// Dockerfile's ASPNETCORE_URLS=http://+:8080 remains the default fallback.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// Serialize enums as their string names so the TypeScript client can send
// "AlignLeft" instead of a magic integer.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PptPowerKeys API", Version = "v1" });
});

var settingsDataPath = builder.Configuration["Settings:DataPath"]
    ?? Environment.GetEnvironmentVariable("SETTINGS_DATA_PATH")
    ?? "/data/settings";

builder.Services.AddSingleton<IUserSettingsStore>(_ => new FileUserSettingsStore(settingsDataPath));

// The Office task pane runs on a different origin (the add-in dev server / CDN),
// so CORS is required. Origins are configurable via "Cors:AllowedOrigins".
// Production also allows the GitHub Pages host used for the add-in bundle.
const string CorsPolicy = "AddInClients";
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
var allowedOrigins = configuredOrigins
    .Concat(new[]
    {
        "https://localhost:3000",
        "https://alexb0nch.github.io",
    })
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return false;
            }

            if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            // Office Online may load the task pane from a public static host while
            // the exact path varies (GitHub Pages project sites, custom domains).
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps
                && uri.Host.Equals("alexb0nch.github.io", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsPolicy);

// ── Health ──────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("System");

// ── Command catalog (drives the task pane UI + migration audit) ──────────────
app.MapGet("/api/commands", () => Results.Ok(CommandCatalog.All))
    .WithName("GetCommands")
    .WithTags("Commands");

app.MapGet("/api/commands/{id}", (string id) =>
{
    var descriptor = CommandCatalog.Find(id);
    return descriptor is null ? Results.NotFound() : Results.Ok(descriptor);
})
    .WithName("GetCommand")
    .WithTags("Commands");

// ── Layout (the extracted business logic) ────────────────────────────────────
app.MapPost("/api/layout/apply", ([FromBody] LayoutApiRequest request) =>
{
    if (request.Command == CommandIds.None)
    {
        return Results.BadRequest(new { error = "Command is required." });
    }

    var result = LayoutEngine.Apply(request.ToCore());
    return Results.Ok(result);
})
    .WithName("ApplyLayout")
    .WithTags("Layout");

// ── Smart duplicate target position ──────────────────────────────────────────
app.MapPost("/api/objects/duplicate-offset", ([FromBody] DuplicateApiRequest request) =>
{
    var target = DuplicationEngine.ComputeDuplicate(request.Command, request.Source, request.Gap);
    return target is null
        ? Results.BadRequest(new { error = $"'{request.Command}' is not a duplicate command." })
        : Results.Ok(target.Value);
})
    .WithName("DuplicateOffset")
    .WithTags("Objects");

// ── Text aggregation (Addup) ─────────────────────────────────────────────────
app.MapPost("/api/text/addup", ([FromBody] AddupApiRequest request) =>
{
    var stats = NumberAggregator.Compute(request.Texts);
    return Results.Ok(stats);
})
    .WithName("AddupText")
    .WithTags("Text");

// ── Settings ─────────────────────────────────────────────────────────────────
// X-User-Id stands in for the SSO identity that getAccessToken() would supply.
app.MapGet("/api/settings", (IUserSettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId) =>
        Results.Ok(store.Get(userId)))
    .WithName("GetSettings")
    .WithTags("Settings");

app.MapPut("/api/settings", (IUserSettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId,
        [FromBody] UserSettings settings) =>
        Results.Ok(store.Save(userId, settings)))
    .WithName("SaveSettings")
    .WithTags("Settings");

app.MapPost("/api/settings/reset", (IUserSettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId) =>
        Results.Ok(store.Reset(userId)))
    .WithName("ResetSettings")
    .WithTags("Settings");

app.Run();

// Exposed for integration testing via WebApplicationFactory.
public partial class Program;

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PptPowerKeys.Api.Contracts;
using PptPowerKeys.Api.Services;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Layout;
using PptPowerKeys.Core.Text;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton<SettingsStore>();

// The Office task pane runs on a different origin (the add-in dev server / CDN),
// so CORS is required. Origins are configurable via "Cors:AllowedOrigins".
const string CorsPolicy = "AddInClients";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
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
app.MapGet("/api/settings", (SettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId) =>
        Results.Ok(store.Get(userId)))
    .WithName("GetSettings")
    .WithTags("Settings");

app.MapPut("/api/settings", (SettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId,
        [FromBody] PptPowerKeys.Core.Settings.UserSettings settings) =>
        Results.Ok(store.Save(userId, settings)))
    .WithName("SaveSettings")
    .WithTags("Settings");

app.MapPost("/api/settings/reset", (SettingsStore store, [FromHeader(Name = "X-User-Id")] string? userId) =>
        Results.Ok(store.Reset(userId)))
    .WithName("ResetSettings")
    .WithTags("Settings");

app.Run();

// Exposed for integration testing via WebApplicationFactory.
public partial class Program;

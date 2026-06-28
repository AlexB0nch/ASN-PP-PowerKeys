namespace PptPowerKeys.Core.Settings;

/// <summary>
/// Per-user settings persistence. Implementations live in the API layer (file, DB, etc.);
/// the contract stays in Core so business logic and tests can depend on it without ASP.NET.
/// </summary>
public interface IUserSettingsStore
{
    UserSettings Get(string? userId);

    UserSettings Save(string? userId, UserSettings settings);

    UserSettings Reset(string? userId);
}

using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using PptPowerKeys.Windows.Settings;
using Xunit;

namespace PptPowerKeys.Tests;

public class CommandRouterRoutabilityTests
{
    [Fact]
    public void Execute_all_catalog_commands_does_not_throw_NotSupportedException()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pptpowerkeys-router", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            var taskPane = new RecordingTaskPaneService();
            var router = new CommandRouter(new StubComHostAdapter(), store, taskPane);
            var notSupported = new List<CommandIds>();

            foreach (var descriptor in CommandCatalog.All)
            {
                try
                {
                    router.Execute(descriptor.Id);
                }
                catch (NotSupportedException)
                {
                    notSupported.Add(descriptor.Id);
                }
                catch (InvalidOperationException)
                {
                    // Expected for host-script commands without a real selection.
                }
            }

            Assert.Empty(notSupported);
            Assert.Equal(CommandCatalog.All.Count, CommandCatalog.All.Count); // 79 commands in catalog
            Assert.Equal(79, CommandCatalog.All.Count);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Execute_settings_commands_delegates_to_task_pane_service()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pptpowerkeys-router", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new WindowsUserSettingsStore(tempDir);
            var taskPane = new RecordingTaskPaneService();
            var router = new CommandRouter(new StubComHostAdapter(), store, taskPane);

            router.Execute(CommandIds.OpenShortcutManager);
            router.Execute(CommandIds.OpenColorScheme);
            router.Execute(CommandIds.ResetToDefaults);

            Assert.Contains(nameof(ITaskPaneService.ShowSettingsScrollToShortcuts), taskPane.Calls);
            Assert.Contains(nameof(ITaskPaneService.ShowColorsPlaceholder), taskPane.Calls);
            Assert.Contains(nameof(ITaskPaneService.ReloadFromStore), taskPane.Calls);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class RecordingTaskPaneService : ITaskPaneService
    {
        public List<string> Calls { get; } = new();

        public void ShowSettings() => Calls.Add(nameof(ShowSettings));

        public void ShowSettingsScrollToShortcuts() => Calls.Add(nameof(ShowSettingsScrollToShortcuts));

        public void ShowColorsPlaceholder() => Calls.Add(nameof(ShowColorsPlaceholder));

        public void ReloadFromStore() => Calls.Add(nameof(ReloadFromStore));
    }
}

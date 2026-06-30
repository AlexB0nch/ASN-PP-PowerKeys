using System;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Layout;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Routes <see cref="CommandIds"/> to in-process Core (ServerLayout) or future host scripts.
    /// S08-001: all 32 <see cref="LayoutEngine.IsLayoutCommand"/> ids via <see cref="ExecuteServerLayout"/>.
    /// S08-002: passes <see cref="LayoutOptions.SnapToGrid"/> from <see cref="WindowsUserSettingsStore"/>.
    /// </summary>
    public sealed class CommandRouter
    {
        private readonly IComHostAdapter _host;
        private readonly WindowsUserSettingsStore _settingsStore;

        public CommandRouter(IComHostAdapter host, WindowsUserSettingsStore settingsStore)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        public LayoutResult Execute(CommandIds command)
        {
            if (!LayoutEngine.IsLayoutCommand(command))
            {
                throw new NotSupportedException(
                    $"Command '{command}' is not implemented in PptPowerKeys.Windows yet.");
            }

            return ExecuteServerLayout(command);
        }

        private LayoutOptions GetLayoutOptions() =>
            new LayoutOptions { SnapToGrid = _settingsStore.Current.SnapToGrid };

        private LayoutResult ExecuteServerLayout(CommandIds command)
        {
            var shapes = _host.ReadSelectedShapeBounds();
            var request = new LayoutRequest
            {
                Command = command,
                Shapes = shapes,
                Options = GetLayoutOptions(),
            };

            var result = LayoutEngine.Apply(request);
            if (result.Changed)
            {
                _host.ApplyShapeBounds(result.Shapes);
            }

            return result;
        }
    }
}

using System;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Layout;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Routes <see cref="CommandIds"/> to in-process Core (ServerLayout) or future host scripts.
    /// S08-001: all 32 <see cref="LayoutEngine.IsLayoutCommand"/> ids via <see cref="ExecuteServerLayout"/>.
    /// </summary>
    public sealed class CommandRouter
    {
        private readonly IComHostAdapter _host;

        public CommandRouter(IComHostAdapter host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
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

        private LayoutResult ExecuteServerLayout(CommandIds command)
        {
            var shapes = _host.ReadSelectedShapeBounds();
            var request = new LayoutRequest
            {
                Command = command,
                Shapes = shapes,
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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Office.Interop.PowerPoint;
using PptPowerKeys.Commands;

namespace PptPowerKeys.Core
{
    /// <summary>
    /// Routes commands to category handlers. Full registration — S01-003.
    /// </summary>
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Application _application;
        private readonly Dictionary<CommandIds, ICommand> _commands = new Dictionary<CommandIds, ICommand>();

        public CommandDispatcher(Application application)
        {
            _application = application;
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            // Command registration will be added in S01-003 / S01-004+.
        }

        public void Dispatch(CommandIds commandId)
        {
            var context = new CommandContext(_application);
            if (!context.HasSelection)
            {
                Debug.WriteLine($"Command {commandId} skipped: no shape selection.");
                return;
            }

            if (!_commands.TryGetValue(commandId, out ICommand command))
            {
                Debug.WriteLine($"Command {commandId} is not registered yet.");
                return;
            }

            command.Execute(context);
        }
    }
}

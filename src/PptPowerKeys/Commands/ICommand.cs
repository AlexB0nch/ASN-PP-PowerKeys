namespace PptPowerKeys.Commands
{
    public interface ICommand
    {
        CommandIds Id { get; }

        void Execute(Core.CommandContext ctx);
    }
}

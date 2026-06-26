namespace PptPowerKeys.Core
{
    public interface ICommandDispatcher
    {
        void Dispatch(Commands.CommandIds commandId);
    }
}

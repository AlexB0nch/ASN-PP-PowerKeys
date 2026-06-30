using PptPowerKeys.Core.Layout;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Outcome of <see cref="CommandRouter.Execute"/> for layout and host-script commands.
    /// </summary>
    public sealed record CommandExecutionResult
    {
        public required bool Changed { get; init; }

        public required string Message { get; init; }

        public static CommandExecutionResult FromLayoutResult(LayoutResult result) =>
            new()
            {
                Changed = result.Changed,
                Message = result.Message
                    ?? (result.Changed ? "Layout applied." : "Nothing to change."),
            };
    }
}

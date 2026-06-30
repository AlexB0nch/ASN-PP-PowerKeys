using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class DuplicateGapStoreTests
{
    public DuplicateGapStoreTests()
    {
        DuplicateGapStore.Clear();
    }

    [Fact]
    public void GetGap_returns_zero_when_not_set()
    {
        Assert.Equal(0, DuplicateGapStore.GetGap(CommandIds.DuplicateRight));
    }

    [Fact]
    public void SetGap_and_GetGap_round_trip_per_command()
    {
        DuplicateGapStore.SetGap(CommandIds.DuplicateRight, 12);
        DuplicateGapStore.SetGap(CommandIds.DuplicateDown, 5);

        Assert.Equal(12, DuplicateGapStore.GetGap(CommandIds.DuplicateRight));
        Assert.Equal(5, DuplicateGapStore.GetGap(CommandIds.DuplicateDown));
        Assert.Equal(0, DuplicateGapStore.GetGap(CommandIds.DuplicateLeft));
    }

    [Fact]
    public void SetGap_clamps_negative_to_zero()
    {
        DuplicateGapStore.SetGap(CommandIds.DuplicateUp, -3);
        Assert.Equal(0, DuplicateGapStore.GetGap(CommandIds.DuplicateUp));
    }

    [Fact]
    public void Clear_removes_all_gaps()
    {
        DuplicateGapStore.SetGap(CommandIds.DuplicateRight, 8);
        DuplicateGapStore.Clear();
        Assert.Equal(0, DuplicateGapStore.GetGap(CommandIds.DuplicateRight));
    }
}

using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class PositionClipboardStoreTests
{
    public PositionClipboardStoreTests()
    {
        PositionClipboardStore.Clear();
    }

    [Fact]
    public void Get_returns_null_when_empty()
    {
        Assert.Null(PositionClipboardStore.Get());
    }

    [Fact]
    public void Set_and_Get_round_trip()
    {
        PositionClipboardStore.Set(12.5, 34.7);

        var snapshot = PositionClipboardStore.Get();
        Assert.NotNull(snapshot);
        Assert.Equal(12.5, snapshot.Value.Left);
        Assert.Equal(34.7, snapshot.Value.Top);
    }

    [Fact]
    public void Set_overwrites_previous_snapshot()
    {
        PositionClipboardStore.Set(1, 2);
        PositionClipboardStore.Set(100, 200);

        var snapshot = PositionClipboardStore.Get();
        Assert.NotNull(snapshot);
        Assert.Equal(100, snapshot.Value.Left);
        Assert.Equal(200, snapshot.Value.Top);
    }

    [Fact]
    public void Clear_removes_snapshot()
    {
        PositionClipboardStore.Set(10, 20);
        PositionClipboardStore.Clear();

        Assert.Null(PositionClipboardStore.Get());
    }
}

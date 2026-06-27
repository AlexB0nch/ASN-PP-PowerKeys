using PptPowerKeys.Commands;
using Xunit;

namespace PptPowerKeys.Tests
{
    public class CommandIdsTests
    {
        [Fact]
        public void AlignLeft_IsDefined()
        {
            Assert.Equal("AlignLeft", CommandIds.AlignLeft.ToString());
        }

        [Fact]
        public void CommandIds_HasAlignmentAndResizeValues()
        {
            Assert.True((int)CommandIds.AlignLeft < (int)CommandIds.SameWidth);
            Assert.True((int)CommandIds.SameWidth < (int)CommandIds.InsertTextbox);
        }
    }
}

using System.Collections.Generic;

namespace PptPowerKeys.Core
{
    public interface IColorSchemeReader
    {
        IReadOnlyList<System.Drawing.Color> GetThemeColors();

        IReadOnlyList<System.Drawing.Color> GetRecentColors();
    }
}

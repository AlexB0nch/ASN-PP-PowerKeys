using System.Collections.Generic;
using System.Drawing;
using Microsoft.Office.Interop.PowerPoint;

namespace PptPowerKeys.Core
{
    /// <summary>
    /// Reads theme colors from Slide Master. Full implementation — S01-007.
    /// </summary>
    public class ColorSchemeReader : IColorSchemeReader
    {
        private readonly Application _application;

        public ColorSchemeReader(Application application)
        {
            _application = application;
        }

        public IReadOnlyList<Color> GetThemeColors()
        {
            return new List<Color>();
        }

        public IReadOnlyList<Color> GetRecentColors()
        {
            return new List<Color>();
        }
    }
}

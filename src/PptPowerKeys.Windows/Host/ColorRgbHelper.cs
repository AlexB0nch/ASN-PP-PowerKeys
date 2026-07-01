using System;
using System.Globalization;
using PptPowerKeys.Core.Colors;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Converts between Core hex colors and Office OLE RGB (BGR integer).
    /// Pure helpers testable without PowerPoint.
    /// </summary>
    public static class ColorRgbHelper
    {
        private const int NearBlackThreshold = 48;

        /// <summary>Converts <c>#RRGGBB</c> to Office OLE RGB (BGR).</summary>
        public static int HexToOleRgb(string hex)
        {
            string normalized = ThemeColor.NormalizeHex(hex);
            int r = int.Parse(normalized.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int g = int.Parse(normalized.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int b = int.Parse(normalized.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return r | (g << 8) | (b << 16);
        }

        /// <summary>Converts Office OLE RGB (BGR) to uppercase <c>#RRGGBB</c>.</summary>
        public static string OleRgbToHex(int oleRgb)
        {
            int r = oleRgb & 0xFF;
            int g = (oleRgb >> 8) & 0xFF;
            int b = (oleRgb >> 16) & 0xFF;
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>True when all RGB channels are below 48 (parity with Web <c>isNearBlack</c>).</summary>
        public static bool IsNearBlackHex(string hex)
        {
            if (!ThemeColor.IsValidHex(hex))
            {
                return false;
            }

            string normalized = ThemeColor.NormalizeHex(hex);
            int r = int.Parse(normalized.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int g = int.Parse(normalized.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            int b = int.Parse(normalized.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return r < NearBlackThreshold && g < NearBlackThreshold && b < NearBlackThreshold;
        }

        /// <summary>True when OLE RGB channels are below 48.</summary>
        public static bool IsNearBlackOle(int oleRgb) => IsNearBlackHex(OleRgbToHex(oleRgb));
    }
}

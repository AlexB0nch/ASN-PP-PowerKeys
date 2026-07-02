using System;
using System.Collections.Generic;
using PptPowerKeys.Core.Colors;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Shared palette building for format-color cycle commands and the color picker pane.
    /// Avoids duplicating <see cref="ColorPaletteBuilder"/> wiring.
    /// </summary>
    public sealed class FormatColorPaletteProvider
    {
        private readonly IComHostAdapter _host;
        private readonly WindowsUserSettingsStore _settingsStore;

        public FormatColorPaletteProvider(IComHostAdapter host, WindowsUserSettingsStore settingsStore)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        /// <summary>
        /// True when the presentation theme is unavailable and the fallback palette is used.
        /// </summary>
        public bool UsesFallbackTheme
        {
            get
            {
                var theme = _host.ReadPresentationThemeColors();
                return TakeValidColors(theme, ColorPaletteBuilder.MaxThemeColors).Count == 0;
            }
        }

        /// <summary>
        /// Theme swatches for the picker UI (up to 10). Uses fallback when COM theme is empty.
        /// </summary>
        public IReadOnlyList<string> GetThemeSwatches()
        {
            var theme = TakeValidColors(
                _host.ReadPresentationThemeColors(),
                ColorPaletteBuilder.MaxThemeColors);
            if (theme.Count == 0)
            {
                return TakeValidColors(DefaultColorPalette.FallbackTheme, ColorPaletteBuilder.MaxThemeColors);
            }

            return theme;
        }

        /// <summary>Recent colors from user settings (newest first, normalized).</summary>
        public IReadOnlyList<string> GetRecentSwatches()
        {
            var recent = _settingsStore.GetRecentColors();
            var result = new List<string>(Math.Min(recent.Count, ColorPaletteBuilder.MaxRecentColors));
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in recent)
            {
                if (result.Count >= ColorPaletteBuilder.MaxRecentColors)
                {
                    break;
                }

                if (!TryNormalize(raw, out var hex))
                {
                    continue;
                }

                if (!seen.Add(hex))
                {
                    continue;
                }

                result.Add(hex);
            }

            return result;
        }

        /// <summary>
        /// Merged cycling palette for Fill/Line/Text ribbon commands.
        /// </summary>
        public IReadOnlyList<string> BuildCyclingPalette()
        {
            var theme = _host.ReadPresentationThemeColors();
            var recent = _settingsStore.GetRecentColors();
            return ColorPaletteBuilder.Build(
                theme,
                recent,
                DefaultColorPalette.FallbackTheme);
        }

        private static List<string> TakeValidColors(IReadOnlyList<string>? colors, int max)
        {
            var result = new List<string>();
            if (colors is null)
            {
                return result;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in colors)
            {
                if (result.Count >= max)
                {
                    break;
                }

                if (!TryNormalize(raw, out var hex))
                {
                    continue;
                }

                if (!seen.Add(hex))
                {
                    continue;
                }

                result.Add(hex);
            }

            return result;
        }

        private static bool TryNormalize(string raw, out string hex)
        {
            hex = string.Empty;
            if (!ThemeColor.IsValidHex(raw))
            {
                return false;
            }

            try
            {
                hex = ThemeColor.NormalizeHex(raw);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}

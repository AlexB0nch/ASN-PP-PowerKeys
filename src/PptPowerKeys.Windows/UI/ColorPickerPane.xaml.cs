using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PptPowerKeys.Core.Colors;
using PptPowerKeys.Windows.Host;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.UI
{
    public partial class ColorPickerPane : UserControl
    {
        private readonly IComHostAdapter _host;
        private readonly WindowsUserSettingsStore _settingsStore;
        private readonly FormatColorPaletteProvider _paletteProvider;
        private string? _selectedColor;
        private bool _suppressHexChange;

        public ColorPickerPane(
            IComHostAdapter host,
            WindowsUserSettingsStore settingsStore,
            FormatColorPaletteProvider paletteProvider)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
            _paletteProvider = paletteProvider ?? throw new ArgumentNullException(nameof(paletteProvider));

            InitializeComponent();
            HexInput.TextChanged += OnHexInputTextChanged;
            ReloadPalette();
        }

        public void FocusPicker()
        {
            RootScroller.BringIntoView();
            Focus();
        }

        public void ReloadPalette()
        {
            FallbackWarning.Visibility = _paletteProvider.UsesFallbackTheme
                ? Visibility.Visible
                : Visibility.Collapsed;

            var themeColors = _paletteProvider.GetThemeSwatches();
            PopulateSwatches(ThemeSwatches, themeColors);

            var recentColors = _paletteProvider.GetRecentSwatches();
            if (recentColors.Count > 0)
            {
                RecentSection.Visibility = Visibility.Visible;
                PopulateSwatches(RecentSwatches, recentColors);
            }
            else
            {
                RecentSection.Visibility = Visibility.Collapsed;
                RecentSwatches.Children.Clear();
            }

            if (_selectedColor == null && themeColors.Count > 0)
            {
                SelectColor(themeColors[0], updateHex: true);
            }
            else if (_selectedColor != null)
            {
                UpdatePreview(_selectedColor);
            }
        }

        private void PopulateSwatches(UniformGrid grid, IReadOnlyList<string> colors)
        {
            grid.Children.Clear();
            foreach (var color in colors)
            {
                grid.Children.Add(CreateSwatchButton(color));
            }
        }

        private Button CreateSwatchButton(string color)
        {
            string normalized = NormalizeOrDefault(color);
            var button = new Button
            {
                Width = 36,
                Height = 36,
                Margin = new Thickness(2),
                Padding = new Thickness(0),
                Background = CreateBrush(normalized),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                ToolTip = normalized,
                Tag = normalized,
            };

            if (_selectedColor != null
                && string.Equals(normalized, _selectedColor, StringComparison.OrdinalIgnoreCase))
            {
                button.BorderBrush = Brushes.DodgerBlue;
                button.BorderThickness = new Thickness(2);
            }

            button.Click += OnSwatchClick;
            return button;
        }

        private void OnSwatchClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string color)
            {
                SelectColor(color, updateHex: true);
                ReloadPalette();
            }
        }

        private void SelectColor(string color, bool updateHex)
        {
            _selectedColor = NormalizeOrDefault(color);
            UpdatePreview(_selectedColor);

            if (updateHex)
            {
                _suppressHexChange = true;
                try
                {
                    HexInput.Text = _selectedColor;
                    HexError.Visibility = Visibility.Collapsed;
                }
                finally
                {
                    _suppressHexChange = false;
                }
            }
        }

        private void UpdatePreview(string color)
        {
            PreviewPanel.Visibility = Visibility.Visible;
            PreviewSwatch.Background = CreateBrush(color);
            PreviewHex.Text = color;
        }

        private static SolidColorBrush CreateBrush(string hex)
        {
            try
            {
                string digits = hex.TrimStart('#');
                byte r = Convert.ToByte(digits.Substring(0, 2), 16);
                byte g = Convert.ToByte(digits.Substring(2, 2), 16);
                byte b = Convert.ToByte(digits.Substring(4, 2), 16);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        private void OnHexInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressHexChange)
            {
                return;
            }

            string trimmed = HexInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmed))
            {
                HexError.Visibility = Visibility.Collapsed;
                return;
            }

            if (ThemeColor.IsValidHex(trimmed))
            {
                HexError.Visibility = Visibility.Collapsed;
                SelectColor(ThemeColor.NormalizeHex(trimmed), updateHex: false);
                ReloadPalette();
                return;
            }

            HexError.Text = "Invalid HEX. Use #RRGGBB or RRGGBB (6 hex digits).";
            HexError.Visibility = Visibility.Visible;
        }

        private void OnHexInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ApplyHexInput();
            }
        }

        private void OnHexSetClick(object sender, RoutedEventArgs e) => ApplyHexInput();

        private void ApplyHexInput()
        {
            string trimmed = HexInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmed))
            {
                HexError.Text = "Enter a HEX color (#RRGGBB or RRGGBB).";
                HexError.Visibility = Visibility.Visible;
                return;
            }

            if (!ThemeColor.IsValidHex(trimmed))
            {
                HexError.Text = "Invalid HEX. Use #RRGGBB or RRGGBB (6 hex digits).";
                HexError.Visibility = Visibility.Visible;
                return;
            }

            HexError.Visibility = Visibility.Collapsed;
            SelectColor(ThemeColor.NormalizeHex(trimmed), updateHex: true);
            ReloadPalette();
        }

        private void OnPickFillClick(object sender, RoutedEventArgs e) =>
            PickFromShape(ColorPickSource.Fill);

        private void OnPickLineClick(object sender, RoutedEventArgs e) =>
            PickFromShape(ColorPickSource.Line);

        private void OnPickTextClick(object sender, RoutedEventArgs e) =>
            PickFromShape(ColorPickSource.Text);

        private void PickFromShape(ColorPickSource source)
        {
            try
            {
                string color = _host.ReadColorFromSelection(source);
                SelectColor(color, updateHex: true);
                _settingsStore.RecordRecentColor(color);
                ReloadPalette();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnApplyFillClick(object sender, RoutedEventArgs e) => ApplyColor(applyFill: true);

        private void OnApplyLineClick(object sender, RoutedEventArgs e) => ApplyColor(applyLine: true);

        private void OnApplyTextClick(object sender, RoutedEventArgs e) => ApplyColor(applyText: true);

        private void ApplyColor(bool applyFill = false, bool applyLine = false, bool applyText = false)
        {
            if (string.IsNullOrEmpty(_selectedColor))
            {
                MessageBox.Show("Select a color first.", "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string normalized = NormalizeOrDefault(_selectedColor);

            try
            {
                int count;
                string label;
                if (applyFill)
                {
                    count = _host.ApplyFillColor(normalized);
                    label = "Fill";
                }
                else if (applyLine)
                {
                    count = _host.ApplyLineColor(normalized);
                    label = "Line";
                }
                else
                {
                    count = _host.ApplyTextColor(normalized);
                    label = "Text";
                }

                _settingsStore.RecordRecentColor(normalized);
                ReloadPalette();
                MessageBox.Show(
                    $"{label} color {normalized} applied to {count} shape(s).",
                    "PPT PowerKeys",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string NormalizeOrDefault(string color)
        {
            if (ThemeColor.IsValidHex(color))
            {
                return ThemeColor.NormalizeHex(color);
            }

            return "#000000";
        }
    }
}

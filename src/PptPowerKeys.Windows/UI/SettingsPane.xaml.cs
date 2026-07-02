using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Settings;
using PptPowerKeys.Core.Text;
using PptPowerKeys.Windows.Host;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.UI
{
    public partial class SettingsPane : UserControl
    {
        private const int SettingsSchemaVersion = 1;

        private readonly WindowsUserSettingsStore _store;
        private readonly Action _onSettingsSaved;
        private readonly ObservableCollection<ShortcutRow> _shortcutRows = new();
        private readonly List<CommandDescriptor> _commands;
        private bool _suppressProfileChange;

        public SettingsPane(
            WindowsUserSettingsStore store,
            IComHostAdapter host,
            FormatColorPaletteProvider paletteProvider,
            Action onSettingsSaved)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _onSettingsSaved = onSettingsSaved ?? throw new ArgumentNullException(nameof(onSettingsSaved));
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (paletteProvider is null)
            {
                throw new ArgumentNullException(nameof(paletteProvider));
            }

            _commands = CommandCatalog.All
                .OrderBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            InitializeComponent();

            ColorPicker = new ColorPickerPane(host, store, paletteProvider);
            ColorsTab.Content = ColorPicker;

            ProfileCombo.ItemsSource = ConsultingProfilePresets.KnownProfiles;
            AddupModeCombo.ItemsSource = BuildAddupModeOptions();
            NewCommandCombo.ItemsSource = _commands;
            ShortcutsGrid.ItemsSource = _shortcutRows;

            ReloadFromStore();
        }

        public void ReloadFromStore()
        {
            var settings = _store.Current;
            _suppressProfileChange = true;
            try
            {
                ProfileCombo.SelectedItem = NormalizeProfile(settings.Profile);
                SnapToGridCheck.IsChecked = settings.SnapToGrid;
                SelectAddupMode(settings.AddupDisplayMode);
                LoadShortcuts(settings.Shortcuts);
            }
            finally
            {
                _suppressProfileChange = false;
            }

            PresetWarning.Visibility = Visibility.Collapsed;
            ImportWarning.Visibility = Visibility.Collapsed;
        }

        public void SelectGeneralTab()
        {
            MainTabs.SelectedIndex = 0;
        }

        public void SelectColorsTab()
        {
            MainTabs.SelectedItem = ColorsTab;
        }

        public void FocusColorPicker()
        {
            SelectColorsTab();
            ColorPicker.FocusPicker();
        }

        public void ReloadColorPicker()
        {
            ColorPicker.ReloadPalette();
        }

        private ColorPickerPane ColorPicker { get; set; } = null!;

        public void ScrollToShortcuts()
        {
            SelectGeneralTab();
            ShortcutsHeader.BringIntoView();
            ShortcutsGrid.BringIntoView();
        }

        private static string NormalizeProfile(string? profile)
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                return ConsultingProfilePresets.Custom;
            }

            return ConsultingProfilePresets.IsKnownProfile(profile)
                ? profile
                : ConsultingProfilePresets.Custom;
        }

        private void LoadShortcuts(IReadOnlyList<ShortcutBinding>? shortcuts)
        {
            _shortcutRows.Clear();
            if (shortcuts is null)
            {
                return;
            }

            foreach (var binding in shortcuts)
            {
                _shortcutRows.Add(CreateRow(binding.CommandId, binding.Keys));
            }
        }

        private ShortcutRow CreateRow(string commandId, string keys)
        {
            var descriptor = CommandCatalog.Find(commandId);
            return new ShortcutRow
            {
                CommandId = commandId,
                CommandTitle = descriptor?.Title ?? commandId,
                Keys = keys ?? string.Empty,
            };
        }

        private static IReadOnlyList<AddupModeOption> BuildAddupModeOptions() =>
            new[]
            {
                new AddupModeOption(AddupStatusFormatter.ModeAll, "All metrics"),
                new AddupModeOption(AddupStatusFormatter.ModeSum, "Sum only"),
                new AddupModeOption(AddupStatusFormatter.ModeMin, "Min only"),
                new AddupModeOption(AddupStatusFormatter.ModeMax, "Max only"),
                new AddupModeOption(AddupStatusFormatter.ModeAverage, "Average only"),
            };

        private void SelectAddupMode(string? mode)
        {
            var normalized = AddupStatusFormatter.NormalizeMode(mode);
            AddupModeCombo.SelectedItem = BuildAddupModeOptions()
                .First(o => o.Value == normalized);
        }

        private UserSettings BuildSettingsFromUi()
        {
            var profile = ProfileCombo.SelectedItem as string ?? ConsultingProfilePresets.Custom;
            var addupOption = AddupModeCombo.SelectedItem as AddupModeOption;
            return new UserSettings
            {
                Profile = profile,
                SnapToGrid = SnapToGridCheck.IsChecked == true,
                AddupDisplayMode = addupOption?.Value ?? UserSettings.AddupDisplayModeDefault,
                Shortcuts = _shortcutRows
                    .Where(r => !string.IsNullOrWhiteSpace(r.CommandId) && !string.IsNullOrWhiteSpace(r.Keys))
                    .Select(r => new ShortcutBinding { CommandId = r.CommandId, Keys = r.Keys.Trim() })
                    .ToList(),
            };
        }

        private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressProfileChange)
            {
                return;
            }

            var profile = ProfileCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(profile) || profile == ConsultingProfilePresets.Custom)
            {
                PresetWarning.Visibility = Visibility.Collapsed;
                return;
            }

            if (!ConsultingProfilePresets.IsKnownProfile(profile))
            {
                return;
            }

            var presetShortcuts = ConsultingProfilePresets.GetShortcuts(profile);
            LoadShortcuts(presetShortcuts.ToList());
            PresetWarning.Visibility = Visibility.Visible;
            ImportWarning.Visibility = Visibility.Collapsed;
        }

        private void OnAddupModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // UI-only until Save.
        }

        private void OnDeleteShortcutClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ShortcutRow row)
            {
                _shortcutRows.Remove(row);
            }
        }

        private void OnAddShortcutClick(object sender, RoutedEventArgs e)
        {
            if (NewCommandCombo.SelectedItem is not CommandDescriptor command)
            {
                MessageBox.Show("Select a command.", "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var keys = NewKeysBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keys))
            {
                MessageBox.Show("Enter shortcut keys.", "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _shortcutRows.FirstOrDefault(r => r.CommandId == command.Key);
            if (existing != null)
            {
                existing.Keys = keys;
            }
            else
            {
                _shortcutRows.Add(CreateRow(command.Key, keys));
            }

            NewKeysBox.Clear();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _store.Save(BuildSettingsFromUi());
                PresetWarning.Visibility = Visibility.Collapsed;
                ImportWarning.Visibility = Visibility.Collapsed;
                _onSettingsSaved();
                MessageBox.Show("Settings saved.", "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _store.Reset();
                ReloadFromStore();
                _onSettingsSaved();
                MessageBox.Show("Settings reset to defaults.", "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            var settings = BuildSettingsFromUi();
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = BuildExportFilename(settings.Profile),
                Title = "Export settings",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var payload = new Dictionary<string, object?>
            {
                ["schemaVersion"] = SettingsSchemaVersion,
                ["profile"] = settings.Profile,
                ["snapToGrid"] = settings.SnapToGrid,
                ["addupDisplayMode"] = settings.AddupDisplayMode,
                ["shortcuts"] = settings.Shortcuts.Select(s => new { commandId = s.CommandId, keys = s.Keys }),
            };

            var json = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);
        }

        private void OnImportClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Import settings",
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(dialog.FileName);
                var result = _store.Import(json);
                if (result.Settings is null)
                {
                    MessageBox.Show(
                        result.Error ?? "Invalid JSON.",
                        "PPT PowerKeys",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                _suppressProfileChange = true;
                try
                {
                    ProfileCombo.SelectedItem = NormalizeProfile(result.Settings.Profile);
                    SnapToGridCheck.IsChecked = result.Settings.SnapToGrid;
                    SelectAddupMode(result.Settings.AddupDisplayMode);
                    LoadShortcuts(result.Settings.Shortcuts);
                }
                finally
                {
                    _suppressProfileChange = false;
                }

                PresetWarning.Visibility = Visibility.Collapsed;
                ImportWarning.Visibility = Visibility.Visible;
                ImportWarning.Text = result.Warnings.Count > 0
                    ? "Imported — click Save to persist. " + string.Join(" ", result.Warnings)
                    : "Imported — click Save to persist.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PPT PowerKeys", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string BuildExportFilename(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile) || profile == ConsultingProfilePresets.Custom)
            {
                return "ppt-powerkeys-settings.json";
            }

            var sanitized = new string(profile
                .Trim()
                .Select(c => char.IsLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '-')
                .ToArray())
                .Trim('-');

            return string.IsNullOrEmpty(sanitized)
                ? "ppt-powerkeys-settings.json"
                : $"ppt-powerkeys-settings-{sanitized}.json";
        }

        private sealed class AddupModeOption
        {
            public AddupModeOption(string value, string label)
            {
                Value = value;
                Label = label;
            }

            public string Value { get; }

            public string Label { get; }

            public override string ToString() => Label;
        }

        private sealed class ShortcutRow : INotifyPropertyChanged
        {
            private string _keys = string.Empty;

            public string CommandId { get; set; } = string.Empty;

            public string CommandTitle { get; set; } = string.Empty;

            public string Keys
            {
                get => _keys;
                set
                {
                    if (_keys != value)
                    {
                        _keys = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

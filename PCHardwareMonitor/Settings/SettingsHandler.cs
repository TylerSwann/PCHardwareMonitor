﻿
using System.IO;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;


namespace PCHardwareMonitor
{
    public enum SettingsOption { Hardware = 0, WindowBackgroundColor, BarBackgroundColor, BarForegroundColor, BorderColor, Font, Position, Reset }
    public enum LayoutPosition { TopRight, TopLeft, BottomRight, BottomLeft, Center }

    class SettingsHandler: IDisposable
    {
        private string[] hardwareVitalTitles = new string[] { "GPU Temp", "GPU Fan RPM", "GPU Usage", "GPU Memory Usage", "CPU Usage", "RAM Usage", "Harddrive Space"};
        private string[] positionTitles = new string[] { "Top Right", "Top Left", "Bottom Right", "Bottom Left", "Center" };
        private List<string> fontFamilies = new List<string>();
        private List<Vital> vitalsToMonitor = new List<Vital>();
        private List<CheckBox> checkboxes = new List<CheckBox>();
        private List<RadioButton> radioButtons = new List<RadioButton>();
        private StackPanel hardwareMenu = new StackPanel();
        private CheckBox rowsCheckBox = new CheckBox();
        private ComboBox fontbox = new ComboBox();
        private Slider fontSizeSlider = new Slider();
        private Label fontSizeLabel = new Label();
        private UserSettings settings;
        private StackPanel positionMenu;
        private SettingsPanel settingsPanel;
        private SettingsOption currentSetting;
        public SettingsDelegate Delegate;

        public SettingsHandler(SettingsPanel settingsPanel)
        {
            CheckForSettingsFile();
            this.settingsPanel = settingsPanel;
            settingsPanel.didClickButtonAtIndex = (j) => { CurrentSettingDidChange(j); };
            hardwareMenu.Width = 290.0;
            hardwareMenu.Height = 380.0;
            hardwareMenu.Background = new SolidColorBrush(Color.FromArgb((byte)0, (byte)70, (byte)85, (byte)90));
            hardwareMenu.Margin = new Thickness(0, 0, 30, 130);
            hardwareMenu.HorizontalAlignment = HorizontalAlignment.Right;
            hardwareMenu.VerticalAlignment = VerticalAlignment.Center;
            positionMenu = new StackPanel();
            positionMenu.Width = 290.0;
            positionMenu.Height = 380.0;
            positionMenu.Visibility = Visibility.Hidden;
            positionMenu.HorizontalAlignment = HorizontalAlignment.Right;
            positionMenu.VerticalAlignment = VerticalAlignment.Center;
            positionMenu.Margin = new Thickness(100, 0, 0, 50);
            for (int i = 0; i < positionTitles.Length; i++)
            {
                var radioButton = new RadioButton();
                var index = i;
                radioButton.Width = 100.0;
                radioButton.Height = 30.0;
                radioButton.Content = positionTitles[i];
                radioButton.HorizontalAlignment = HorizontalAlignment.Center;
                radioButton.VerticalAlignment = VerticalAlignment.Center;
                radioButton.Margin = new Thickness(0, 0, 30, 0);
                radioButton.LayoutTransform = new ScaleTransform(1.6, 1.6);
                radioButton.Click += (object sender, RoutedEventArgs e) => { NewPositionWasSelected((LayoutPosition)index, rowsCheckBox.IsChecked ?? false); };
                radioButtons.Add(radioButton);
                positionMenu.Children.Add(radioButton);
            }
            rowsCheckBox.Content = "Layout in rows";
            rowsCheckBox.Width = 180.0;
            rowsCheckBox.Height = 30.0;
            rowsCheckBox.LayoutTransform = new ScaleTransform(1.4, 1.4);
            rowsCheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            rowsCheckBox.Foreground = new SolidColorBrush(Color.FromRgb((byte)200, (byte)200, (byte)200));
            rowsCheckBox.Click += (object sender, RoutedEventArgs e) => { NewPositionWasSelected(settings.startupPosition, rowsCheckBox.IsChecked ?? false); };
            rowsCheckBox.IsChecked = settings.layoutIsInRows;
            positionMenu.Children.Add(rowsCheckBox);
            settingsPanel.Children.Add(positionMenu);
            switch (settings.startupPosition)
            {
                case (LayoutPosition)0: radioButtons[0].IsChecked = true; break;
                case (LayoutPosition)1: radioButtons[1].IsChecked = true; break;
                case (LayoutPosition)2: radioButtons[2].IsChecked = true; break;
                case (LayoutPosition)3: radioButtons[3].IsChecked = true; break;
                case (LayoutPosition)4: radioButtons[4].IsChecked = true; break;
            }
            settingsPanel.rightPanel.Children.Add(hardwareMenu);
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            settingsPanel.SetButtonSelected(0, true);
            settingsPanel.didSelectedNewColor = (color) => { NewColorWasSelected(color); };
            foreach (Vital vital in settings.startupVitals) { vitalsToMonitor.Add(vital); }
            bool hasReachedSegoe = false;
            foreach (var font in Fonts.SystemFontFamilies)
            {
                fontFamilies.Add(font.ToString());
                if (font.ToString().Contains("Segoe") && hasReachedSegoe == false)
                {
                    fontFamilies.Add("Segoe UI Black");
                    fontFamilies.Add("Segoe UI Light");
                    fontFamilies.Add("Segoe UI Semibold");
                    fontFamilies.Add("Segoe UI Semilight");
                    hasReachedSegoe = true;
                }
            }

            fontbox.Width = 120.0;
            fontbox.Height = 22.0;
            fontbox.ItemsSource = fontFamilies;
            fontbox.SelectedIndex = fontFamilies.IndexOf(settings.font.fontFamily);
            fontbox.Visibility = Visibility.Hidden;
            fontbox.HorizontalAlignment = HorizontalAlignment.Center;
            fontbox.VerticalAlignment = VerticalAlignment.Center;
            fontbox.Margin = new Thickness(250, 120, 0, 0);
            fontbox.SelectionChanged += (object sender, SelectionChangedEventArgs e) => { settings.font.fontFamily = (string)fontbox.SelectedItem; this.Delegate.DidSelectNewFont(settings.font); };
            fontSizeSlider.Width = 200.0;
            fontSizeSlider.Height = 20.0;
            fontSizeSlider.Maximum = 40.0;
            fontSizeSlider.Value = settings.font.size;
            fontSizeSlider.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) => {
                fontSizeLabel.Content = $"Font Size: {(int)e.NewValue}";
                settings.font.size = (int)e.NewValue;
                this.Delegate.DidSelectNewFont(settings.font);
            };
            fontSizeSlider.Margin = new Thickness(340, 50, 0, 0);
            fontSizeSlider.Visibility = Visibility.Hidden;
            fontSizeLabel.Width = 80.0;
            fontSizeLabel.Height = 35.0;
            fontSizeLabel.Content = "Font Size: 20";
            fontSizeLabel.VerticalAlignment = VerticalAlignment.Center;
            fontSizeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            fontSizeLabel.Margin = new Thickness(60, 60, 0, 0);
            fontSizeLabel.Visibility = Visibility.Hidden;
            settingsPanel.Children.Add(fontSizeLabel);
            settingsPanel.Children.Add(fontSizeSlider);
            settingsPanel.Children.Add(fontbox);

            for (int i = 0; i < hardwareVitalTitles.Length; i++)
            {
                var vital = (Vital)i;
                var checkbox = new CheckBox();
                checkbox.Content = hardwareVitalTitles[i];
                checkbox.Width = 180.0;
                checkbox.Height = 30.0;
                checkbox.LayoutTransform = new ScaleTransform(1.4, 1.4);
                checkbox.HorizontalAlignment = HorizontalAlignment.Right;
                checkbox.Foreground = new SolidColorBrush(Color.FromRgb((byte)200, (byte)200, (byte)200));
                checkboxes.Add(checkbox);
                checkbox.Click += (object sender, RoutedEventArgs e) => {
                    if (checkbox.IsChecked ?? false) { vitalsToMonitor.Add(vital); this.Delegate.DidSelectNewVital(vital, true); }
                    else { if (vitalsToMonitor.Contains(vital)) { vitalsToMonitor.Remove(vital); this.Delegate.DidSelectNewVital(vital, false); } }
                    settings.startupVitals = vitalsToMonitor.ToArray();
                };
                hardwareMenu.Children.Add(checkbox);
            }
            ShowCheckMenu();
        }

        public void SetCheckHidden(int index, bool selected)
        {
            if (index > checkboxes.Count - 1 || index < 0) { System.Console.WriteLine("INDEX OUT OF RANGE"); return; }
            checkboxes[index].IsChecked = selected;
        }

        private void CheckForSettingsFile()
        {

            if(File.Exists($"{AppDirectory.rootDirectory}/user.json"))
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.userSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
            }
            else
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.defaultSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
            }
            if (this.settings == null) { Console.WriteLine("SETTINGS ARE NULL"); }
        }

        private void CurrentSettingDidChange(int index)
        {
            currentSetting = (SettingsOption)index;
            switch (currentSetting)
            {
                case SettingsOption.Hardware: ShowCheckMenu(); break;
                case SettingsOption.WindowBackgroundColor: ShowColorPicker(); break;
                case SettingsOption.BarBackgroundColor: ShowColorPicker(); break;
                case SettingsOption.BarForegroundColor: ShowColorPicker(); break;
                case SettingsOption.BorderColor: ShowColorPicker(); break;
                case SettingsOption.Font: ShowFontMenu(); break;
                case SettingsOption.Position: ShowRadioButtonMenu(); break;
                case SettingsOption.Reset: ConfirmSettingReset(); break;
            }
        }

        private void ConfirmSettingReset()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to reset the colors?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.defaultSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
                this.Delegate.ResetSettings();
            }
        }

        private void NewPositionWasSelected(LayoutPosition position, bool layoutInRows)
        {
            this.Delegate.DidSelectNewPosition(position, layoutInRows);
            settings.startupPosition = position;
            settings.layoutIsInRows = layoutInRows;
        }

        private void NewColorWasSelected(Color? color)
        {
            this.Delegate.DidSelectNewColor(currentSetting, color);
            Color newColor;
            if (color.HasValue) { newColor = color.Value; }
            else { return; }
            switch (currentSetting)
            {
                case SettingsOption.WindowBackgroundColor: settings.windowBackgroundColor = newColor; break;
                case SettingsOption.BarBackgroundColor: settings.barBackgroundColor = newColor; break;
                case SettingsOption.BarForegroundColor: settings.barForegroundColor = newColor; break;
                case SettingsOption.BorderColor: settings.borderColor = newColor; break;
                case SettingsOption.Font: settings.font.textColor = newColor; this.Delegate.DidSelectNewFont(settings.font); break;
                default: break;
            }
        }

        private void ShowColorPicker()
        {
            fontSizeSlider.Visibility = Visibility.Hidden;
            fontbox.Visibility = Visibility.Hidden;
            fontSizeLabel.Visibility = Visibility.Hidden;
            settingsPanel.colorCanvas.Visibility = Visibility.Visible;
            hardwareMenu.Visibility = Visibility.Hidden;
            positionMenu.Visibility = Visibility.Hidden;

            switch (currentSetting)
            {
                case SettingsOption.WindowBackgroundColor: settingsPanel.colorCanvas.SelectedColor = settings.windowBackgroundColor; break;
                case SettingsOption.BarBackgroundColor: settingsPanel.colorCanvas.SelectedColor = settings.barBackgroundColor; break;
                case SettingsOption.BarForegroundColor: settingsPanel.colorCanvas.SelectedColor = settings.barForegroundColor; break;
                case SettingsOption.BorderColor: settingsPanel.colorCanvas.SelectedColor = settings.borderColor; break;
                case SettingsOption.Font: settingsPanel.colorCanvas.SelectedColor = settings.font.textColor; break;
                default: break;
            }
        }

        private void ShowCheckMenu()
        {
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            fontSizeSlider.Visibility = Visibility.Hidden;
            fontbox.Visibility = Visibility.Hidden;
            fontSizeLabel.Visibility = Visibility.Hidden;
            hardwareMenu.Visibility = Visibility.Visible;
            positionMenu.Visibility = Visibility.Hidden;
            for (int i = 0; i < checkboxes.ToArray().Length; i++)
            {
                var checkbox = checkboxes[i];
                if (vitalsToMonitor.Contains((Vital)i)) { checkbox.IsChecked = true; }
            }
        }

        private void ShowRadioButtonMenu()
        {
            switch (settings.startupPosition)
            {
                case (LayoutPosition)0: radioButtons[0].IsChecked = true; break;
                case (LayoutPosition)1: radioButtons[1].IsChecked = true; break;
                case (LayoutPosition)2: radioButtons[2].IsChecked = true; break;
                case (LayoutPosition)3: radioButtons[3].IsChecked = true; break;
                case (LayoutPosition)4: radioButtons[4].IsChecked = true; break;
            }
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            fontSizeSlider.Visibility = Visibility.Hidden;
            fontbox.Visibility = Visibility.Hidden;
            fontSizeLabel.Visibility = Visibility.Hidden;
            hardwareMenu.Visibility = Visibility.Hidden;
            positionMenu.Visibility = Visibility.Visible;
        }

        private void ShowFontMenu()
        {
            ShowColorPicker();
            fontSizeSlider.Visibility = Visibility.Visible;
            fontbox.Visibility = Visibility.Visible;
            fontSizeLabel.Visibility = Visibility.Visible;
            hardwareMenu.Visibility = Visibility.Hidden;
            positionMenu.Visibility = Visibility.Hidden;
        }

        private void SaveUserSettings()
        {
            try
            {
                var rawJson = JsonConvert.SerializeObject(this.settings);
                File.WriteAllText(AppDirectory.userSettings, rawJson);
            }
            catch (System.Exception ex) { System.Console.WriteLine(ex); }
        }

        public void Dispose() { SaveUserSettings(); }
    }
}


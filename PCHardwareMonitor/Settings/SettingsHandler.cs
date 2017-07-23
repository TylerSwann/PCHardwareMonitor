
using System.IO;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;


namespace PCHardwareMonitor
{
    public enum SettingsOption { Hardware = 0, WindowBackgroundColor, BarBackgroundColor, BarForegroundColor, BorderColor, Font, Position, Reset }
    public enum LayoutPosition { TopRight, TopLeft, BottomRight, BottomLeft, Center }

    class SettingsHandler: IDisposable
    {
        private string[] optionTitles = new string[] { "Hardware", "Window Background Color", "Bar Background Color", "Bar Foreground Color", "Border Color", "Font", "Position" };
        private string[] hardwareVitalTitles = new string[] { "GPU Temp", "GPU Fan RPM", "GPU Usage", "GPU Memory Usage", "CPU Usage", "CPU Core Usage", "RAM Usage", "Harddrive Space"};
        private string[] positionTitles = new string[] { "Top Right", "Top Left", "Bottom Right", "Bottom Left", "Center" };
        private List<Vital> vitalsToMonitor = new List<Vital>();
        private List<UICheckBox> checkboxes = new List<UICheckBox>();
        private StackPanel hardwareMenu = new StackPanel();
        private UserSettings settings;
        private UIRadioButtonMenu positionMenu;
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
            hardwareMenu.Margin = new Thickness(0, 0, 25, 150);
            hardwareMenu.HorizontalAlignment = HorizontalAlignment.Right;
            hardwareMenu.VerticalAlignment = VerticalAlignment.Center;
            positionMenu = new UIRadioButtonMenu(positionTitles, 290.0, 380.0);
            positionMenu.Visibility = Visibility.Hidden;
            positionMenu.didSelectedIndex = (int i) => { NewPositionWasSelected((LayoutPosition)i); };
            switch (settings.startupPosition)
            {
                case (LayoutPosition)0: positionMenu.SetButtonSelected(0, true); break;
                case (LayoutPosition)1: positionMenu.SetButtonSelected(1, true); break;
                case (LayoutPosition)2: positionMenu.SetButtonSelected(2, true); break;
                case (LayoutPosition)3: positionMenu.SetButtonSelected(3, true); break;
                case (LayoutPosition)4: positionMenu.SetButtonSelected(4, true); break;
            }
            settingsPanel.Children.Add(positionMenu);
            settingsPanel.rightPanel.Children.Add(hardwareMenu);
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            settingsPanel.SetButtonSelected(0, true);
            settingsPanel.didSelectedNewColor = (color) => { NewColorWasSelected(color); };
            foreach (Vital vital in settings.startupVitals) { vitalsToMonitor.Add(vital); }
            for(int i = 0; i < hardwareVitalTitles.Length; i++)
            {
                var vital = (Vital)i;
                var checkbox = new UICheckBox(hardwareVitalTitles[i]);
                checkbox.label.Foreground = new SolidColorBrush(Color.FromRgb((byte)200, (byte)200, (byte)200));
                checkboxes.Add(checkbox);
                checkbox.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {

                    if (checkbox.isChecked) { vitalsToMonitor.Add(vital); this.Delegate.DidSelectNewVital(vital, true); }
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
            checkboxes[index].SetChecked(selected);
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

        private void NewPositionWasSelected(LayoutPosition position)
        {
            this.Delegate.DidSelectNewPosition(position);
            settings.startupPosition = position;
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
                default: break;
            }
        }

        private void ShowColorPicker()
        {
            settingsPanel.colorCanvas.Visibility = Visibility.Visible;
            hardwareMenu.Visibility = Visibility.Hidden;
            positionMenu.Visibility = Visibility.Hidden;

            switch (currentSetting)
            {
                case SettingsOption.WindowBackgroundColor: settingsPanel.colorCanvas.SelectedColor = settings.windowBackgroundColor; break;
                case SettingsOption.BarBackgroundColor: settingsPanel.colorCanvas.SelectedColor = settings.barBackgroundColor; break;
                case SettingsOption.BarForegroundColor: settingsPanel.colorCanvas.SelectedColor = settings.barForegroundColor; break;
                case SettingsOption.BorderColor: settingsPanel.colorCanvas.SelectedColor = settings.borderColor; break;
                default: break;
            }
        }

        private void ShowCheckMenu()
        {
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            hardwareMenu.Visibility = Visibility.Visible;
            positionMenu.Visibility = Visibility.Hidden;
            for (int i = 0; i < checkboxes.ToArray().Length; i++)
            {
                var checkbox = checkboxes[i];
                if (vitalsToMonitor.Contains((Vital)i)) { checkbox.SetChecked(true); }
            }
        }

        private void ShowRadioButtonMenu()
        {
            switch(settings.startupPosition)
            {
                case (LayoutPosition)0: positionMenu.SetButtonSelected(0, true); break;
                case (LayoutPosition)1: positionMenu.SetButtonSelected(1, true); break;
                case (LayoutPosition)2: positionMenu.SetButtonSelected(2, true); break;
                case (LayoutPosition)3: positionMenu.SetButtonSelected(3, true); break;
                case (LayoutPosition)4: positionMenu.SetButtonSelected(4, true); break;
            }
            settingsPanel.colorCanvas.Visibility = Visibility.Hidden;
            hardwareMenu.Visibility = Visibility.Hidden;
            positionMenu.Visibility = Visibility.Visible;
        }

        private void ShowFontMenu()
        {

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


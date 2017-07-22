using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace PCHardwareMonitor
{
    class WindowManager: SettingsDelegate
    {
        private UserSettings settings;
        private Window parent;
        private Grid root;
        private double indicatorHeight = 40.0;
        private double indicatorWidth = 280.0;
        private HoverButton settingsButton;
        private SolidColorBrush windowColor = new SolidColorBrush(Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255));
        private List<BorderedVitalIndicator> borderedIndicators = new List<BorderedVitalIndicator>();
        private VitalMonitor monitor = new VitalMonitor();
        private List<Window> windows = new List<Window>();
        private LayoutPosition currentPosition;
        private Vital[] vitals = new Vital[] { Vital.cpuLoad, Vital.ramLoad,
                                               Vital.gpuLoad, Vital.gpuMemoryLoad,
                                               Vital.gpuTemp, Vital.gpuFanSpeed, Vital.driveSpace };
        private HardwareVital[] vitalsToMonitor;

        public WindowManager(UserSettings settings, Window parent, Grid root)
        {
            this.settings = settings;
            this.parent = parent;
            this.root = root;
        }

        public void Open()
        {
            SetupIndicators(() => {
                ApplySettings();
                SetPosition(currentPosition);
                ApplySettings();
                ShowWindows();
                AddSettingsButton();
            });
        }

        private void ApplySettings()
        {
            if (settings != null)
            {
                this.vitalsToMonitor = settings.startupVitals;
                this.currentPosition = settings.startupPosition;
                foreach (var window in windows) { window.Background = new SolidColorBrush(settings.windowBackgroundColor); }
                foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.indicator.SetBarBackgroundColor(settings.barBackgroundColor); }
                foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.indicator.SetBarForegroundColor(settings.barForegroundColor); }
                foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.BorderBrush = new SolidColorBrush(settings.borderColor); }
            }
        }

        private void ShowWindows() { foreach (var window in windows.ToArray()) { window.Show(); } }

        private void SetupIndicators(Action completion)
        {
            foreach (var vital in vitals)
            {
                VitalIndicator indicator;
                switch (vital)
                {
                    case Vital.cpuLoad:
                        indicator = new VitalIndicator($"{VitalMonitor.GetCPUName()} Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToCPUUsage(indicator);
                        break;
                    case Vital.ramLoad:
                        indicator = new VitalIndicator("RAM Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToRAMUsage(indicator);
                        break;
                    case Vital.cpuCoreLoad: /*SetupCPUCoreIndicators();*/ return;
                    case Vital.gpuLoad:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPULoad(indicator);
                        break;
                    case Vital.gpuMemoryLoad:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Memory ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUMemoryLoad(indicator);
                        break;
                    case Vital.gpuFanSpeed:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Fan ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUFanSpeed(indicator);
                        break;
                    case Vital.gpuTemp:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Temp ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUTemp(indicator);
                        break;
                    case Vital.driveSpace:
                        var drives = DriveInfo.GetDrives();
                        for (int i = 0; i < drives.Count(); i++)
                        {
                            var drive = drives[i];
                            var driveIndicator = new VitalIndicator($"{drive.Name} ", this.indicatorWidth, this.indicatorHeight);
                            monitor.ListenToDrive(driveIndicator, drive);
                            AddIndicator(driveIndicator);
                        }
                        continue;
                    default: continue;
                }
                AddIndicator(indicator);
            }
            completion();
        }

        private void AddIndicator(VitalIndicator indicator)
        {
            var borderedIndicator = new BorderedVitalIndicator(indicator, this.indicatorWidth, this.indicatorHeight);
            var window = AddWindow(borderedIndicator, (indicator.Width - 70.0), indicator.Height);
            windows.Add(window);
            borderedIndicators.Add(borderedIndicator);
        }


        private Window AddWindow(UIElement root, double width, double height)
        {
            var window = new Window();
            window.Width = width;
            window.Height = height;
            window.AllowsTransparency = true;
            window.WindowStyle = WindowStyle.None;
            var windowBlur = new WindowBlur(window);
            window.ShowInTaskbar = false;
            window.Content = root;
            window.Background = windowColor;
            window.MouseDown += (object sender, MouseButtonEventArgs e) => { window.DragMove(); };
            window.Loaded += (object sender, RoutedEventArgs e) => { windowBlur.Apply(); };
            return window;
        }

        private void SetPosition(LayoutPosition position)
        {
            switch(position)
            {
                case LayoutPosition.TopRight:
                    WindowPositioner.StackWindowsToTopRight(this.windows.ToArray());
                    WindowPositioner.PositionToRight(this.parent);
                    break;
                case LayoutPosition.TopLeft:
                    WindowPositioner.StackWindowsToTopLeft(this.windows.ToArray());
                    WindowPositioner.PositionToLeft(this.parent);
                    break;
                case LayoutPosition.BottomRight:
                    WindowPositioner.StackWindowsToBottomRight(this.windows.ToArray());
                    WindowPositioner.PositionToRight(this.parent);
                    break;
                case LayoutPosition.BottomLeft:
                    WindowPositioner.StackWindowsToBottomLeft(this.windows.ToArray());
                    WindowPositioner.PositionToLeft(this.parent);
                    break;
                case LayoutPosition.Center:
                    WindowPositioner.StackWindowsToCenter(this.windows.ToArray());
                    WindowPositioner.PositionToCenter(this.parent);
                    break;
                default: break;
            }
            if (settingsButton != null) { SetSettingsButtonPosition(position); }
        }

        private void AddSettingsButton()
        {
            settingsButton = new HoverButton(30.0, 30.0);
            SetSettingsButtonPosition(currentPosition);
            settingsButton.CornerRadius = new CornerRadius(5);
            settingsButton.Opacity = 0.0;
            settingsButton.onClick = () => { ShowSettingsWindow(); };
            this.parent.MouseLeave += (object sender, MouseEventArgs e) => { settingsButton.SetHidden(); };
            this.parent.MouseEnter += (object sender, MouseEventArgs e) => { settingsButton.SetVisible(); };
            root.Children.Add(settingsButton);
        }

        private void SetSettingsButtonPosition(LayoutPosition position)
        {
            switch (position)
            {
                case LayoutPosition.TopRight:
                    settingsButton.Margin = new Thickness(0, 100, 60, 0);
                    settingsButton.HorizontalAlignment = HorizontalAlignment.Left;
                    settingsButton.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case LayoutPosition.TopLeft:
                    settingsButton.Margin = new Thickness(60, 100, 0, 0);
                    settingsButton.HorizontalAlignment = HorizontalAlignment.Right;
                    settingsButton.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case LayoutPosition.BottomRight:
                    settingsButton.Margin = new Thickness(0, 0, 60, 100);
                    settingsButton.HorizontalAlignment = HorizontalAlignment.Left;
                    settingsButton.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case LayoutPosition.BottomLeft:
                    settingsButton.Margin = new Thickness(60, 0, 0, 100);
                    settingsButton.HorizontalAlignment = HorizontalAlignment.Right;
                    settingsButton.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case LayoutPosition.Center:
                    settingsButton.Margin = new Thickness(60, 0, 0, 100);
                    settingsButton.HorizontalAlignment = HorizontalAlignment.Right;
                    settingsButton.VerticalAlignment = VerticalAlignment.Center;
                    break;
                default: break;
            }
        }

        private void ShowSettingsWindow()
        {
            var window = new Window();
            window.Width = 600.0;
            window.Height = 700.0;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            var settings = new string[] { "Hardware", "Window Background Color", "Bar Background Color", "Bar Foreground Color", "Border Color", "Font", "Position", "Reset" };
            var settingsPanel = new SettingsPanel(settings);
            var handler = new SettingsHandler(settingsPanel);
            window.Closed += (object sender, EventArgs e) => { handler.Dispose(); };
            handler.Delegate = this;
            window.Content = settingsPanel;
            window.Show();
        }

        public void DidSelectNewColor(SettingsOption selectedOption, Color? newColor)
        {
            Color color;
            if (newColor.HasValue) { color = newColor.Value; }
            else { return; }
            switch (selectedOption)
            {
                case SettingsOption.WindowBackgroundColor:
                    foreach (var window in windows) { window.Background = new SolidColorBrush(color); }
                    break;
                case SettingsOption.BarBackgroundColor:
                    foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.indicator.SetBarBackgroundColor(color); }
                    break;
                case SettingsOption.BarForegroundColor:
                    foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.indicator.SetBarForegroundColor(color); }
                    break;
                case SettingsOption.BorderColor:
                    foreach (var borderedIndicator in borderedIndicators) { borderedIndicator.BorderBrush = new SolidColorBrush(color); }
                    break;
                default: break;
            }
        }
        public void DidSelectNewPosition(LayoutPosition position)
        {
            foreach (var window in windows) { window.Opacity = 0.0; }
            SetPosition(position);
            foreach (var window in windows) { window.Opacity = 1.0; }
        }

        public void DidSelectNewVital(HardwareVital vital)
        {

        }

        public void ResetSettings()
        {
            try { if (File.Exists(AppDirectory.userSettings)) { File.Delete(AppDirectory.userSettings); } }
            catch (System.Exception ex) { Console.WriteLine(ex); }
            finally
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.defaultSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
                ApplySettings();
            }
        }

        // TODO
        /*private void SetupCPUCoreIndicators()
        {
            List<VitalIndicator> indicators = new List<VitalIndicator>();
            for (int i = 0; i < VitalMonitor.GetCPUCoreCount(); i++)
            {
                var indicator = new VitalIndicator($"CPU {i + 1}: ", this.indicatorWidth, this.indicatorHeight);
                indicator.Height = indicator.Height;
                indicator.label.FontSize = 15.0;
                indicator.VerticalAlignment = VerticalAlignment.Bottom;
                indicators.Add(indicator);
            }
            monitor.ListenToCPUCoreUsage(indicators.ToArray());
        }*/
    }
}

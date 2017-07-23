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
        private Button settingsButton;
        private SolidColorBrush windowColor = new SolidColorBrush(Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255));
        
        private VitalMonitor monitor = new VitalMonitor();
        private List<IndicatorWindow> indicatorWindows = new List<IndicatorWindow>();
        private LayoutPosition currentPosition;
        private Vital[] vitalsToMonitor;

        public WindowManager(UserSettings settings, Window parent, Grid root)
        {
            this.settings = settings;
            this.parent = parent;
            this.root = root;
        }

        public void Open()
        {
            this.vitalsToMonitor = settings.startupVitals;
            this.currentPosition = settings.startupPosition;
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
                foreach (var indicatorWindow in indicatorWindows)
                {
                    indicatorWindow.Background = new SolidColorBrush(settings.windowBackgroundColor);
                    indicatorWindow.SetBarBackgroundColor(settings.barBackgroundColor); 
                    indicatorWindow.SetBarForegroundColor(settings.barForegroundColor);
                    indicatorWindow.SetBorderBrushColor(settings.borderColor);
                }
            }
        }

        private void ShowWindows() { foreach (var indicatorWindow in indicatorWindows.ToArray()) { indicatorWindow.Show(); } }

        private void SetupIndicators(Action completion)
        {
            foreach (var vital in vitalsToMonitor)
            {
                VitalIndicator indicator;
                switch (vital)
                {
                    case Vital.CPUUsage:
                        indicator = new VitalIndicator($"{VitalMonitor.GetCPUName()} Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToCPUUsage(indicator);
                        break;
                    case Vital.RAMUsage:
                        indicator = new VitalIndicator("RAM Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToRAMUsage(indicator);
                        break;
                    case Vital.CPUCoreUsage: /*SetupCPUCoreIndicators();*/ return;
                    case Vital.GPUUsage:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Load ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPULoad(indicator);
                        break;
                    case Vital.GPUMemoryUsage:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Memory ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUMemoryLoad(indicator);
                        break;
                    case Vital.GPUFanRPM:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Fan ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUFanSpeed(indicator);
                        break;
                    case Vital.GPUTemp:
                        indicator = new VitalIndicator($"{VitalMonitor.GetGPUName()} Temp ", this.indicatorWidth, this.indicatorHeight);
                        monitor.ListenToGPUTemp(indicator);
                        break;
                    case Vital.HarddriveSpace:
                        var drives = DriveInfo.GetDrives();
                        for (int i = 0; i < drives.Count(); i++)
                        {
                            var drive = drives[i];
                            var driveIndicator = new VitalIndicator($"{drive.Name} ", this.indicatorWidth, this.indicatorHeight);
                            monitor.ListenToDrive(driveIndicator, drive);
                            AddIndicatorWindow(driveIndicator, vital);
                        }
                        continue;
                    default: continue;
                }
                AddIndicatorWindow(indicator, vital);
            }
            completion();
        }

        private void AddIndicatorWindow(VitalIndicator indicator, Vital monitoringVital)
        {
            var borderedIndicator = new BorderedVitalIndicator(indicator, this.indicatorWidth, this.indicatorHeight);
            var indicatorWindow = new IndicatorWindow(borderedIndicator, monitoringVital, (indicator.Width - 70.0), indicator.Height);
            indicatorWindows.Add(indicatorWindow);
        }

        private void SetPosition(LayoutPosition position)
        {
            switch (position)
            {
                case LayoutPosition.TopRight:
                    WindowPositioner.StackWindowsToTopRight(this.indicatorWindows.ToArray());
                    WindowPositioner.PositionToRight(this.parent);
                    break;
                case LayoutPosition.TopLeft:
                    WindowPositioner.StackWindowsToTopLeft(this.indicatorWindows.ToArray());
                    WindowPositioner.PositionToLeft(this.parent);
                    break;
                case LayoutPosition.BottomRight:
                    WindowPositioner.StackWindowsToBottomRight(this.indicatorWindows.ToArray());
                    WindowPositioner.PositionToRight(this.parent);
                    break;
                case LayoutPosition.BottomLeft:
                    WindowPositioner.StackWindowsToBottomLeft(this.indicatorWindows.ToArray());
                    WindowPositioner.PositionToLeft(this.parent);
                    break;
                case LayoutPosition.Center:
                    WindowPositioner.StackWindowsToCenter(this.indicatorWindows.ToArray());
                    WindowPositioner.PositionToCenter(this.parent);
                    break;
                default: break;
            }
            if (settingsButton != null) { SetSettingsButtonPosition(position); }
        }

        private void AddSettingsButton()
        {
            var rectangle = new RectangleGeometry();
            rectangle.RadiusX = 5.0;
            rectangle.RadiusY = 5.0;
            rectangle.Rect = new Rect(new Point(0, 0), new Size(30.0, 30.0));
            settingsButton = new Button();
            settingsButton.Width = 30.0;
            settingsButton.Height = 30.0;
            settingsButton.Background = new SolidColorBrush(Color.FromArgb((byte)1, (byte)45, (byte)45, (byte)45));
            settingsButton.ClipToBounds = true;
            settingsButton.Clip = rectangle;
            settingsButton.BorderThickness = new Thickness(0);
            SetSettingsButtonPosition(currentPosition);
            settingsButton.Click += (object sender, RoutedEventArgs e) => { ShowSettingsWindow(); };
            this.parent.MouseLeave += (object sender, MouseEventArgs e) => {
                if (this.parent.IsMouseOver == false)
                {
                    System.Threading.Thread.Sleep(1000);
                    settingsButton.Background = new SolidColorBrush(Color.FromArgb((byte)1, (byte)45, (byte)45, (byte)45));
                }
            };
            this.parent.MouseEnter += (object sender, MouseEventArgs e) => {
                settingsButton.Background = new SolidColorBrush(Color.FromArgb((byte)255, (byte)45, (byte)45, (byte)45));
            };
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
                    foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.Background = new SolidColorBrush(color); }
                    break;
                case SettingsOption.BarBackgroundColor:
                    foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.SetBarBackgroundColor(color); }
                    break;
                case SettingsOption.BarForegroundColor:
                    foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.SetBarForegroundColor(color); }
                    break;
                case SettingsOption.BorderColor:
                    foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.SetBorderBrushColor(color); }
                    break;
                default: break;
            }
        }
        public void DidSelectNewPosition(LayoutPosition position)
        {
            foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.Opacity = 0.0; }
            SetPosition(position);
            foreach (var indicatorWindow in indicatorWindows) { indicatorWindow.Opacity = 1.0; }
        }



        public void DidSelectNewVital(Vital vital, bool addOrRemove)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PCHardwareMonitor
{
    public enum Vital { cpuLoad, ramLoad, gpuLoad, gpuMemoryLoad, gpuFanSpeed, gpuTemp, driveSpace, cpuCoreLoad };
    public enum WindowOrientation { vertical, horizontal }
    public enum WindowPosition { top, bottom, left, right }

    public partial class MainWindow : Window, SettingsDelegate
    {
        private SolidColorBrush windowColor = new SolidColorBrush(Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255));
        private double indicatorHeight = 40.0;
        private double indicatorWidth = 280.0;
        private List<BorderedVitalIndicator> borderedIndicators = new List<BorderedVitalIndicator>();
        private VitalMonitor monitor = new VitalMonitor();
        private List<Window> windows = new List<Window>();
        private UserSettings settings;
        private WindowPosition currentPosition = WindowPosition.right;
        private Vital[] vitals = new Vital[] { Vital.cpuLoad, Vital.ramLoad,
                                               Vital.gpuLoad, Vital.gpuMemoryLoad,
                                               Vital.gpuTemp, Vital.gpuFanSpeed, Vital.driveSpace };
        private HardwareVital[] vitalsToMonitor;
        private LayoutPosition position;

        public MainWindow()
        {
            InitializeComponent();
            VitalMonitor.currentPC.Open();
            VitalMonitor.currentPC.CPUEnabled = true;
            VitalMonitor.currentPC.GPUEnabled = true;
            VitalMonitor.currentPC.HDDEnabled = true;
            VitalMonitor.currentPC.RAMEnabled = true;
            VitalMonitor.currentPC.MainboardEnabled = true;
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.Background = new SolidColorBrush(Color.FromArgb((byte)1, (byte)255, (byte)255, (byte)255));
            this.ShowInTaskbar = false;
            ReadSettings();
            SetupIndicators(() => {
                SetPosition(currentPosition);
                ApplySettings();
                ShowWindows();
                AddSettingsButton();
            });
            /*this.Loaded += (object sender, RoutedEventArgs e) => {
                ReadSettings();
                SetupIndicators(() => {
                    SetPosition(currentPosition);
                    ReadSettings();
                    foreach (var window in windows.ToArray()) { window.Show(); }
                });
                AddSettingsButton();
            };*/
        }

        private void ReadSettings()
        {
            try
            {
                if (Directory.Exists(AppDirectory.rootDirectory) == false) { Directory.CreateDirectory(AppDirectory.rootDirectory); }
                if (File.Exists($"{AppDirectory.rootDirectory}/user.json") == false)
                {
                    var defaults = UserSettings.defaults;
                    settings = defaults;
                    return;
                }
                var saveSettings = UserSettings.LoadFromFile($"{AppDirectory.rootDirectory}/user.json");
                if (saveSettings == null)
                {
                    var defaults = UserSettings.defaults;
                    settings = defaults;
                    return;
                }
                settings = saveSettings;
                this.vitalsToMonitor = settings.startupVitals;
                this.position = settings.startupPosition;
            }
            catch (System.Exception ex) { Console.WriteLine(ex); }
        }

        private void ApplySettings()
        {
            if (settings != null)
            {
                this.vitalsToMonitor = settings.startupVitals;
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
                    case Vital.cpuCoreLoad: SetupCPUCoreIndicators(); return;
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

        private void SetPosition(WindowPosition position)
        {
            switch(position)
            {
                case WindowPosition.top:
                    WindowPositioner.PositionToTop(this);
                    WindowPositioner.StackWindowsToTop(windows.ToArray()); break;
                case WindowPosition.bottom:
                    WindowPositioner.PositionToBottom(this);
                    WindowPositioner.StackWindowsToBottom(windows.ToArray()); break;
                case WindowPosition.right:
                    WindowPositioner.PositionToRight(this);
                    WindowPositioner.StackWindowsToRight(windows.ToArray()); break;
                case WindowPosition.left:
                    WindowPositioner.PositionToLeft(this);
                    WindowPositioner.StackWindowsToLeft(windows.ToArray()); break;
            }
        }

        private void AddSettingsButton()
        {
            var button = new HoverButton(30.0, 30.0);
            button.onClick = () => {  };
            switch(currentPosition)
            {
                case WindowPosition.right:
                    button.Margin = new Thickness(0, 100, 60, 0); 
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case WindowPosition.left:
                    button.Margin = new Thickness(60, 100, 0, 0); 
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case WindowPosition.top:
                    button.Margin = new Thickness(60, 100, 0, 0);
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    break;
                case WindowPosition.bottom:
                    button.Margin = new Thickness(60, 0, 0, 100);
                    button.HorizontalAlignment = HorizontalAlignment.Left;
                    button.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
            }
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.VerticalAlignment = VerticalAlignment.Top;
            button.CornerRadius = new CornerRadius(5);
            button.Opacity = 0.0;
            button.onClick = () => { ShowSettingsWindow(); };
            this.MouseLeave += (object sender, MouseEventArgs e) => { button.SetHidden(); };
            this.MouseEnter += (object sender, MouseEventArgs e) => { button.SetVisible(); };
            root.Children.Add(button);
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
            switch(selectedOption)
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
                var defaults = UserSettings.defaults;
                settings = defaults;
                //ReadSettings();
                ApplySettings();
            }
        }

        public void PrintSettings()
        {
            Console.WriteLine("Window Background color" + settings.windowBackgroundColor);
        }

        /// <summary>
        /// Not done yet
        /// </summary>
        private void SetupCPUCoreIndicators()
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
        }
    }
}



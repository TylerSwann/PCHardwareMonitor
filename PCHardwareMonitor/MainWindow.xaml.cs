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
    public enum Vital { GPUTemp = 0, GPUFanRPM, GPUUsage, GPUMemoryUsage, CPUUsage, CPUCoreUsage, RAMUsage, HarddriveSpace };

    public partial class MainWindow : Window
    {
        
        private UserSettings settings;
        private WindowManager manager;

        public MainWindow()
        {
            InitializeComponent();
            VitalMonitor.currentPC.Open();
            VitalMonitor.currentPC.CPUEnabled = true;
            VitalMonitor.currentPC.GPUEnabled = true;
            VitalMonitor.currentPC.HDDEnabled = true;
            VitalMonitor.currentPC.RAMEnabled = true;
            VitalMonitor.currentPC.MainboardEnabled = true;
            Uri iconUri = new Uri("pack://application:,,,/heart.ico", UriKind.RelativeOrAbsolute);
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.Background = new SolidColorBrush(Color.FromArgb((byte)1, (byte)255, (byte)255, (byte)255));
            this.ShowInTaskbar = false;
            this.Title = "PC Hardware Monitor";
            this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
            ReadSettings();
            manager = new WindowManager(this.settings, this, this.root);
            manager.Open();
        }

        public void ShowSettingsWindow() { manager.ShowSettingsWindow(); }

        private void ReadSettings()
        {
            if (Directory.Exists(AppDirectory.rootDirectory) == false) { Directory.CreateDirectory(AppDirectory.rootDirectory); }
            if (File.Exists(AppDirectory.defaultSettings) == false) { UserSettings.defaults.WriteToFile(AppDirectory.defaultSettings); }
            if (File.Exists($"{AppDirectory.rootDirectory}/user.json") == false)
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.defaultSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
                return;
            }
            else
            {
                try { this.settings = UserSettings.LoadFromFile(AppDirectory.userSettings); }
                catch (System.Exception ex) { Console.WriteLine(ex); }
            }
            if (settings == null) { Console.WriteLine("SETTINGS IS NULL"); }
        }
    }
}



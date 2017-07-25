using System;
using System.Windows;

namespace PCHardwareMonitor
{
    public partial class App : Application
    {
        public App()
        {
            var notifyicon = new System.Windows.Forms.NotifyIcon();
            notifyicon.Visible = true;
            notifyicon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/heart.ico")).Stream);
            notifyicon.Click += (object sender, EventArgs e) => {
                try
                {
                    var mainwindow = (MainWindow)Application.Current.MainWindow;
                    mainwindow.ShowSettingsWindow();
                }
                catch (System.Exception ex) { Console.WriteLine(ex); }
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PCHardwareMonitor
{
    public class IndicatorWindow: Window
    {
        public BorderedVitalIndicator indicator;
        public Vital monitoringVital;

        public IndicatorWindow(BorderedVitalIndicator indicator, Vital monitoringVital, double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.indicator = indicator;
            this.monitoringVital = monitoringVital;
            var windowBlur = new WindowBlur(this);
            this.ShowInTaskbar = false;
            this.Content = indicator;
            this.Background = new SolidColorBrush(Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255)); ;
            this.MouseDown += (object sender, MouseButtonEventArgs e) => { this.DragMove(); };
            this.Loaded += (object sender, RoutedEventArgs e) => { windowBlur.Apply(); };
            this.Title = monitoringVital.ToString();
        }

        public void SetBarBackgroundColor(Color color) { this.indicator.indicator.SetBarBackgroundColor(color); }
        public void SetBarForegroundColor(Color color) { this.indicator.indicator.SetBarForegroundColor(color); }
        public void SetBorderBrushColor(Color color) { this.indicator.BorderBrush = new SolidColorBrush(color); }
    }
}

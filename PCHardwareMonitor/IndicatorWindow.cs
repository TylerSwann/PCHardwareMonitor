using System;
using System.Windows;
using System.Windows.Media;
using PCHardwareMonitor.IndicatorTheming;

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
            Uri iconUri = new Uri("pack://application:,,,/heart.ico", UriKind.RelativeOrAbsolute);
            this.ShowInTaskbar = false;
            this.Content = indicator;
            this.Background = new SolidColorBrush(Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255)); ;
            this.Loaded += (object sender, RoutedEventArgs e) => { windowBlur.Apply(); };
            this.Title = monitoringVital.ToString();
            this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
        }

        public void SetBarBackgroundColor(Color color) { this.indicator.indicator.SetBarBackgroundColor(color); }
        public void SetBarForegroundColor(Color color) { this.indicator.indicator.SetBarForegroundColor(color); }
        public void SetBorderBrushColor(Color color) { this.indicator.BorderBrush = new SolidColorBrush(color); }
        public void SetFont(Font font)
        {
            this.indicator.indicator.label.FontFamily = new FontFamily(font.fontFamily);
            this.indicator.indicator.label.FontSize = font.size;
            this.indicator.indicator.label.Foreground = new SolidColorBrush(font.textColor);
        }
    }
}

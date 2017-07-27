
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;


namespace PCHardwareMonitor
{
    public class BorderedVitalIndicator: Border
    {
        public VitalIndicator indicator;

        public BorderedVitalIndicator(VitalIndicator indicator, double width, double height)
        {
            this.indicator = indicator;
            var indicatorRoot = new Grid();
            indicatorRoot.Width = width;
            indicatorRoot.Height = (height * 1.5);
            indicatorRoot.HorizontalAlignment = HorizontalAlignment.Center;
            indicatorRoot.VerticalAlignment = VerticalAlignment.Top;
            this.Width = (indicator.Width - 70.0);
            this.Height = indicator.Height;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)255, (byte)50, (byte)50, (byte)50));
            this.BorderThickness = new Thickness(2);
            this.Child = indicatorRoot;
            indicator.label.FontSize = 15.0;
            indicator.label.VerticalAlignment = VerticalAlignment.Bottom;
            indicator.label.Margin = new Thickness(0, 0, 0, 15);
            indicatorRoot.Children.Add(indicator);
        }
    }
}

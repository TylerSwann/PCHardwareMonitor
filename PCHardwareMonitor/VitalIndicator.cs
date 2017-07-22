

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCHardwareMonitor
{
    public class VitalIndicator : Grid
    {
        public readonly ProgressBar vitalBar;
        public readonly Label label;
        public String title;

        public VitalIndicator(String title, double width, double height)
        {
            this.title = title;
            this.Width = (width * 1.5);
            this.Height = (height * 2);
            label = new Label();
            label.Content = title;
            label.Width = width;
            label.Height = height;
            label.FontSize = 20.0;
            label.Foreground = new SolidColorBrush(Color.FromRgb((byte)0, (byte)0, (byte)0));
            vitalBar = new ProgressBar();
            vitalBar.Width = width;
            vitalBar.Height = (height / 1.5);
            Setup();
        }

        private void Setup()
        {
            
            var foregroundColor = Color.FromRgb((byte)50, (byte)50, (byte)50);
            var backgroundColor = Color.FromArgb((byte)120, (byte)250, (byte)250, (byte)250);
            vitalBar.BorderThickness = new Thickness(0);
            SetBarColor(backgroundColor, foregroundColor);
            //this.Background = new SolidColorBrush(Color.FromArgb((byte)255, (byte)0, (byte)0, (byte)250));
            this.Children.Add(label);
            this.Children.Add(vitalBar);
            SetAlignment();
        }

        void SetAlignment()
        {
            this.VerticalAlignment = VerticalAlignment.Center;
            this.Margin = new Thickness(0, 0, 0, 0);
            this.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Stretch;
            label.Padding = new Thickness(0, 0, 0, 0);
            label.Margin = new Thickness(0, 0, 0, 0);
            vitalBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            vitalBar.VerticalAlignment = VerticalAlignment.Bottom;
            vitalBar.Padding = new Thickness(0, 0, 0, 0);
            vitalBar.Margin = new Thickness(0, 0, 0, 0);
        }

        public void SetBarColor(Color backgroundColor, Color foregroundColor)
        {
            vitalBar.Background = new SolidColorBrush(backgroundColor);
            vitalBar.BorderBrush = new SolidColorBrush(backgroundColor);
            vitalBar.Foreground = new SolidColorBrush(foregroundColor);
        }

        public void SetBarBackgroundColor(Color backgroundColor) { vitalBar.Background = new SolidColorBrush(backgroundColor); }
        public void SetBarForegroundColor(Color foregroundColor) { vitalBar.Foreground = new SolidColorBrush(foregroundColor); }

        public void UpdateIndicator(String title, double newValue)
        {
            vitalBar.Value = newValue;
            label.Content = title;
        }
    }
}

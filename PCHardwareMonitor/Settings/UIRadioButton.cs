using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
namespace PCHardwareMonitor
{
    class UIRadioButton: Grid
    {
        public readonly Label label;
        public bool isOn { get; private set; }
        public Action onClick = () => { };
        private Border button;
        private Ellipse fillOffset = new Ellipse();
        private SolidColorBrush buttonColor = new SolidColorBrush(Color.FromRgb((byte)0, (byte)122, (byte)255));
        private SolidColorBrush deselectedColor = new SolidColorBrush(Color.FromArgb((byte)1, (byte)0, (byte)122, (byte)255));

        public UIRadioButton(string message)
        {
            var width = 180.0;
            var height = 40.0;
            this.Width = width;
            this.Height = height;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            label = new Label();
            label.Width = (width - 25.0);
            label.Height = height;
            label.Margin = new Thickness(0, 0, 0, 0);
            label.HorizontalContentAlignment = HorizontalAlignment.Left;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Content = message;
            label.FontSize = 16.0;
            button = new Border();
            button.Width = 25.0;
            button.Height = 25.0;
            button.BorderThickness = new Thickness(3);
            button.CornerRadius = new CornerRadius(15);
            button.BorderBrush = buttonColor;
            button.Margin = new Thickness(0, 5, 0, 0);
            button.Padding = new Thickness(0);
            button.VerticalAlignment = VerticalAlignment.Center;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.Background = deselectedColor;
            button.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => { SwitchSelected(true); onClick(); };
            button.MouseEnter += (object sender, MouseEventArgs e) => { button.Opacity = 0.7; fillOffset.Opacity = 0.7; };
            button.MouseLeave += (object sender, MouseEventArgs e) => { button.Opacity = 1.0; fillOffset.Opacity = 1.0; };

            fillOffset.Width = 25.0;
            fillOffset.Height = 25.0;
            fillOffset.Margin = new Thickness(0, 5, 0, 0);
            fillOffset.VerticalAlignment = VerticalAlignment.Center;
            fillOffset.HorizontalAlignment = HorizontalAlignment.Left;
            fillOffset.Fill = deselectedColor;
            fillOffset.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => { SwitchSelected(true); onClick(); };
            fillOffset.MouseEnter += (object sender, MouseEventArgs e) => { fillOffset.Opacity = 0.7; button.Opacity = 0.7; };
            fillOffset.MouseLeave += (object sender, MouseEventArgs e) => { fillOffset.Opacity = 1.0; button.Opacity = 1.0; };

            this.Children.Add(label);
            this.Children.Add(button);
            this.Children.Add(fillOffset);
            this.isOn = false;
            isOn = false;
        }

        public void SetSelected(bool isChecked)
        {
            if (isChecked && !this.isOn || !isChecked && this.isOn) { SwitchSelected(true); }
            else if (isChecked && this.isOn || !isChecked && !this.isOn) { return; }
        }

        private void SwitchSelected(bool animated)
        {
            this.isOn = isOn ? false : true;
            if (isOn)
            {
                Action completion = () => { button.Background = buttonColor; fillOffset.Fill = buttonColor; };
                if (animated) { PlayOpacityAnimation(completion); }
            }
            else
            {
                Action completion = () => { button.Background = deselectedColor; fillOffset.Fill = deselectedColor; };
                if (animated) { PlayOpacityAnimation(completion); }
            }
        }

        private void PlayOpacityAnimation(Action completion)
        {
            var fadeInAnimation = new DoubleAnimation
            {
                To = 1.0,
                BeginTime = TimeSpan.FromSeconds(0.0),
                Duration = TimeSpan.FromSeconds(0.05),
                FillBehavior = FillBehavior.Stop
            };
            fadeInAnimation.Completed += (s, a) =>
            {
                button.Opacity = 1.0;
                fillOffset.Opacity = 1.0;
                completion();
            };

            var fadeOutAnimation = new DoubleAnimation
            {
                To = 0.5,
                BeginTime = TimeSpan.FromSeconds(0.0),
                Duration = TimeSpan.FromSeconds(0.05),
                FillBehavior = FillBehavior.Stop
            };
            fadeOutAnimation.Completed += (s, a) =>
            {
                fillOffset.Opacity = 0.5;
                button.Opacity = 0.5;
                button.BeginAnimation(OpacityProperty, fadeInAnimation);
                fillOffset.BeginAnimation(OpacityProperty, fadeInAnimation);
            };
            button.BeginAnimation(OpacityProperty, fadeOutAnimation);
            fillOffset.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }
}

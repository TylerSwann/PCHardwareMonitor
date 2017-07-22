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
    public class UICheckBox: Grid
    {
        public readonly Label label;
        public bool isChecked { get;  private set; }
        public Action onClick = () => {};
        private Border checkBox;
        private Ellipse fillOffset = new Ellipse();
        private SolidColorBrush buttonColor = new SolidColorBrush(Color.FromRgb((byte)0, (byte)122, (byte)255));
        private SolidColorBrush deselectedColor = new SolidColorBrush(Color.FromArgb((byte)1, (byte)0, (byte)122, (byte)255));

        public UICheckBox(string message)
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
            checkBox = new Border();
            checkBox.Width = 25.0;
            checkBox.Height = 25.0;
            checkBox.BorderThickness = new Thickness(3);
            checkBox.CornerRadius = new CornerRadius(5);
            checkBox.BorderBrush = buttonColor;
            checkBox.Margin = new Thickness(0, 5, 0, 0);
            checkBox.Padding = new Thickness(0);
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.Background = deselectedColor;
            checkBox.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => { SwitchCheck(true); onClick(); };
            checkBox.MouseEnter += (object sender, MouseEventArgs e) => { checkBox.Opacity = 0.7; fillOffset.Opacity = 0.7; };
            checkBox.MouseLeave += (object sender, MouseEventArgs e) => { checkBox.Opacity = 1.0; fillOffset.Opacity = 1.0; };

            fillOffset.Width = 25.0;
            fillOffset.Height = 26.3;
            fillOffset.Margin = new Thickness(0, 5, 0, 0);
            fillOffset.VerticalAlignment = VerticalAlignment.Center;
            fillOffset.HorizontalAlignment = HorizontalAlignment.Left;
            fillOffset.Fill = deselectedColor;
            fillOffset.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => { SwitchCheck(true); onClick(); };
            fillOffset.MouseEnter += (object sender, MouseEventArgs e) => { fillOffset.Opacity = 0.7; checkBox.Opacity = 0.7; };
            fillOffset.MouseLeave += (object sender, MouseEventArgs e) => { fillOffset.Opacity = 1.0; checkBox.Opacity = 1.0; };

            this.Children.Add(label);
            this.Children.Add(checkBox);
            this.Children.Add(fillOffset);
            this.isChecked = false;
            isChecked = false;
        }

        public void SetChecked(bool isChecked)
        {
            if (isChecked && !this.isChecked || !isChecked && this.isChecked) { SwitchCheck(true); }
            else if (isChecked && this.isChecked || !isChecked && !this.isChecked) { return; }
        }

        private void SwitchCheck(bool animated)
        {
            this.isChecked = isChecked ? false : true;
            if(isChecked)
            {
                Action completion = () => { checkBox.Background = buttonColor; fillOffset.Fill = buttonColor; };
                if (animated) { PlayOpacityAnimation(completion); }
            }
            else
            {
                Action completion = () => { checkBox.Background = deselectedColor; fillOffset.Fill = deselectedColor; };
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
                checkBox.Opacity = 1.0;
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
                checkBox.Opacity = 0.5;
                checkBox.BeginAnimation(OpacityProperty, fadeInAnimation);
                fillOffset.BeginAnimation(OpacityProperty, fadeInAnimation);
            };
            checkBox.BeginAnimation(OpacityProperty, fadeOutAnimation);
            fillOffset.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace PCHardwareMonitor
{
    class SettingsPanel: Grid
    {
        public readonly Border leftPanel = new Border();
        public readonly Grid rightPanel = new Grid();
        private StackPanel buttonPanel = new StackPanel();
        private string[] buttonTitles;
        private Border border = new Border();
        private List<SettingsButton> buttons = new List<SettingsButton>();
        public readonly ColorCanvas colorCanvas = new ColorCanvas();
        public Action<int> didClickButtonAtIndex = (i) => {};
        public Action<Color?> didSelectedNewColor = (color) => {};

        public SettingsPanel(string[] buttonTitles)
        {
            this.buttonTitles = buttonTitles;
            this.Width = 600.0;
            this.Height = 700.0;
            this.Margin = new Thickness(0);
            this.Background = new SolidColorBrush(Color.FromRgb((byte)70, (byte)85, (byte)90));
            leftPanel.Width = 275.0;
            leftPanel.Height = 700.0;
            leftPanel.Margin = new Thickness(0);
            leftPanel.HorizontalAlignment = HorizontalAlignment.Left;
            leftPanel.VerticalAlignment = VerticalAlignment.Center;
            rightPanel.Width = 325.0;
            rightPanel.Height = 700.0;
            rightPanel.HorizontalAlignment = HorizontalAlignment.Right;
            rightPanel.VerticalAlignment = VerticalAlignment.Center;
            rightPanel.Background = new SolidColorBrush(Color.FromRgb((byte)60, (byte)74, (byte)77));//new SolidColorBrush(Color.FromRgb((byte)77, (byte)98, (byte)105));
            buttonPanel.Width = 275.0;
            buttonPanel.Height = 600.0;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Left;
            buttonPanel.VerticalAlignment = VerticalAlignment.Center;
            var colorCanvasBorder = new Border();
            colorCanvasBorder.Width = 290.0;
            colorCanvasBorder.Height = 380.0;
            colorCanvasBorder.BorderThickness = new Thickness(0, 0, 0, 0);
            colorCanvasBorder.Background = new SolidColorBrush(Color.FromRgb((byte)70, (byte)85, (byte)90));
            colorCanvasBorder.CornerRadius = new CornerRadius(10.0);
            colorCanvasBorder.Margin = new Thickness(0, 0, 25, 200);
            colorCanvasBorder.HorizontalAlignment = HorizontalAlignment.Right;
            colorCanvasBorder.VerticalAlignment = VerticalAlignment.Center;
            colorCanvas.Width = 275.0;
            colorCanvas.Height = 500.0;
            colorCanvas.BorderThickness = new Thickness(0, 0, 0, 0);
            colorCanvas.Background = new SolidColorBrush(Color.FromArgb((byte)0, (byte)0, (byte)0, (byte)0));
            colorCanvas.Margin = new Thickness(0, 0, 25, 0);
            colorCanvas.HorizontalAlignment = HorizontalAlignment.Right;
            colorCanvas.VerticalAlignment = VerticalAlignment.Center;
            colorCanvas.SelectedColorChanged += (object sender, RoutedPropertyChangedEventArgs<Color?> e) => { didSelectedNewColor(e.NewValue.Value); };
            border.Width = 325.0;
            border.Height = 700.0;
            border.BorderThickness = new Thickness(3, 0, 0, 0);
            border.BorderBrush = new SolidColorBrush(Color.FromRgb((byte)0, (byte)0, (byte)0));
            border.Margin = new Thickness(0);
            border.HorizontalAlignment = HorizontalAlignment.Right;
            border.VerticalAlignment = VerticalAlignment.Center;
            rightPanel.Children.Add(border);
            rightPanel.Children.Add(colorCanvasBorder);
            rightPanel.Children.Add(colorCanvas);
            this.Children.Add(leftPanel);
            this.Children.Add(rightPanel);
            this.Children.Add(buttonPanel);
            
            for (int i = 0; i < buttonTitles.Length; i++)
            {
                var button = new SettingsButton(275.0, 50.0);
                button.label.Foreground = new SolidColorBrush(Color.FromRgb((byte)200, (byte)200, (byte)200));
                var index = i;
                button.onClick += () => {
                    didClickButtonAtIndex(index);
                    foreach (var otherButton in buttons.ToArray())
                    {
                        if (otherButton == button) { continue; }
                        otherButton.SetSelected(false);
                    }
                };
                button.label.Content = buttonTitles[i];
                button.label.FontFamily = new FontFamily("Segoe UI Black");
                button.label.FontSize = 18.0;
                button.Margin = new Thickness(0, 5, 0, 0);
                buttons.Add(button);
                buttonPanel.Children.Add(button);
            }
        }

        public void SetButtonSelected(int index, bool selected)
        {
            if (index > buttons.Count - 1 || index < 0) { Console.WriteLine("BUTTON INDEX IS OUT OF RANGE"); return; }
            buttons[index].SetSelected(selected);
        }

    }

    internal class SettingsButton: Grid
    {
        internal Label label;
        internal Action onClick = () => { };
        private Color IdleColor = Color.FromRgb((byte)49, (byte)57, (byte)60);
        private Color hoverColor = Color.FromArgb((byte)200, (byte)49, (byte)57, (byte)60);
        private Color clickColor = Color.FromArgb((byte)100, (byte)49, (byte)57, (byte)60);
        private bool isSelected = false;

        internal SettingsButton(double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.Margin = new Thickness(0);
            label = new Label();
            label.Width = width;
            label.Height = height;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.Margin = new Thickness(0);
            this.Background = new SolidColorBrush(IdleColor);
            this.Children.Add(label);
            this.MouseEnter += (object sender, MouseEventArgs e) => { if (!isSelected) { this.Background = new SolidColorBrush(hoverColor); } };
            this.MouseLeave += (object sender, MouseEventArgs e) => { if (!isSelected) { this.Background = new SolidColorBrush(IdleColor); } };
            this.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {onClick(); this.Background = new SolidColorBrush(clickColor); this.isSelected = true; };
        }

        internal void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
            if (isSelected) { this.Background = new SolidColorBrush(clickColor); }
            else { this.Background = new SolidColorBrush(IdleColor);}
        }
    }
}

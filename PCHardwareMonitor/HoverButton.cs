using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PCHardwareMonitor
{
    public class HoverButton: Border
    {
        public Action onClick;
        private Color idleColor;
        private Color hoverColor;
        private Color clickColor;
        private bool isBeingSetHidden = false;

        public HoverButton(double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.idleColor = Color.FromRgb((byte)60, (byte)60, (byte)60);
            this.hoverColor = Color.FromRgb((byte)90, (byte)90, (byte)90);
            this.clickColor = Color.FromRgb((byte)120, (byte)120, (byte)120);
            this.Background = new SolidColorBrush(idleColor);
           // this.MouseLeave += (object sender, MouseEventArgs e) => { this.Background = new SolidColorBrush(idleColor); };
            //this.MouseEnter += (object sender, MouseEventArgs e) => { this.Background = new SolidColorBrush(hoverColor); };
            //this.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) => { this.Background = new SolidColorBrush(hoverColor); };
            this.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                //this.Background = new SolidColorBrush(clickColor);
                onClick?.Invoke();
            };
        }
        public void SetHoverColor(Color color) { this.hoverColor = color; }
        public void SetClickColor(Color color) { this.clickColor = color; }
        public void SetIdleColor(Color color) { this.idleColor = color; }

        public void SetHidden()
        {
            return;
            isBeingSetHidden = true;
            System.Threading.Thread.Sleep(2000);
            if (isBeingSetHidden) { this.Opacity = 0.0; }
            /*if (this.Opacity != 1.0) { return; }
            var fadeAnimation = new DoubleAnimation
            {
                To = 0.0,
                BeginTime = TimeSpan.FromSeconds(1.0),
                Duration = TimeSpan.FromSeconds(0.5),
                FillBehavior = FillBehavior.Stop
            };
            fadeAnimation.Completed += (s, a) => this.Opacity = 0.0;
            this.BeginAnimation(OpacityProperty, fadeAnimation);*/
        }
        public void SetVisible()
        {

            //isBeingSetHidden = false;
            this.Opacity = 1.0;
            /*if (this.Opacity != 0.0) { return; }
            var fadeAnimation = new DoubleAnimation
            {
                To = 1.0,
                BeginTime = TimeSpan.FromSeconds(1.0),
                Duration = TimeSpan.FromSeconds(0.5),
                FillBehavior = FillBehavior.Stop
            };
            fadeAnimation.Completed += (s, a) => this.Opacity = 1.0;
            this.BeginAnimation(OpacityProperty, fadeAnimation);*/
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
                this.Opacity = 1.0;
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
                this.Opacity = 0.5;
                this.BeginAnimation(OpacityProperty, fadeInAnimation);
            };
            this.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }
    }
}

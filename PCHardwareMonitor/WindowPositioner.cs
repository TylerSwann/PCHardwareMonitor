using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCHardwareMonitor
{
    public class WindowPositioner
    {
        private static double screenHeight = SystemParameters.PrimaryScreenHeight;
        private static double screenWidth = SystemParameters.PrimaryScreenWidth;

        public static void PositionToTop(Window window)
        {
            window.Width = SystemParameters.PrimaryScreenWidth;
            window.Height = (SystemParameters.PrimaryScreenHeight / 8);
            window.Top = 0.0;
            window.Left = 0.0;
        }
        public static void PositionToBottom(Window window)
        {
            var widgetHeight = (screenHeight / 8);
            window.Width = screenWidth;
            window.Height = widgetHeight;
            window.Top = (SystemParameters.WorkArea.Height - window.Height);
            window.Left = 0.0;
        }
        public static void PositionToRight(Window window)
        {
            var widgetWidth = (screenWidth / 6);
            window.Width = widgetWidth;
            window.Height = screenHeight;
            window.Top = 0.0;
            window.Left = (SystemParameters.WorkArea.Width - window.Width);
        }
        public static void PositionToLeft(Window window)
        {
            var widgetWidth = (screenWidth / 6);
            window.Width = widgetWidth;
            window.Height = screenHeight;
            window.Top = 0.0;
            window.Left = 0.0;
        }

        public static void PositionToCenter(Window window)
        {
            var widgetWidth = (screenWidth / 6);
            window.Width = widgetWidth;
            window.Height = screenHeight;
            window.Top = 0.0;
            window.Left = ((SystemParameters.WorkArea.Width / 2) - (window.Width / 2));
        }

        public static void StackWindowsToTopRight(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows)
            {
                window.Top = nextPosition;
                window.Left = (SystemParameters.WorkArea.Width - (window.Width + 10.0));
                nextPosition += spacing;
            }
        }
        public static void StackWindowsToTopLeft(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows)
            {
                window.Top = nextPosition;
                window.Left = 10.0;
                nextPosition += spacing;
            }
        }

        public static void StackWindowsToBottomRight(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows.Reverse())
            {
                window.Top = ((SystemParameters.WorkArea.Height - (window.Height + 10.0)) - (nextPosition));
                window.Left = (SystemParameters.WorkArea.Width - (window.Width + 10.0));
                nextPosition += spacing;
            }
        }
        public static void StackWindowsToBottomLeft(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows.Reverse())
            {
                window.Top = ((SystemParameters.WorkArea.Height - (window.Height + 10.0)) - (nextPosition));
                window.Left = 10.0;
                nextPosition += spacing;
            }
        }
        public static void StackWindowsToCenter(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            var requiredSpace = (windows.Length * (windows[0].Height + 10));
            var availableSpace = SystemParameters.WorkArea.Height;
            var padding = ((availableSpace - requiredSpace) / 2);
            foreach (var window in windows)
            {
                window.Top = nextPosition + padding;
                window.Left = ((SystemParameters.WorkArea.Width / 2) - ((window.Width / 2) + 10.0));
                nextPosition += spacing;
            }
        }

        public static void StackWindowsInRowToRight(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.1);
            var nextPosition = 10.0;
            var isSideWindow = false;
            foreach (var window in windows)
            {
                var leftOffset = (SystemParameters.WorkArea.Width - (window.Width + 10.0));
                window.Top = nextPosition;
                window.Left = isSideWindow ? leftOffset : (leftOffset - (window.Width + 10.0));
                nextPosition += isSideWindow ? spacing : 0.0;
                isSideWindow = !isSideWindow;
            }
        }

        public static void StackWindowsInRowToLeft(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.1);
            var nextPosition = 10.0;
            var isSideWindow = false;
            foreach (var window in windows)
            {
                var leftOffset = 10.0;
                window.Top = nextPosition;
                window.Left = isSideWindow ? leftOffset : (leftOffset + (window.Width + 10.0));
                nextPosition += isSideWindow ? spacing : 0.0;
                isSideWindow = !isSideWindow;
            }
        }

        // Will be removed
        public static void StackWindowsToRight(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows)
            {
                window.Top = nextPosition;
                window.Left = (SystemParameters.WorkArea.Width - (window.Width + 10.0));
                nextPosition += spacing;
            }
        }


        // Will be removed
        public static void StackWindowsToLeft(Window[] windows)
        {
            var spacing = (windows[0].Height * 1.2);
            var nextPosition = 10.0;
            foreach (var window in windows)
            {
                window.Top = nextPosition;
                window.Left = 10.0;
                nextPosition += spacing;
            }
        }


        // Will be removed
        public static void StackWindowsToTop(Window[] windows)
        {
            var spacing = (windows[0].Width * 1.1);
            var nextPosition = 5.0;
            foreach (var window in windows)
            {
                window.Top = 10.0;
                window.Left = nextPosition;
                nextPosition += spacing;
            }
        }
        // Will be removed
        public static void StackWindowsToBottom(Window[] windows)
        {
            var spacing = (windows[0].Width * 1.1);
            var nextPosition = 5.0;
            foreach (var window in windows)
            {
                window.Top = (SystemParameters.WorkArea.Height - (window.Height + 5.0));
                window.Left = nextPosition;
                nextPosition += spacing;
            }
        }
    }
}

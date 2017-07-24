using System.Windows.Media;

namespace PCHardwareMonitor.IndicatorTheming
{
    public class ColorScheme
    {
        public Color windowBackgroundColor;
        public Color barBackgroundColor;
        public Color barForegroundColor;
        public Color borderColor;
        public ColorScheme(Color windowBackgroundColor, Color barBackgroundColor, Color barForegroundColor, Color borderColor)
        {
            this.windowBackgroundColor = windowBackgroundColor;
            this.barBackgroundColor = barBackgroundColor;
            this.barForegroundColor = barForegroundColor;
            this.borderColor = borderColor;
        }
    }
}


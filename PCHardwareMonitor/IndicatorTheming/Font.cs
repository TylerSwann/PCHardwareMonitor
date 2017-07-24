
using System.Windows.Media;

namespace PCHardwareMonitor.IndicatorTheming
{
    public class Font
    {
        public string fontFamily;
        public Color textColor;
        public int size;
        public Font(string fontFamily, Color textColor, int size)
        {
            this.fontFamily = fontFamily;
            this.textColor = textColor;
            this.size = size;
        }
    }
}

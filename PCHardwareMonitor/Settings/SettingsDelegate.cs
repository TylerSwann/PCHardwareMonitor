
using System.Windows.Media;
using PCHardwareMonitor.IndicatorTheming;

namespace PCHardwareMonitor
{
    public interface SettingsDelegate
    {
        void DidSelectNewColor(SettingsOption selectedOption, Color? newColor);
        void DidSelectNewPosition(LayoutPosition position, bool layoutInRows);
        void DidSelectNewVital(Vital vital, bool shouldAdd);
        void DidSelectNewFont(Font font);
        void ResetSettings();
    }
}

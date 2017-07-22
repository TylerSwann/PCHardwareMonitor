
using System.Windows.Media;
using System;

namespace PCHardwareMonitor
{
    public interface SettingsDelegate
    {
        void DidSelectNewColor(SettingsOption selectedOption, Color? newColor);
        void DidSelectNewPosition(LayoutPosition position);
        void DidSelectNewVital(HardwareVital vital);
        void ResetSettings();
    }
}

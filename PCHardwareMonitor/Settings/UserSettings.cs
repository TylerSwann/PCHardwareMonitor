using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using PCHardwareMonitor.IndicatorTheming;

namespace PCHardwareMonitor
{
    public class UserSettings
    {
        private static Vital[] defaultStartupVitals = new Vital[] { Vital.CPUUsage, Vital.RAMUsage, Vital.GPUUsage, Vital.GPUMemoryUsage, Vital.GPUTemp, Vital.GPUFanRPM, Vital.HarddriveSpace };
        private static LayoutPosition defaultStartupPosition = LayoutPosition.TopRight;
        private static Color defaultWindowBackgroundColor = Color.FromArgb((byte)30, (byte)255, (byte)255, (byte)255);
        private static Color defaultBarBackgroundColor = Color.FromArgb((byte)120, (byte)250, (byte)250, (byte)250);
        private static Color defaultBarForegroundColor = Color.FromRgb((byte)50, (byte)50, (byte)50);
        private static Color defaultBorderColor = Color.FromArgb((byte)255, (byte)50, (byte)50, (byte)50);
        private static Color defaultFontColor = Color.FromRgb((byte)0, (byte)0, (byte)0);
        private static ColorScheme defaultColorScheme = new ColorScheme(defaultWindowBackgroundColor, defaultBarBackgroundColor, defaultBarForegroundColor, defaultBorderColor);
        private static Font defaultFont = new Font("Segoe UI", defaultFontColor, 15);
        public static readonly UserSettings defaults = new UserSettings(defaultStartupVitals, defaultStartupPosition, true, defaultColorScheme, defaultFont);


        public Vital[] startupVitals;
        public LayoutPosition startupPosition;
        public Color windowBackgroundColor;
        public Color barBackgroundColor;
        public Color barForegroundColor;
        public Color borderColor;
        public Font font;
        public bool layoutIsInRows;

        public UserSettings() { }

        public UserSettings(Vital[] startupVitals, LayoutPosition startupPosition, bool layoutIsInRows, ColorScheme colorScheme, Font font)
        {
            this.startupVitals = startupVitals;
            this.startupPosition = startupPosition;
            this.layoutIsInRows = layoutIsInRows;
            this.windowBackgroundColor = colorScheme.windowBackgroundColor;
            this.barBackgroundColor = colorScheme.barBackgroundColor;
            this.barForegroundColor = colorScheme.barForegroundColor;
            this.borderColor = colorScheme.borderColor;
            this.font = font;
        }

        public UserSettings(Vital[] startupVitals, LayoutPosition startupPosition, bool layoutIsInRows, Color windowBackgroundColor, Color barBackgroundColor, Color barForegroundColor, Color borderColor)
        {
            this.startupVitals = startupVitals;
            this.startupPosition = startupPosition;
            this.windowBackgroundColor = windowBackgroundColor;
            this.barBackgroundColor = barBackgroundColor;
            this.barForegroundColor = barForegroundColor;
            this.borderColor = borderColor;
        }
        
        public void WriteToFile(string path)
        {
            try
            {
                var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
                var rawJson = JsonConvert.SerializeObject(this, jsonSettings);
                System.IO.File.WriteAllText(path, rawJson);
            }
            catch (System.Exception ex) { Console.WriteLine(ex); }
        }

        public static UserSettings LoadFromFile(string path)
        {
            try
            {
                var rawJson = System.IO.File.ReadAllText(path);
                var jsonObject = JObject.Parse(rawJson);
                UserSettings settings = JsonConvert.DeserializeObject<UserSettings>(jsonObject.ToString());
                return settings;
            }
            catch (System.Exception ex) { Console.WriteLine(ex); }
            return null;
        }
    }
}

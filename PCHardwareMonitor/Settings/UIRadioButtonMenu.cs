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
    class UIRadioButtonMenu: StackPanel
    {
        private List<UIRadioButton> buttons = new List<UIRadioButton>();
        public int selectedIndex { get; private set; }
        public Action<int> didSelectedIndex = (int i) => { };

        public UIRadioButtonMenu(string[] titles, double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.Margin = new Thickness(0, 0, 0, 25);
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.VerticalAlignment = VerticalAlignment.Center;
            
            for(int i = 0; i < titles.Length; i++)
            {
                var title = titles[i];
                var button = new UIRadioButton(title);
                var index = Array.FindIndex<string>(titles, x => x.Contains(title));
                if (i == 0) { button.SetSelected(true); }
                button.onClick = () => {
                    selectedIndex = index;
                    didSelectedIndex(selectedIndex);
                    foreach (var otherButton in buttons)
                    {
                        if (otherButton == button) { continue; }
                        otherButton.SetSelected(false);
                    }
                };
                buttons.Add(button);
                this.Children.Add(button);
            }
        }

        public void SetButtonSelected(int index, bool selected)
        {
            if (index > buttons.Count - 1 || index < 0) { Console.WriteLine("INDEX OUT OF RANGE"); return; }
            buttons[index].SetSelected(selected);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Screen;

namespace NSprakIDE.Controls
{
    public class ScreenSupplier : ViewSupplier<ComputerScreen>
    {
        public ScreenSupplier(ViewSelect view) : base(view) { }

        public ComputerScreen Start(string id, string name, string category)
        {
            ComputerScreen value = new ComputerScreen(View.Dispatcher);
            Start(value, id, name, category);
            return value;
        }
    }


    public partial class ScreenView : UserControl
    {
        public ScreenSupplier Supplier { get; }

        public ScreenView()
        {
            InitializeComponent();

            Supplier = new ScreenSupplier(ViewSelect);

            ViewSelect.Selected += ViewSelect_Selected;
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            ComputerScreen item = (ComputerScreen)e.NewValue;

            if (item == null)
                Screen.ClearLayers();

            else
                Screen.SetLayers(item.Layers);
        }
    }
}

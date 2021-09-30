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


    public partial class ScreenView : UserControl, IViewSupplierView<ComputerScreen>
    {
        public ScreenSupplier Supplier { get; }

        ViewSupplier<ComputerScreen> IViewSupplierView<ComputerScreen>.Supplier 
            => Supplier; 

        public ScreenView()
        {
            InitializeComponent();

            Supplier = new ScreenSupplier(ViewSelect);

            ViewSelect.Selected += ViewSelect_Selected;
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            ComputerScreen newScreen = (ComputerScreen)e.NewValue;
            ComputerScreen oldScreen = (ComputerScreen)e.OldValue;

            UpdateClearButton();

            if (oldScreen != null)
                oldScreen.HasContentChanged -= Screen_OnHasContentChanged;

            if (newScreen == null)
                Screen.ClearLayers();

            else
            {
                newScreen.HasContentChanged += Screen_OnHasContentChanged;
                Screen.SetLayers(newScreen.Layers);
            }  
        }

        private void Screen_OnHasContentChanged(object sender, EventArgs e)
        {
            UpdateClearButton();
        }

        private void UpdateClearButton()
        {
            ViewItem<ComputerScreen> screen = ViewSelect
                .SelectedItem as ViewItem<ComputerScreen>;

            Clear.Visibility = screen.Value.HasContent ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewItem<ComputerScreen> screen = ViewSelect
                .SelectedItem as ViewItem<ComputerScreen>;

            screen?.Value.ClearText();
        }
    }
}

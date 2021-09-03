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

using Microsoft.Extensions.Logging;

namespace NSprakIDE.Controls.General
{
    public partial class CommandButton2 : UserControl
    {
        public static DependencyProperty IconNameProperty = DependencyProperty.Register(
            "IconName", typeof(string), typeof(CommandButton2), new PropertyMetadata(OnIconNameChanged));

        public static DependencyProperty UICommandProperty = DependencyProperty.Register(
            "UICommand", typeof(RoutedUICommand), typeof(CommandButton2), new PropertyMetadata(OnCommandChanged));

        public string IconName
        {
            get => (string)GetValue(IconNameProperty);
            set => SetValue(IconNameProperty, value);
        }

        public RoutedUICommand UICommand
        {
            get => (RoutedUICommand)GetValue(UICommandProperty);
            set => SetValue(UICommandProperty, value);
        }

        private static void OnIconNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton button = (CommandButton)sender;

            string resourceName = "Icon_" + button.IconName;
            string largeResourceName = "LargeIcon_" + button.IconName;

            Logs.Core.LogInformation("Hello From Icon Changed!");
            Logs.Core.LogDebug(resourceName);

            //button.Icon = button.TryFindResource(resourceName);
            //button.LargeIcon = button.TryFindResource(largeResourceName);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton button = (CommandButton)sender;

            button.Command = button?.UICommand;
            button.Header = button?.UICommand.Text;
        }

        public CommandButton2()
        {
            InitializeComponent();
        }
    }
}

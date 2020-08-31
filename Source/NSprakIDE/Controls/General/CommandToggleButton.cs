using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace NSprakIDE.Controls.General
{
    public class CommandToggleButton : Fluent.ToggleButton
    {
        public static DependencyProperty IconNameProperty = DependencyProperty.Register(
            "IconName", typeof(string), typeof(CommandToggleButton), new PropertyMetadata(OnIconNameChanged));

        public static DependencyProperty UICommandProperty = DependencyProperty.Register(
            "UICommand", typeof(RoutedUICommand), typeof(CommandToggleButton), new PropertyMetadata(OnCommandChanged));

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
            CommandToggleButton button = (CommandToggleButton)sender;

            string resourceName = "Icon_" + button.IconName;
            string largeResourceName = "LargeIcon_" + button.IconName;

            button.Icon = button.TryFindResource(resourceName);
            button.LargeIcon = button.TryFindResource(largeResourceName);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandToggleButton button = (CommandToggleButton)sender;

            button.Command = button?.UICommand;
            button.Header = button?.UICommand.Text;
        }

        public CommandToggleButton()
        {
            Size = Fluent.RibbonControlSize.Middle;
        }
    }
}

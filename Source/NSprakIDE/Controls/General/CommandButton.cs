using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace NSprakIDE.Controls.General
{
    public class CommandButton : Fluent.Button
    {
        public static DependencyProperty IconNameProperty = DependencyProperty.Register(
            "IconName", typeof(string), typeof(CommandButton), new PropertyMetadata(OnIconNameChanged));

        public static DependencyProperty UICommandProperty = DependencyProperty.Register(
            "UICommand", typeof(RoutedUICommand), typeof(CommandButton), new PropertyMetadata(OnCommandChanged));

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

            button.Icon = button.TryFindResource(resourceName);
            button.LargeIcon = button.TryFindResource(largeResourceName);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton button = (CommandButton)sender;

            button.Command = button?.UICommand;
            button.Header = button?.UICommand.Text;
        }

        public CommandButton()
        {
            Size = Fluent.RibbonControlSize.Middle;
        }
    }
}

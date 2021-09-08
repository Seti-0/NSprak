using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NSprakIDE.Controls.General
{
    public enum CommandButtonSize
    {
        Medium, Large
    }

    public class CommandButton3 : Button
    {
        public static DependencyProperty SizeProperty = 
            DependencyProperty.Register(
                nameof(Size), 
                typeof(CommandButtonSize), 
                typeof(CommandButton3), 
                new PropertyMetadata(OnSizeChanged)
                );

        public static DependencyProperty IconNameProperty = 
            DependencyProperty.Register(
                nameof(IconName), 
                typeof(string), 
                typeof(CommandButton3), 
                new PropertyMetadata(OnIconNameChanged)
                );

        public static DependencyProperty UICommandProperty = 
            DependencyProperty.Register(
                nameof(UICommand), 
                typeof(RoutedUICommand), 
                typeof(CommandButton3), 
                new PropertyMetadata(OnCommandChanged)
                );

        public static DependencyProperty ImageSourceProperty = 
            DependencyProperty.Register(
                nameof(ImageSource), 
                typeof(object), 
                typeof(CommandButton3)
                );

        public static DependencyProperty CommandEnabledProperty =
            DependencyProperty.Register(
                nameof(CommandEnabled),
                typeof(bool),
                typeof(CommandButton3)
                );

        private EventHandler _enabledHandler;

        public CommandButtonSize Size
        {
            get => (CommandButtonSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

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

        public object ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public bool CommandEnabled
        {
            get => (bool)GetValue(CommandEnabledProperty);
            set => SetValue(CommandEnabledProperty, value);
        }

        private static void OnSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton3 button = (CommandButton3)sender;
            UpdateImageSource(button);
        }

        private static void OnIconNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton3 button = (CommandButton3)sender;
            UpdateImageSource(button);
        }

        private static void UpdateImageSource(CommandButton3 target)
        {
            if (target.IconName == null)
                return;

            string resourceName = "Icon_" + target.IconName;
            if (target.Size == CommandButtonSize.Large)
                resourceName = "Large" + resourceName;

            object source = target.FindResource(resourceName);
            target.ImageSource = source;
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CommandButton3 button = (CommandButton3)sender;

            if (button._enabledHandler != null)
                button.Command.CanExecuteChanged -= button._enabledHandler;

            button.Command = button?.UICommand;
            button.Content = button?.UICommand?.Text;
            button.CommandEnabled = button.Command.CanExecute(null);

            void EnabledHandler(object sender, EventArgs e)
            {
                // Passing the button as the target element for CanExecute here
                // is important - it means that CanExecute will not consider 
                // the element that is actually focused. This is good, because
                // the button should not be greyed out just because the parent
                // element is out of focus, that's only relevant to keyboard 
                // shortcuts. Which makes me think that there might be another
                // method I'm missing.
                button.CommandEnabled = button.UICommand.CanExecute(null, button);
            }

            button._enabledHandler = EnabledHandler;
            button.Command.CanExecuteChanged += EnabledHandler;
        }

        public CommandButton3()
        {
            Size = CommandButtonSize.Medium;
        }
    }
}

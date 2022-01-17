using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Controls.Primitives;

namespace NSprakIDE.Controls.General
{
    public class FlatToggleButton : ToggleButton
    {
        public static DependencyProperty IconNameProperty =
            DependencyProperty.Register(
                nameof(IconName),
                typeof(string),
                typeof(FlatToggleButton),
                new PropertyMetadata(OnIconNameChanged)
                );

        public static DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(object),
                typeof(FlatToggleButton)
                );

        private static void OnIconNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FlatToggleButton button = (FlatToggleButton)sender;
            UpdateImageSource(button);
        }

        private static void UpdateImageSource(FlatToggleButton target)
        {
            if (target.IconName == null)
                return;

            string resourceName = "Icon_" + target.IconName;
            object source = target.FindResource(resourceName);
            target.ImageSource = source;
        }

        public string IconName
        {
            get => (string)GetValue(IconNameProperty);
            set => SetValue(IconNameProperty, value);
        }

        public object ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
    }
}

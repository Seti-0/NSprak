using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace NSprakIDE.Commands
{
    public static class CommandHelper
    {
        public static ICommand CreateCommand(string name, Key key = Key.None, ModifierKeys modifiers = ModifierKeys.None)
        {
            return CreateCommand(name, name, key, modifiers);
        }

        public static ICommand CreateCommand(string name, string userText, Key key = Key.None, ModifierKeys modifiers = ModifierKeys.None)
        {
            InputGestureCollection gestures = new InputGestureCollection { new KeyGesture(key, modifiers) };
            return new RoutedUICommand(userText, name, typeof(CommandHelper), gestures);
        }

        public static void Bind(UIElement element,  ICommand command, Action action, Func<bool> canExecute = null)
        {
            canExecute ??= () => true;

            void ExecuteHandler(object sender, ExecutedRoutedEventArgs e)
            {
                action();
            }

            void CanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = canExecute();
            }

            element.CommandBindings.Add(new CommandBinding(command, ExecuteHandler, CanExecuteHandler));
        }
    }
}

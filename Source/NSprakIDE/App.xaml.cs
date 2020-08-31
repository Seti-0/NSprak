using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Xml;
using ControlzEx.Theming;
using NSprakIDE.Logging;

namespace NSprakIDE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Log.Outputs.Add(new DirectOutput(new FileWriter("log.txt")));
            Log.Begin();

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            ThemeManager.Current.DetectTheme();

            Log.Core.Important("Hello world!");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void HandleException(Exception e)
        {
            Log.Core.Error("Unexpected error occured", e);
            
            if (e == null)
                // I'm not sure if this can happen. It seems possible from the CurrentDomain callback.
                Log.Core.Debug("Note to dev: Exception is null. This probably a breakpoint in App.cs is the only starting point?");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.End();
        }
    }
}

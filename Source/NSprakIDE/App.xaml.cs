using System;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Logging;

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

            Logs.Core.LogInformation("==== Beginning of Log ====");

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
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
            Logs.Core.LogError("Unexpected error occured", e);
            
            if (e == null)
                // I'm not sure if this can happen. It seems possible from the CurrentDomain callback.
                Logs.Core.LogDebug("Note to dev: Exception is null. This probably a breakpoint in App.cs is the only starting point?");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logs.Core.LogInformation("==== End of Log ====");
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Windows;

// Imported
using Serilog;

namespace Xenia_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // This is needed for Console to show up when using argument -console
        [DllImport("Kernel32")]
        public static extern void AllocConsole();
        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        private bool logging = false;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Serilog.Log.Logger = Log.Logger; // Initializing Logger
                                             // Checks for all of the Launch Arguments
            if (e.Args.Contains("-console"))
            {
                AllocConsole();
                logging = true;
            }
            // Creating Logger Configuration
            if (logging)
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss}|{Level}|{Message}{NewLine}{Exception}")
                //.WriteTo.File("Logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss}|{Level}|{Message}{NewLine}{Exception}")
                //.WriteTo.File("Logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            }

            Log.Information("App Loaded");
        }
    }
}

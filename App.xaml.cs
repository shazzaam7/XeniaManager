using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;

// Imported
using Serilog;
using Xenia_Manager.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xenia_Manager.Classes;
using Newtonsoft.Json;

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

        /// <summary>
        /// Toggle for logging to a file
        /// </summary>
        private bool logging = false;

        /// <summary>
        /// This is where the Manager Executable is
        /// </summary>
        public static string InstallationDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// This is where config.json is loaded
        /// </summary>
        public static AppConfiguration? appConfig;

        /// <summary>
        /// Used to load config.json
        /// </summary>
        private async Task LoadConfigurationFile()
        {
            try
            {
                Log.Information("Trying to load config.json");
                if (File.Exists(InstallationDirectory + @"config.json"))
                {
                    string json = File.ReadAllText(InstallationDirectory + @"config.json");
                    appConfig = JsonConvert.DeserializeObject<AppConfiguration>(json);
                    Log.Information("config.json loaded.");
                }
                else
                {
                    Log.Warning("config.json not found. (Could be fresh install)");
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                return;
            }
        }

        /// <summary>
        /// Everything that happens on startup
        /// </summary>
        private async void Application_Startup(object sender, StartupEventArgs e)
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

            Log.Information("Loading the config.json");
            await LoadConfigurationFile();

            await Task.Delay(1);
            Log.Information("App Loaded");
        }
    }
}

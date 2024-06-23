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
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Globalization;

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
        /// Function that grabs the download link of the selected build
        /// </summary>
        /// <param name="url">URL of the builds releases page API</param>
        /// <returns></returns>
        private async Task CheckForUpdates()
        {
            try
            {
                Log.Information("Checking for updates.");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                    HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/xenia-canary/xenia-canary/releases");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JArray releases = JArray.Parse(json);

                        if (releases.Count > 0)
                        {
                            JObject latestRelease = (JObject)releases[0];
                            JArray assets = (JArray)latestRelease["assets"];
                            int id = (int)latestRelease["id"];
                            string releaseDate = (string)latestRelease["published_at"];
                            if (id != appConfig.VersionID)
                            {
                                Log.Information("Found newer version of Xenia");
                                MessageBoxResult result = MessageBox.Show("Found a new version of Xenia. Do you want to update Xenia?", "Confirmation", MessageBoxButton.YesNo);
                                if (result == MessageBoxResult.Yes)
                                {
                                    Log.Information("ID of the build: " + id.ToString());
                                    if (assets.Count > 0)
                                    {
                                        JObject firstAsset = (JObject)assets[0];
                                        string downloadUrl = firstAsset["browser_download_url"].ToString();
                                        Log.Information("Download link of the build: " + downloadUrl);
                                        DownloadManager downloadManager = new DownloadManager(null, downloadUrl, AppDomain.CurrentDomain.BaseDirectory + @"\xenia.zip");
                                        Log.Information("Downloading the latest Xenia Canary build.");
                                        await downloadManager.DownloadAndExtractAsync();
                                        Log.Information("Downloading and extraction of the latest Xenia Canary build done.");
                                        Log.Information("Updating configuration file.");
                                        appConfig.VersionID = id;
                                        appConfig.ReleaseDate = DateTime.ParseExact(releaseDate, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        appConfig.LastUpdateCheckDate = DateTime.Now;
                                        appConfig.SaveChanges(App.InstallationDirectory + @"config.json");
                                        Log.Information("Xenia has been updated to the latest build.");
                                        MessageBox.Show("Xenia has been updated to the latest build.");
                                    }
                                }
                                else
                                {
                                    appConfig.LastUpdateCheckDate = DateTime.Now;
                                    appConfig.SaveChanges(App.InstallationDirectory + @"config.json");
                                }
                            }
                            else
                            {
                                appConfig.LastUpdateCheckDate = DateTime.Now;
                                appConfig.SaveChanges(App.InstallationDirectory + @"config.json");
                                Log.Information("Latest version is already installed.");
                            }
                        }
                        else
                        {
                            appConfig.LastUpdateCheckDate = DateTime.Now;
                            appConfig.SaveChanges(App.InstallationDirectory + @"config.json");
                            Log.Error("No releases found.");
                        }
                    }
                    else
                    {
                        appConfig.LastUpdateCheckDate = DateTime.Now;
                        appConfig.SaveChanges(App.InstallationDirectory + @"config.json");
                        Log.Error("Failed to retrieve releases. Status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
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

            if (appConfig != null && (appConfig.LastUpdateCheckDate == null || (DateTime.Now - appConfig.LastUpdateCheckDate.Value).TotalDays >= 1))
            {
                await CheckForUpdates();
            }
            await Task.Delay(1);
            Log.Information("App Loaded");
        }
    }
}

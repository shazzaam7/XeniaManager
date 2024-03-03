using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

// Imported
using Serilog;
using Xenia_Manager.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.Diagnostics;

namespace Xenia_Manager.Windows
{
    /// <summary>
    /// Interaction logic for WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        /// <summary>
        /// Stores the downloadURL of the Xenia Build
        /// </summary>
        private string downloadUrl = "";

        /// <summary>
        /// Stores Xenia Build ID
        /// </summary>
        private int id = 0;

        /// <summary>
        /// Stores release date of the Xenia Build
        /// </summary>
        private string releaseDate = "";

        public WelcomeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Function that grabs the download link of the selected build
        /// </summary>
        /// <param name="url">URL of the builds releases page API</param>
        /// <returns></returns>
        private async Task GrabbingDownloadLink(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information("Got the response from the API. Parsing the JSON file.");
                        string json = await response.Content.ReadAsStringAsync();
                        JArray releases = JArray.Parse(json);

                        if (releases.Count > 0)
                        {
                            Log.Information("Found a release of Xenia.");
                            JObject latestRelease = (JObject)releases[0];
                            JArray assets = (JArray)latestRelease["assets"];
                            id = (int)latestRelease["id"];
                            Log.Information("ID of the build: " + id.ToString());
                            releaseDate = (string)latestRelease["published_at"];
                            Log.Information("Release date: " + releaseDate);
                            if (assets.Count > 0)
                            {
                                JObject firstAsset = (JObject)assets[0];
                                downloadUrl = firstAsset["browser_download_url"].ToString();
                                Log.Information("Download link of the build: " + downloadUrl);
                            }
                            else
                            {
                                Log.Error("No assets found for the first release.");
                            }
                        }
                        else
                        {
                            Log.Error("No releases found.");
                        }
                    }
                    else
                    {
                        Log.Error("Failed to retrieve releases. Status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                return;
            }
        }

        /// <summary>
        /// Generates Xenia's configuration file
        /// </summary>
        /// <param name="executablePath">Path to the Xenia executable</param>
        /// <param name="configPath">Path to the Xenia configuration file</param>
        /// <returns></returns>
        private async Task GenerateConfigFile(string executablePath, string configPath)
        {
            try
            {
                Log.Information("Generating xenia-canary.config.toml by launching the emulator.");
                Process xenia = new Process();
                xenia.StartInfo.FileName = executablePath;
                xenia.Start();
                Log.Information("Emulator Launched");
                Log.Information("Waiting for configuration file to be generated.");
                while (!File.Exists(configPath))
                {
                    await Task.Delay(100);
                }
                Log.Information("Configuration file found. Closing the emulator.");
                xenia.CloseMainWindow();
                xenia.Close();
                xenia.Dispose();
                Log.Information("Emulator closed.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                return;
            }
        }

        /// <summary>
        /// Installs the Xenia Stable build
        /// </summary>
        private async void InstallXeniaStable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("To be implemented.");
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
        /// Installs the Xenia Canary build
        /// </summary>
        private async void InstallXeniaCanary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("Grabbing the link to the latest Xenia Canary build.");
                await GrabbingDownloadLink("https://api.github.com/repos/xenia-canary/xenia-canary/releases");

                DownloadManager downloadManager = new DownloadManager(Progress, downloadUrl, AppDomain.CurrentDomain.BaseDirectory + @"\xenia.zip");
                Log.Information("Downloading the latest Xenia Canary build.");
                await downloadManager.DownloadAndExtractAsync();
                Log.Information("Downloading and extraction of the latest Xenia Canary build done.");
                Log.Information("Creating a JSON configuration file for the Xenia Manager.");

                // Saving Configuration File as a JSON
                App.appConfig = new AppConfiguration
                {
                    VersionID = id,
                    Branch = "Canary",
                    ReleaseDate = DateTime.Parse(releaseDate),
                    EmulatorLocation = AppDomain.CurrentDomain.BaseDirectory + @"Xenia\",
                    ConfigurationFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Xenia\xenia-canary.config.toml",
                    ExecutableFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Xenia\xenia_canary.exe"
                };
                string json = JsonConvert.SerializeObject(App.appConfig);

                // Write the JSON to a file
                File.WriteAllText(App.InstallationDirectory + @"config.json", json);
                Log.Information("Done. Generating Xenia configuration by running it.");
                await GenerateConfigFile(App.appConfig.EmulatorLocation + @"xenia_canary.exe", App.appConfig.EmulatorLocation + @"\xenia-canary.config.toml");
                Log.Information("Done.");
                MessageBox.Show("Xenia Canary installed");
                this.Close();
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                return;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            this.Topmost = false;
        }
    }
}

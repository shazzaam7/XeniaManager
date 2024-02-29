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

        /// <summary>
        /// Instance of Download Manager Class
        /// </summary>
        private DownloadManager Downloader = new DownloadManager();

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
                    // Set GitHub API headers
                    client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JArray releases = JArray.Parse(json);

                        if (releases.Count > 0)
                        {
                            JObject latestRelease = (JObject)releases[0];
                            JArray assets = (JArray)latestRelease["assets"];
                            id = (int)latestRelease["id"];
                            releaseDate = (string)latestRelease["published_at"];
                            if (assets.Count > 0)
                            {
                                JObject firstAsset = (JObject)assets[0];
                                downloadUrl = firstAsset["browser_download_url"].ToString();
                                Log.Information("Download link: " + downloadUrl);
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
                return;
            }
        }
        
        /// <summary>
        /// Updates the ProgressBar when downloading Xenia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progress"></param>
        private void Downloader_ProgressChanged(object sender, int progress)
        {
            // Update progress bar
            Progress.Value = progress;
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

        private async void InstallXeniaCanary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // AppDomain.CurrentDomain.BaseDirectory - To grab where the app is really stored
                // Grabbing the link to the latest Xenia Canary Build
                await GrabbingDownloadLink("https://api.github.com/repos/xenia-canary/xenia-canary/releases");

                // Downloading the latest Xenia Canary build and updating ProgressBar
                Downloader.ProgressChanged += Downloader_ProgressChanged;
                await Downloader.DownloadBuild(downloadUrl, AppDomain.CurrentDomain.BaseDirectory);
                
                // Saving Configuration File as a JSON
                AppConfiguration appConfig = new AppConfiguration{
                    VersionID = id,
                    Branch = "Canary",
                    ReleaseDate = DateTime.Parse(releaseDate),
                    EmulatorLocation = AppDomain.CurrentDomain.BaseDirectory
                };
                string json = JsonConvert.SerializeObject(appConfig);

                // Write the JSON to a file
                File.WriteAllText("config.json", json);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                return;
            }
        }
    }
}

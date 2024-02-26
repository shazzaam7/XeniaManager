using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

// Imported
using Xenia_Manager.Classes;
using Newtonsoft.Json.Linq;

namespace Xenia_Manager.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            DownloadTest();
        }

        private async void DownloadTest()
        {
            Xenia xenia = new Xenia(143495069);
            string url = "https://api.github.com/repos/xenia-canary/xenia-canary/releases";
            string downloadUrl = "";
            int id = 0;
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
                        JObject firstRelease = (JObject)releases[0];
                        JArray assets = (JArray)firstRelease["assets"];
                        id = (int)firstRelease["id"];
                        if (assets.Count > 0)
                        {
                            JObject firstAsset = (JObject)assets[0];
                            downloadUrl = firstAsset["browser_download_url"].ToString();
                            Console.WriteLine("First asset download URL: " + downloadUrl);
                        }
                        else
                        {
                            Console.WriteLine("No assets found for the first release.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No releases found.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieve releases. Status code: " + response.StatusCode);
                }
            }
            if (id > 143495069)
            {
                xenia.ProgressChanged += Downloader_ProgressChanged;
                await xenia.DownloadFileAsync(downloadUrl, System.IO.Directory.GetCurrentDirectory() + @"\xenia_canary.zip");
            }
        }

        private void Downloader_ProgressChanged(object sender, int progress)
        {
            // Update progress bar
            Progress.Value = progress;
        }
    }
}

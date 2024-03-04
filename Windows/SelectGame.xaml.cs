using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// Imported
using Serilog;
using Newtonsoft.Json;
using Xenia_Manager.Classes;
using ImageMagick;
using Microsoft.Extensions.DependencyModel;
using Xenia_Manager.Pages;

namespace Xenia_Manager.Windows
{
    /// <summary>
    /// Interaction logic for SelectGame.xaml
    /// </summary>
    public partial class SelectGame : Window
    {
        public SelectGame()
        {
            InitializeComponent();
            InitializeAsync();
        }

        public SelectGame(Xenia_Manager.Pages.Library library,string selectedGameFilePath)
        {
            InitializeComponent();
            this.GameFilePath = selectedGameFilePath;
            _library = library;
            InitializeAsync();
        }

        public SelectGame(Xenia_Manager.Pages.Library library, string selectedGame, string selectedGameFilePath)
        {
            InitializeComponent();
            if (selectedGame != null)
            {
                this.gameTitle = selectedGame;
            }
            this.GameFilePath = selectedGameFilePath;
            _library = library;
            InitializeAsync();
        }

        LoadingWindow loadingWindow = new LoadingWindow();
        List<GameInfo> games = new List<GameInfo>();
        private List<string> filteredGameTitles = new List<string>();
        private string gameTitle = "";
        private string GameFilePath = "";
        private Xenia_Manager.Pages.Library _library;


        private async void InitializeAsync()
        {
            try
            {
                loadingWindow.Show();
                await ReadGames();
                loadingWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task ReadGames()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/shazzaam7/XeniaManager/master/Wikipedia%20Scraper/All%20xBox%20360%20Titles.json";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();

                            games = JsonConvert.DeserializeObject<List<GameInfo>>(json);
                            foreach (var game in games)
                            {
                                Games.Items.Add(game.Title);
                            }
                            SearchBox.Text = gameTitle;
                        }
                        else
                        {
                            Log.Error($"Failed to load data. Status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, "");
                        MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task GetGameIcon(string imageUrl, string outputPath, int width = 132, int height = 198)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "CoolBot/0.0 (https://example.org/coolbot/; coolbot@example.org) generic-library/0.0"); // Add custom User-Agent header

                    byte[] imageData = webClient.DownloadData(imageUrl);

                    using (MemoryStream memoryStream = new MemoryStream(imageData))
                    {
                        using (var magickImage = new MagickImage(memoryStream))
                        {
                            double aspectRatio = (double)width / height;
                            magickImage.Resize(width, height);

                            double imageRatio = (double)magickImage.Width / magickImage.Height;
                            int newWidth, newHeight, offsetX, offsetY;

                            if (imageRatio > aspectRatio)
                            {
                                newWidth = width;
                                newHeight = (int)Math.Round(width / imageRatio);
                                offsetX = 0;
                                offsetY = (height - newHeight) / 2;
                            }
                            else
                            {
                                newWidth = (int)Math.Round(height * imageRatio);
                                newHeight = height;
                                offsetX = (width - newWidth) / 2;
                                offsetY = 0;
                            }

                            // Create a canvas with black background
                            using (var canvas = new MagickImage(MagickColors.Black, width, height))
                            {
                                // Composite the resized image onto the canvas
                                canvas.Composite(magickImage, offsetX, offsetY, CompositeOperator.SrcOver);

                                // Convert to ICO format
                                canvas.Format = MagickFormat.Ico;
                                canvas.Write(outputPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async void Games_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // Get the clicked item
                    var listBox = sender as ListBox;
                    if (listBox != null && listBox.SelectedItem != null)
                    {
                        var selectedItem = listBox.SelectedItem.ToString();
                        GameInfo selectedGame = games.FirstOrDefault(game => game.Title == selectedItem);
                        if (selectedGame != null)
                        {
                            Log.Information($"Selected Game: {selectedGame.Title}");
                            Log.Information(selectedGame.ImageUrl);
                            await GetGameIcon(selectedGame.ImageUrl, @$"{App.InstallationDirectory}Icons\{selectedGame.Title.Replace(":", " -")}.ico");
                            Game newGame = new Game();
                            newGame.Title = selectedGame.Title.Replace(":", " -");
                            newGame.CoverImage = App.InstallationDirectory + @"Icons\" + selectedGame.Title.Replace(":", " -") + ".ico";
                            newGame.GameLocation = GameFilePath;
                            _library.Games.Add(newGame);
                            this.Close();
                        }
                    }
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchBox.Text.ToLower();
            filteredGameTitles = games.Where(game => game.Title.ToLower().Contains(searchQuery)).Select(game => game.Title).ToList();
            UpdateListBox();
            await Task.Delay(1);
        }

        private void UpdateListBox()
        {
            Games.ItemsSource = null;
            Games.Items.Clear();
            Games.ItemsSource = filteredGameTitles;
        }
    }
}

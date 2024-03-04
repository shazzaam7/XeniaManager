using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xenia_Manager.Classes;

namespace Xenia_Manager.Windows
{
    /// <summary>
    /// Interaction logic for SelectGamePatch.xaml
    /// </summary>
    public partial class SelectGamePatch : Window
    {
        public SelectGamePatch()
        {
            InitializeComponent();
        }

        public SelectGamePatch(SelectGame selectGame)
        {
            InitializeComponent();
            _selectGame = selectGame;
            InitializeAsync();
        }

        LoadingWindow loadingWindow = new LoadingWindow();
        private Xenia_Manager.Windows.SelectGame _selectGame;
        List<GamePatches> patches = new List<GamePatches>();
        private List<string> filteredPatches = new List<string>();

        private async void InitializeAsync()
        {
            try
            {
                loadingWindow.Show();
                await ReadGamePatches();
                loadingWindow.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task ReadGamePatches()
        {
            try
            {
                string url = "https://api.github.com/repos/xenia-canary/game-patches/contents/patches";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        patches = JsonConvert.DeserializeObject<List<GamePatches>>(json);
                        foreach (GamePatches patch in patches)
                        {
                            Patches.Items.Add(patch.name);
                        }
                        SearchBox.Text = _selectGame.newGame.Title;
                    }
                    else
                    {
                        Log.Error($"Failed to fetch folder contents. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task PatchDownloader(string url, string savePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            byte[] content = await response.Content.ReadAsByteArrayAsync();
                            await System.IO.File.WriteAllBytesAsync(savePath, content);
                            Log.Information("Patch successfully downloaded");
                        }
                        else
                        {
                            Log.Error($"Failed to download file. Status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"An error occurred: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async void Patches_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    var listBox = sender as ListBox;
                    if (listBox != null && listBox.SelectedItem != null)
                    {
                        var selectedItem = listBox.SelectedItem.ToString();
                        GamePatches selectedPatch = patches.FirstOrDefault(patch => patch.name == listBox.SelectedItem.ToString());
                        if (selectedPatch != null)
                        {
                            Log.Information($"Selected Patch: {selectedPatch}");
                            Log.Information(selectedPatch.download_url);
                            await PatchDownloader(selectedPatch.download_url, App.appConfig.EmulatorLocation + @"patches\" + selectedPatch.name);
                            _selectGame.newGame.PatchLocation = App.appConfig.EmulatorLocation + @"patches\" + selectedPatch.name;
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
            filteredPatches = patches.Where(patch => patch.name.ToLower().Contains(searchQuery)).Select(patch => patch.name).ToList();
            UpdateListBox();
            await Task.Delay(1);
        }

        private void UpdateListBox()
        {
            Patches.ItemsSource = null;
            Patches.Items.Clear();
            Patches.ItemsSource = filteredPatches;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

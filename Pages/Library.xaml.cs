using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

// Imported
using Serilog;
using Microsoft.Win32;
using Xenia_Manager.Classes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Xenia_Manager.Windows;
using Newtonsoft.Json;
using System.IO;


namespace Xenia_Manager.Pages
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {

        public ObservableCollection<Game> Games = new ObservableCollection<Game> ();

        public Library()
        {
            InitializeComponent();
            DataContext = this;
            LoadGamesStartup();
        }

        private void LoadGamesStartup()
        {
            try
            {
                if (File.Exists(App.InstallationDirectory + @"installedGames.json"))
                {
                    wrapPanel.Children.Clear();
                    string JSON = File.ReadAllText(App.InstallationDirectory + @"installedGames.json");
                    Games = JsonConvert.DeserializeObject<ObservableCollection<Game>>((JSON));
                    LoadGamesIntoUI();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
            }
        }

        private async Task LoadGames()
        {
            try
            {
                wrapPanel.Children.Clear();
                await LoadGamesIntoUI();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task LoadGamesIntoUI()
        {
            try
            {
                if (Games != null && Games.Count > 0)
                {
                    var orderedGames = Games.OrderBy(game => game.Title);
                    foreach (var game in orderedGames)
                    {
                        var button = new Button();
                        var image = new Image
                        {
                            Source = new BitmapImage(new Uri(game.CoverImage)),
                            Stretch = Stretch.UniformToFill
                        };

                        var border = new Border
                        {
                            CornerRadius = new CornerRadius(20),
                            Child = image
                        };

                        button.Content = border;
                        button.Click += (sender, e) => GameCover_Click(game);
                        button.Cursor = Cursors.Hand;
                        button.Style = (Style)FindResource("GameCoverButtons");
                        button.ToolTip = game.Title;
                        wrapPanel.Children.Add(button);
                        button.Loaded += (sender, e) =>
                        {
                            button.Width = 132;
                            button.Height = 198;
                            button.Margin = new Thickness(5);

                            // Create the context menu
                            ContextMenu contextMenu = new ContextMenu();

                            // Create menu items
                            MenuItem EditGame = new MenuItem();
                            EditGame.Header = "Edit game";
                            EditGame.Click += (sender, e) => EditGame_Click(game);

                            MenuItem RemoveGame = new MenuItem();
                            RemoveGame.Header = "Remove game";
                            RemoveGame.Click += (sender, e) => RemoveGame_Click(game);

                            contextMenu.Items.Add(EditGame);
                            contextMenu.Items.Add(RemoveGame);

                            // Assign context menu to button
                            button.ContextMenu = contextMenu;
                        };
                    }
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async void EditGame_Click(Game game)
        {
            try
            {
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async void RemoveGame_Click(Game game)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show($"Do you want to remove {game.Title}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Games.Remove(game);
                    await LoadGames();
                    await SaveGames();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }


        private async void GameCover_Click(Game game)
        {
            try
            {
                Process xenia = new Process();
                xenia.StartInfo.FileName = App.appConfig.ExecutableFilePath;
                xenia.StartInfo.Arguments = $@"""{game.GameLocation}"" --fullscreen";
                xenia.Start();
                Log.Information("Emulator started.");
                xenia.WaitForExitAsync();
                Log.Information("Emulator closed.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async Task SaveGames()
        {
            try
            {
                string JSON = JsonConvert.SerializeObject(Games, Formatting.Indented);
                File.WriteAllText(App.InstallationDirectory + @"installedGames.json", JSON);
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private async void AddGame_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select a game";
                openFileDialog.Filter = "Supported Files|*.iso;*.xex;*.zar|ISO Files (*.iso)|*.iso|XEX Files (*.xex)|*.xex|ZAR Files (*.zar)|*.zar";
                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    Log.Information(selectedFilePath);

                    Log.Information("Launching game with Xenia to find the name of the game.");
                    Process xenia = new Process();
                    xenia.StartInfo.FileName = App.appConfig.ExecutableFilePath;
                    xenia.StartInfo.Arguments = $@"""{openFileDialog.FileName}"" --fullscreen";
                    xenia.Start();
                    xenia.WaitForInputIdle();

                    string test = "Xenia-canary (canary_experimental@e0f0dc7f3 on Dec 23 2023)";
                    string gameTitle = "";
                    string gameVersion = "";

                    Process process = Process.GetProcessById(xenia.Id);
                    while (process.MainWindowTitle.Length <= (test.Length))
                    {
                        process = Process.GetProcessById(xenia.Id);
                        await Task.Delay(1000);
                    }
                    Log.Information($"Xenia Window Title: {xenia.MainWindowTitle}");

                    Log.Information("Trying first method to find the game title from Xenia Window Title.");
                    Regex versionRegex = new Regex(@"\[([A-Z0-9]+)\s+v\d+\.\d+\]");
                    Regex gameNameRegex = new Regex(@"\]\s+(.+)\s+<");

                    Match versionMatch = versionRegex.Match(xenia.MainWindowTitle);
                    gameVersion = versionMatch.Success ? versionMatch.Groups[1].Value : "Not found";
                    Match gameNameMatch = gameNameRegex.Match(xenia.MainWindowTitle);
                    gameTitle = gameNameMatch.Success ? gameNameMatch.Groups[1].Value : "Not found";

                    if (gameTitle == "" || gameTitle == "Not found")
                    {
                        Log.Information("First method failed.");
                        Log.Information("Trying second method to find the game title from Xenia Window Title.");

                        string pattern = @"\[(?<version>[^\]]+)\]\s(?<gameName>[^\<]+)";
                        Match match = Regex.Match(xenia.MainWindowTitle, pattern);

                        if (match.Success)
                        {
                            gameTitle = match.Groups["version"].Value;
                            gameTitle = match.Groups["gameName"].Value.Trim();

                            Log.Information("Game Title: " + gameTitle);
                            Log.Information("Version: " + gameVersion);
                            xenia.CloseMainWindow();
                            xenia.Close();
                            xenia.Dispose();

                            SelectGame sd = new SelectGame(this, gameTitle, selectedFilePath);
                            sd.ShowDialog();
                        }
                        else
                        {
                            Log.Information("No game title found.");
                            xenia.CloseMainWindow();
                            xenia.Close();
                            xenia.Dispose();
                            SelectGame sd = new SelectGame(this, "", selectedFilePath);
                            sd.ShowDialog();
                        }
                    }
                    else
                    {
                        Log.Information("Game title found.");
                        Log.Information("Game Title: " + gameTitle);
                        Log.Information("Version: " + gameVersion);
                        xenia.CloseMainWindow();
                        xenia.Close();
                        xenia.Dispose();

                        SelectGame sd = new SelectGame(this, gameTitle, selectedFilePath);
                        sd.ShowDialog();
                    }
                }
                await LoadGames();
                await SaveGames();
            } 
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}

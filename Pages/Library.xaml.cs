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
        /// <summary>
        /// Holds all of the imported games into the Manager
        /// </summary>
        public ObservableCollection<Game> Games = new ObservableCollection<Game> ();

        public Library()
        {
            InitializeComponent();
            DataContext = this;
            LoadGamesStartup();
        }

        /// <summary>
        /// Loads all of the games when this page is loaded
        /// </summary>
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

        /// <summary>
        /// Used to load games in general, mostly after importing another game or removing
        /// </summary>
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

        /// <summary>
        /// Loads the games into the Wrappanel
        /// </summary>
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
                        button.Click += (sender, e) => StartGame_Click(game);
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

                            if (game.PatchLocation != null)
                            {
                                MenuItem EditGamePatch = new MenuItem();
                                EditGamePatch.Header = "Edit game patch";
                                EditGamePatch.Click += (sender, e) => EditGamePatch_Click(game);
                                contextMenu.Items.Add(EditGamePatch);
                            }

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

        /// <summary>
        /// Used to edit the game details
        /// </summary>
        private async void EditGamePatch_Click(Game game)
        {
            try
            {
                Log.Information("Editing game patch.");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Used to edit the game details
        /// </summary>
        private async void EditGame_Click(Game game)
        {
            try
            {
                Log.Information("Editing game details.");
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Used for removing the game from the manager
        /// </summary>
        /// <param name="game">Game that we want to remove</param>
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

        /// <summary>
        /// Launches the game and waits for it to close
        /// </summary>
        /// <param name="game">Game we want to launch</param>
        private async void StartGame_Click(Game game)
        {
            try
            {
                Process xenia = new Process();
                xenia.StartInfo.FileName = App.appConfig.ExecutableFilePath;
                xenia.StartInfo.Arguments = $@"""{game.GameLocation}"" --fullscreen";
                xenia.Start();
                Log.Information("Emulator started.");
                await xenia.WaitForExitAsync();
                Log.Information("Emulator closed.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Saves the installed games into installedGames.json
        /// </summary>
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

        /// <summary>
        /// Used to get game title from Xenia Window Title
        /// </summary>
        /// <param name="selectedFilePath">Where the selected game file is (.iso etc.)</param>
        private async Task GetGameTitle(string selectedFilePath)
        {
            try
            {
                Log.Information("Launching game with Xenia to find the name of the game.");
                Process xenia = new Process();
                xenia.StartInfo.FileName = App.appConfig.ExecutableFilePath;
                xenia.StartInfo.Arguments = $@"""{selectedFilePath}""";
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
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Opens FileDialog where user selects the game
        /// </summary>
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
                    Log.Information(openFileDialog.FileName);
                    await GetGameTitle(openFileDialog.FileName);
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

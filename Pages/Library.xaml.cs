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


namespace Xenia_Manager.Pages
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {

        public ObservableCollection<Game> Games { get; set; }

        public Library()
        {
            InitializeComponent();
            DataContext = this;
            LoadGames();
        }

        private void LoadGames()
        {
            try
            {
                wrapPanel.Children.Clear();
                /*
                foreach (var game in Games)
                {
                    var button = new Button();
                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri(game.CoverImage)),
                        Stretch = Stretch.UniformToFill // Resize image proportionally to fit the specified dimensions
                    };

                    var border = new Border
                    {
                        CornerRadius = new CornerRadius(20), // Adjust the corner radius as needed
                        Child = image
                    };

                    button.Content = border;
                    button.Click += (sender, e) => GameCover_Click(game);
                    button.Cursor = Cursors.Hand;
                    button.Style = (Style)FindResource("GameCoverButtons");
                    wrapPanel.Children.Add(button);
                    button.Loaded += (sender, e) =>
                    {
                        button.Width = 132;
                        button.Height = 198;
                        button.Margin = new Thickness(5);
                    };
                }
                */
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "");
                MessageBox.Show(ex.Message + "\nFull Error:\n" + ex);
                return;
            }
        }

        private void GameCover_Click(Game game)
        {
            Log.Information(game.Title);
        }

        private async void AddGame_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select a Game";
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
                        }
                        else
                        {
                            Log.Information("No game title found.");
                            xenia.CloseMainWindow();
                            xenia.Close();
                            xenia.Dispose();
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
    }
}

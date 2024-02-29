using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;

// Imported
using Serilog;
using Xenia_Manager.Windows;

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
        private bool logging = false;

        private void Application_Startup(object sender, StartupEventArgs e)
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
            WelcomeDialog welcomeDialog = new WelcomeDialog();
            welcomeDialog.Show();
            /*
Log.Information("App Loaded");
string dateString = "2024-02-24T20:14:27Z";
DateTime dateTime = DateTime.Parse(dateString);

Console.WriteLine(dateTime);
Process xenia = new Process();
xenia.StartInfo.FileName = @"E:\Programs\Emulators\xBox360\xenia_canary.exe";
xenia.StartInfo.Arguments = @"""E:\Games\Roms\xBox360\Red Dead Redemption\Red Dead Redemption - Game of the Year Edition (USA, Europe) (En,Fr,De,Es,It) (Disc 1) (Red Dead Redemption Single Player).iso""  --fullscreen";
xenia.Start();
xenia.WaitForInputIdle();
string test = "Xenia-canary (canary_experimental@e0f0dc7f3 on Dec 23 2023)";
Log.Information(test.Length.ToString());
Process process = Process.GetProcessById(xenia.Id);
while (process.MainWindowTitle.Length <= (test.Length))
{
    Log.Information(process.MainWindowTitle.Length.ToString());
    process = Process.GetProcessById(xenia.Id);
    await Task.Delay(1000);
}
Log.Information(xenia.MainWindowTitle);

Regex versionRegex = new Regex(@"\[([A-Z0-9]+)\s+v\d+\.\d+\]");
Regex gameNameRegex = new Regex(@"\]\s+(.+)\s+<");

// Match version
Match versionMatch = versionRegex.Match(xenia.MainWindowTitle);
string version = versionMatch.Success ? versionMatch.Groups[1].Value : "Not found";

// Match game name
Match gameNameMatch = gameNameRegex.Match(xenia.MainWindowTitle);
string gameName = gameNameMatch.Success ? gameNameMatch.Groups[1].Value : "Not found";

// Print results
Log.Information("Version: " + version);
Log.Information("Game Name: " + gameName);


// Define the regular expression pattern
string pattern = @"\[(?<version>[^\]]+)\]\s(?<gameName>[^\<]+)";

// Match the pattern in the input string
Match match = Regex.Match(xenia.MainWindowTitle, pattern);

if (match.Success)
{
    // Extract version and game name from the matched groups
    gameName = match.Groups["version"].Value;
    gameName = match.Groups["gameName"].Value.Trim();

    // Output the extracted information
    Log.Information("Version: " + version);
    Log.Information("Game Name: " + gameName);
}
else
{
    Log.Information("No match found.");
}

//https://api.github.com/repos/xenia-canary/game-patches/contents/patches Patches API
*/
        }
    }
}

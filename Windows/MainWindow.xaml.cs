using System;
using System.Windows;
using System.Windows.Input;
using System.IO;

// Imported
using Serilog;
using Xenia_Manager.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Xenia_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When the Main window loads
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(App.InstallationDirectory + @"config.json"))
                {
                    Log.Information("Confuration file not found, opening WelcomeDialog.");
                    WelcomeDialog welcomeDialog = new WelcomeDialog();
                    welcomeDialog.Topmost = true;
                    welcomeDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Used for dragging the window around
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Exits the application completely
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
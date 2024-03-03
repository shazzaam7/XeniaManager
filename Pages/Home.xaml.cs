using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

// Imported
using Xenia_Manager.Classes;
using Newtonsoft.Json.Linq;
using Serilog;

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
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Branch.Text = $"Installed Version: {App.appConfig.Branch} v{App.appConfig.VersionID}";
                await Task.Delay(1);
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

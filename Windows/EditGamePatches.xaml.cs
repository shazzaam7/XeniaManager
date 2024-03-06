using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;


// Imported
using Serilog;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace Xenia_Manager.Windows
{
    public class Patch
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Interaction logic for EditGamePatches.xaml
    /// </summary>
    public partial class EditGamePatches : Window
    {
        public EditGamePatches(string patchLocation)
        {
            InitializeComponent();
            DataContext = this;
            this.patchLocation = patchLocation;
            ReadGamePatch();
        }

        public ObservableCollection<Patch> Patches { get; set; }
        private string patchLocation;

        private void ReadGamePatch()
        {
            try
            {
                if (File.Exists(patchLocation))
                {
                    Patches = new ObservableCollection<Patch>();
                    string content = File.ReadAllText(patchLocation);
                    TomlTable model = Toml.ToModel(content);
                    Log.Information($"Game name: {model["title_name"].ToString()}");
                    Log.Information($"Game ID: {model["title_id"].ToString()}");
                    TomlTableArray patches = model["patch"] as TomlTableArray;
                    foreach (var patch in patches)
                    {
                        Patch newPatch = new Patch();
                        newPatch.Name = patch["name"].ToString();
                        newPatch.IsEnabled = bool.Parse(patch["is_enabled"].ToString());
                        if (patch.ContainsKey("desc"))
                        {
                            newPatch.Description = patch["desc"].ToString();
                        }
                        else
                        {
                            newPatch.Description = "";
                        }
                        Patches.Add(newPatch);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task SaveGamePatch()
        {
            try
            {
                if (File.Exists(patchLocation))
                {
                    string content = File.ReadAllText(patchLocation);
                    TomlTable model = Toml.ToModel(content);

                    TomlTableArray patches = model["patch"] as TomlTableArray;
                    foreach (var patch in Patches)
                    {
                        foreach (TomlTable patchTable in patches)
                        {
                            if (patchTable.ContainsKey("name") && patchTable["name"].Equals(patch.Name))
                            {
                                patchTable["is_enabled"] = patch.IsEnabled;
                                break;
                            }
                        }
                    }

                    // Serialize the TOML model back to a string
                    string updatedContent = Toml.FromModel(model);

                    // Write the updated TOML content back to the file
                    File.WriteAllText(patchLocation, updatedContent);
                    Log.Information("Patches saved successfully.");
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + "\nFull Error:\n" + ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            await SaveGamePatch();
            this.Close();
        }
    }
}

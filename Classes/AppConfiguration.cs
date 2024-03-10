using System;
using System.IO;

// Imported
using Newtonsoft.Json;
using Serilog;

namespace Xenia_Manager.Classes
{
    public class AppConfiguration
    {
        /// <summary>
        /// <para>Stable/Canary</para>
        /// </summary>
        [JsonProperty("Branch")]
        public string? Branch { get; set; }

        /// <summary>
        /// <para>"id" property from this JSON file</para>
        /// <para>Used to update the emulator</para>
        /// </summary>
        [JsonProperty("VersionID")]
        public int? VersionID { get; set; }

        /// <summary>
        /// <para>Date of publishing of the installed build</para>
        /// </summary>
        [JsonProperty("ReleaseDate")]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// <para>Date when the last check for updates was</para>
        /// </summary>
        public DateTime? LastUpdateCheckDate { get; set; }

        /// <summary>
        /// <para>Where the emulator is installed</para>
        /// </summary>
        [JsonProperty("EmulatorLocation")]
        public string? EmulatorLocation { get; set; }

        /// <summary>
        /// <para>Where the emulator configuration file is located</para>
        /// </summary>
        [JsonProperty("ConfigurationFilePath")]
        public string? ConfigurationFilePath { get; set; }

        /// <summary>
        /// <para>Where the emulator executable is located</para>
        /// </summary>
        [JsonProperty("ExecutableFilePath")]
        public string? ExecutableFilePath { get; set; }

        /// <summary>
        /// Saves the changes to a file
        /// </summary>
        /// <param name="filePath">Where the configuration file is located</param>
        public void SaveChanges(string filePath)
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(filePath, json);
        }
    }
}

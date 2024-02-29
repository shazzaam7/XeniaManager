using System;

// Imported
using Newtonsoft.Json;

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
        /// <para>Where the emulator is installed</para>
        /// </summary>
        public string? EmulatorLocation { get; set; }
    }
}

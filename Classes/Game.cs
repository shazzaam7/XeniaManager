﻿using System;

namespace Xenia_Manager.Classes
{
    public class Game
    {
        /// <summary>
        /// Name of the game
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// ID of the game
        /// </summary>
        public string? id { get; set; }

        /// <summary>
        /// Location of the Cover Image
        /// </summary>
        public string? CoverImage { get; set; }

        /// <summary>
        /// Where the game is located (ISO, etc..)
        /// </summary>
        public string? GameLocation { get; set; }

        /// <summary>
        /// Where the game patch is located (ISO, etc..)
        /// </summary>
        public string? PatchLocation { get; set; }
    }
}

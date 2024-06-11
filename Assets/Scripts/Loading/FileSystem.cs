using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Loading
{
    /// <summary>
    /// Sets up the file directories at startup and abstracts over the "real" file system.
    /// </summary>
    public static class FileSystem
    {
        /// <summary>All soundfont file extensions supported by AlphaTab.</summary>
        private static readonly string[] SoundFontFileExtensions = { ".sf2" };

        /// <summary>All score file extensions supported by AlphaTab.</summary>
        private static readonly string[] ScoreFileExtensions =
            { ".gp3", ".gp4", ".gp5", ".gpx", ".gp", ".xml", ".cap", ".txt" };

        /// <summary>Cached data folder path.</summary>
        private static string _folderPath;

        /// <summary>Lazy loaded folder path.</summary>
        private static string FolderPath
        {
            get
            {
                return _folderPath ??= Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"{Application.productName}Data");
            }
        }

        /// <summary>Cached scores folder path.</summary>
        private static string _scoresPath;

        /// <summary>Lazy loaded scores path.</summary>
        private static string ScoresPath
        {
            get { return _scoresPath ??= Path.Combine(FolderPath, "Scores"); }
        }

        /// <summary>Checks if a file's extension is valid in a given extensions set.</summary>
        private static bool ValidExtension(string filePath, IEnumerable<string> extensions) =>
            extensions.Contains(Path.GetExtension(filePath));

        /// <summary>Checks if a file's extension is a valid soundfont extension.</summary>
        private static bool ValidSoundFontExtension(string filePath) =>
            ValidExtension(filePath, SoundFontFileExtensions);

        /// <summary>Checks if a file's extension is a valid score extension.</summary>
        private static bool ValidScoreExtension(string filePath) =>
            ValidExtension(filePath, ScoreFileExtensions);

        /// <summary>
        /// Ran by Unity before the scene is loaded, sets up the file directories.
        /// </summary>
#if UNITY_STANDALONE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Initialize()
        {
            // Check if the data folder exists, if not create a new one
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            // Check if the data folder contains a soundfont file, if not copy in the default soundfont(s) included in
            // the project's assets
            if (!Directory.EnumerateFiles(FolderPath).Any(ValidSoundFontExtension))
            {
                // Copy all soundfont assets into the data directory
                var soundFonts = Resources.LoadAll<TextAsset>("SoundFonts");
                foreach (var soundFont in soundFonts)
                    File.WriteAllBytes(Path.Combine(FolderPath, $"{soundFont.name}.sf2"), soundFont.bytes);
            }

            // Check if the scores folder exists, if not create a new one
            if (!Directory.Exists(ScoresPath)) Directory.CreateDirectory(ScoresPath);

            // Check if the scores folder contains a score file, if not copy in the default scores included in the 
            // project's assets
            if (!Directory.EnumerateFiles(ScoresPath).Any(ValidScoreExtension))
            {
                // Copy all score assets into the data directory
                var scores = Resources.LoadAll<TextAsset>("Scores");
                foreach (var score in scores)
                    File.WriteAllText(Path.Combine(ScoresPath, $"{score.name}.txt"), score.text);
            }
        }

        /// <summary>Load all soundfont data from the data folder.</summary>
        public static byte[] LoadSoundFontData() =>
            File.ReadAllBytes(Directory.EnumerateFiles(FolderPath).First(ValidSoundFontExtension));

        /// <summary>Load all score names from the scores folder inside the data folder.</summary>
        public static IEnumerable<string> LoadScoreNames() => Directory.EnumerateFileSystemEntries(ScoresPath)
            .Select(Path.GetFileName)
            .Where(ValidScoreExtension);

        /// <summary>Load all score data from the score inside the data folder with the given file name.</summary>
        public static byte[] LoadScoreData(string fileName) => File.ReadAllBytes(Path.Combine(ScoresPath, fileName));
    }
}
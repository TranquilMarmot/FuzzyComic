using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FuzzyComic
{
    /// <summary>
    /// Struct containing all settings; this gets written/read from an XML file
    /// </summary>
    public struct Settings
    {
        public bool? isFullScreen { get; set; }

        public String backgroundColor { get; set; }

        public Dictionary<string, ComicInfo> comicList { get; set; }

        /// <summary>
        /// Returns the default settings, for use if no settings file exists
        /// </summary>
        /// <returns></returns>
        public static Settings Default()
        {
            var defaults = new Settings();

            defaults.isFullScreen = false;

            // See OptionsWindow class for definitions of the colors
            defaults.backgroundColor = "backgroundColorBlack";

            defaults.comicList = new Dictionary<string, ComicInfo>();

            return defaults;
        }

        /// <summary>
        /// Merge the given settings with the defaults; if any keys in the given settings don't have values,
        /// they will be set to the defaults.
        ///
        /// This is mainly for when new settings are added and aren't in the serialized settings JSON.
        /// </summary>
        /// <param name="settings">Settings to merge with defaults</param>
        /// <returns>Settings with default values for key the given settings didn't have</returns>
        public static Settings MergeWithDefaults(Settings settings)
        {
            var merged = Default();

            if (settings.isFullScreen.HasValue)
            {
                merged.isFullScreen = settings.isFullScreen;
            }

            if (settings.backgroundColor != null)
            {
                merged.backgroundColor = settings.backgroundColor;
            }

            if (settings.comicList != null)
            {
                merged.comicList = settings.comicList;
            }

            return merged;
        }
    }

    /// <summary>
    /// Utility class for reading/writing settings from a file
    /// </summary>
    public static class UserSettings
    {
        /// <summary> Current settings; this is loaded when the application starts.</summary>
        public static Settings CurrentSettings = Settings.Default();

        /// <summary> Directory at which to find the settings file </summary>
        public static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FuzzyComic");

        /// <summary> Actual path to the settings file </summary>
        public static string SettingsFilePath = Path.Combine(SettingsDirectory, "Settings.json");

        public static async Task SaveToFile()
        {
            // make sure the directory exists
            Directory.CreateDirectory(SettingsDirectory);

            using (FileStream fs = File.Create(SettingsFilePath))
            {
                await JsonSerializer.SerializeAsync<Settings>(fs, CurrentSettings);
            }
        }

        public static Settings LoadFromFile()
        {
            if (!File.Exists(SettingsFilePath))
            {
                System.Console.WriteLine($"Settings file does not exist at {SettingsFilePath}, returning default");
                return Settings.Default();
            }

            var deserialized = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFilePath));

            return Settings.MergeWithDefaults(deserialized);
        }

    }
}

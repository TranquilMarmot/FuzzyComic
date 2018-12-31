using System;
using System.Configuration;
using System.IO;

namespace FuzzyComic
{
    /// <summary>
    /// Struct containing all settings; this gets written/read from an XML file
    /// </summary>
    public struct Settings
    {
        public bool? isFullScreen;

        public String backgroundColor;

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

            return defaults;
        }

        /// <summary>
        /// Merge the given settings with the defaults; if any keys in the given settings don't have values,
        /// they will be set to the defaults.
        ///
        /// This is mainly for when new settings are added and aren't in the serialized settings XML.
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

            return merged;
        }
    }

    /// <summary>
    /// Utility class for reading/writing settings from a file
    /// </summary>
    public static class UserSettings
    {
        public static Settings CurrentSettings = Settings.Default();

        public static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FuzzyComic");

        public static string SettingsFilePath = Path.Combine(SettingsDirectory, "Settings.xml");

        public static void SaveToFile(Settings settings)
        {
            // make sure the directory exists
            Directory.CreateDirectory(SettingsDirectory);

            var writer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            using (var file = File.Create(SettingsFilePath))
            {
                writer.Serialize(file, settings);
            }
        }

        public static Settings LoadFromFile()
        {
            if (!File.Exists(SettingsFilePath))
            {
                System.Console.WriteLine("Settings file does not exist, returning default");
                return Settings.Default();
            }

            var reader = new System.Xml.Serialization.XmlSerializer(typeof(Settings));

            using (var file = File.OpenRead(SettingsFilePath))
            {
                System.Console.WriteLine("Loading settings from file...");
                return Settings.MergeWithDefaults((Settings)reader.Deserialize(file));
            }
        }

    }
}
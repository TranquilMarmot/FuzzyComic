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
        public bool isFullScreen;

        /// <summary>
        /// Returns the default settings, for use if no settings file exists
        /// </summary>
        /// <returns></returns>
        public static Settings Default()
        {
            var defaults = new Settings();

            defaults.isFullScreen = false;

            return defaults;
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
                return (Settings)reader.Deserialize(file);
            }
        }

    }
}
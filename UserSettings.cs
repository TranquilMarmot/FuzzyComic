using System;
using System.Configuration;
using System.IO;

namespace FuzzyComic
{
    public struct Settings
    {
        public bool isFullScreen;
    }
    class UserSettings
    {
        public static void Save(Settings settings)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/FuzzyComic";
            Directory.CreateDirectory(directory);
            var path = directory + "/Settings.xml";
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));

            using (var file = File.Create(path))
            {
                writer.Serialize(file, settings);
            }
        }

    }
}
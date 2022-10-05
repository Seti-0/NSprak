using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace NSprakIDE
{
    public class SaveData
    {
        private const string savePath = "save-data.xml";

        public static SaveData Instance { get; private set; }

        /// <summary>
        /// If there is any config data, attempt to write it to disk. This
        /// logs an error message if that attempt fails.
        /// </summary>
        public static void Save()
        {
            if (!Instance.HasData)
                return;

            try
            {
                using XmlWriter writer = XmlWriter.Create(savePath);
                new XmlSerializer(typeof(SaveData))
                    .Serialize(writer, Instance);
            }
            catch (InvalidOperationException e)
            {
                Logs.Core.LogError($"Failed to save config to '{savePath}'.", e);
            }
        }

        /// <summary>
        /// Attempts to read saved config from the disk. If this fails,
        /// the default initial config is used instead.
        /// </summary>
        public static void Load()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    using XmlReader reader = XmlReader.Create(savePath);
                    Instance = (SaveData)new XmlSerializer(typeof(SaveData))
                        .Deserialize(reader);
                }
                catch (InvalidOperationException e)
                {
                    string msg = $"Failed to read config from '{savePath}'. "
                        + "This could be because the the config file is invalid.";
                    Logs.Core.LogError(msg, e);

                    Instance = new SaveData();
                }
            }
            else
            {
                Instance = new SaveData();
            }
        }

        /// <summary>
        /// The currently opened workspace, or null if there is none.
        /// Currently, there is only one allowed, since there is 
        /// only one window allowed at a time.
        /// </summary>
        public string OpenFolder { get; set; }

        /// <summary>
        /// Returns true if this instance is different from a newly
        /// created one.
        /// </summary>
        public bool HasData
        {
            get
            {
                // I'll need to remember to update this as new fields are added.
                return OpenFolder != null;
            }
        }

        private SaveData(){}
    }
}

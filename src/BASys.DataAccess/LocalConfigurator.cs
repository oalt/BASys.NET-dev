using System.Text;

namespace BASys.DataAccess
{
    /// <summary>
    /// Manages the local data directory of the application (same layout as the Java application):
    /// basys/basys.properties, basys/projects, basys/global-data/eib/devicedata and basys/global-data/eib/eib-devices.xml.
    /// </summary>
    public class LocalConfigurator
    {
        private const string ConfigFileName = "basys.properties";

        private const string ProjectDirectoryName = "projects";

        private const string GlobalDirectoryName = "global-data";

        private const string EibDirectoryName = "eib";

        private const string DeviceDataDirectoryName = "devicedata";

        private const string HelpDirectoryName = "help";

        private readonly string _baseDirectory;

        public LocalConfigurator() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "basys"))
        {
        }

        public LocalConfigurator(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string BaseDirectory
        {
            get
            {
                return _baseDirectory;
            }
        }

        public string ProjectsDirectory
        {
            get
            {
                string result = Path.Combine(_baseDirectory, ProjectDirectoryName);

                return result;
            }
        }

        public string EibDirectory
        {
            get
            {
                string result = Path.Combine(_baseDirectory, GlobalDirectoryName, EibDirectoryName);

                return result;
            }
        }

        /// <summary>
        /// Directory that contains the manufacturer product data files (*.xml).
        /// </summary>
        public string DeviceDataDirectory
        {
            get
            {
                string result = Path.Combine(EibDirectory, DeviceDataDirectoryName);

                return result;
            }
        }

        public string EibDevicesFile
        {
            get
            {
                string result = Path.Combine(EibDirectory, "eib-devices.xml");

                return result;
            }
        }

        private string ConfigFile
        {
            get
            {
                string result = Path.Combine(_baseDirectory, ConfigFileName);

                return result;
            }
        }

        public bool IsValidConfig()
        {
            bool result = File.Exists(ConfigFile);

            return result;
        }

        /// <summary>
        /// Creates the directory structure and the configuration file.
        /// </summary>
        public void CreateConfig(string language)
        {
            Directory.CreateDirectory(ProjectsDirectory);
            Directory.CreateDirectory(DeviceDataDirectory);
            Directory.CreateDirectory(Path.Combine(_baseDirectory, GlobalDirectoryName, HelpDirectoryName));

            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                { "language", language },
                { "basysdir", "basys" },
                { "projectdir", ProjectDirectoryName },
                { "globaldir", GlobalDirectoryName },
                { "eibdir", GlobalDirectoryName + "/" + EibDirectoryName },
                { "eibdevices", GlobalDirectoryName + "/" + EibDirectoryName + "/" + DeviceDataDirectoryName },
                { "helpdir", GlobalDirectoryName + "/" + HelpDirectoryName }
            };

            SaveConfig(properties);
        }

        /// <summary>
        /// Loads the configuration file (Java properties format).
        /// </summary>
        public Dictionary<string, string> LoadConfig()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (File.Exists(ConfigFile))
            {
                foreach (string currentLine in File.ReadAllLines(ConfigFile, Encoding.Latin1))
                {
                    string line = currentLine.Trim();
                    int separatorIndex = line.IndexOfAny(new char[] { '=', ':' });

                    if (line.Length > 0 && line[0] != '#' && line[0] != '!' && separatorIndex > 0)
                    {
                        result[line.Substring(0, separatorIndex).Trim()] = line.Substring(separatorIndex + 1).Trim();
                    }
                }
            }

            return result;
        }

        public void SaveConfig(Dictionary<string, string> properties)
        {
            Directory.CreateDirectory(_baseDirectory);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("#BASys 2003 Configuration");

            foreach (KeyValuePair<string, string> currentProperty in properties)
            {
                builder.AppendLine(currentProperty.Key + "=" + currentProperty.Value);
            }

            File.WriteAllText(ConfigFile, builder.ToString(), Encoding.Latin1);
        }

        /// <summary>
        /// Returns the configured language in the Java locale notation (de_DE, en_EN).
        /// </summary>
        public string GetLanguage()
        {
            string? result;

            if (!LoadConfig().TryGetValue("language", out result))
            {
                result = "de_DE";
            }

            return result;
        }

        public void SetLanguage(string language)
        {
            Dictionary<string, string> properties = LoadConfig();
            properties["language"] = language;
            SaveConfig(properties);
        }
    }
}

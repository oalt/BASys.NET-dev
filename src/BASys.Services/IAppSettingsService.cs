using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Local configuration of the application (language and data directories).
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// False until the configuration was created (first start of the application).
        /// </summary>
        bool IsConfigured { get; }

        string BaseDirectory { get; }

        string ProjectsDirectory { get; }

        string DeviceDataDirectory { get; }

        /// <summary>
        /// Language in Java locale notation (de_DE, en_EN).
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Stores the language and creates the configuration if necessary.
        /// </summary>
        void SetLanguage(string language);

        /// <summary>
        /// Loads the configured bus connection (a mockup connection if nothing is configured).
        /// </summary>
        EibBusSettings LoadBusSettings();

        /// <summary>
        /// Stores the bus connection settings.
        /// </summary>
        void SaveBusSettings(EibBusSettings settings);
    }
}

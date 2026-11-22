using BASys.DataAccess;
using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Settings service that stores the configuration in the local BASys data directory.
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly LocalConfigurator _configurator;

        public AppSettingsService(LocalConfigurator configurator)
        {
            _configurator = configurator;
        }

        public bool IsConfigured
        {
            get
            {
                return _configurator.IsValidConfig();
            }
        }

        public string BaseDirectory
        {
            get
            {
                return _configurator.BaseDirectory;
            }
        }

        public string ProjectsDirectory
        {
            get
            {
                return _configurator.ProjectsDirectory;
            }
        }

        public string DeviceDataDirectory
        {
            get
            {
                return _configurator.DeviceDataDirectory;
            }
        }

        public string Language
        {
            get
            {
                return _configurator.GetLanguage();
            }
        }

        public void SetLanguage(string language)
        {
            if (_configurator.IsValidConfig())
            {
                _configurator.SetLanguage(language);
            }
            else
            {
                _configurator.CreateConfig(language);
            }
        }

        public EibBusSettings LoadBusSettings()
        {
            Dictionary<string, string> properties = _configurator.LoadConfig();
            EibBusSettings result = new EibBusSettings();

            if (properties.TryGetValue("bus.type", out string? type) && Enum.TryParse(type, true, out EibConnectionType connectionType))
            {
                result.ConnectionType = connectionType;
            }

            if (properties.TryGetValue("bus.host", out string? host))
            {
                result.GatewayAddress = host;
            }

            if (properties.TryGetValue("bus.port", out string? port) && int.TryParse(port, out int portNumber))
            {
                result.GatewayPort = portNumber;
            }

            if (properties.TryGetValue("bus.localhost", out string? localHost))
            {
                result.LocalAddress = localHost;
            }

            return result;
        }

        public void SaveBusSettings(EibBusSettings settings)
        {
            if (!_configurator.IsValidConfig())
            {
                _configurator.CreateConfig("de_DE");
            }

            Dictionary<string, string> properties = _configurator.LoadConfig();
            properties["bus.type"] = settings.ConnectionType.ToString();
            properties["bus.host"] = settings.GatewayAddress;
            properties["bus.port"] = settings.GatewayPort.ToString(System.Globalization.CultureInfo.InvariantCulture);
            properties["bus.localhost"] = settings.LocalAddress;
            _configurator.SaveConfig(properties);
        }
    }
}

using BASys.DataModels;

namespace BASys.DataAccess
{
    /// <summary>
    /// Loads and saves the database of known EIB devices (eib-devices.xml).
    /// </summary>
    public class EibDevicesStorage
    {
        private readonly LocalConfigurator _configurator;

        public EibDevicesStorage(LocalConfigurator configurator)
        {
            _configurator = configurator;
        }

        /// <summary>
        /// Loads the device database or creates an empty one if the file does not exist.
        /// </summary>
        public EibDevicesDataModel Load()
        {
            EibDevicesDataModel result = File.Exists(_configurator.EibDevicesFile)
                ? new EibDevicesDataModel(XmlFile.Load(_configurator.EibDevicesFile))
                : new EibDevicesDataModel();

            return result;
        }

        public void Save(EibDevicesDataModel model)
        {
            XmlFile.Save(model.Document, _configurator.EibDevicesFile);
        }
    }
}

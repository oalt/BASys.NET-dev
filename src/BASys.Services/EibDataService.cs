using BASys.DataAccess;
using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// EIB data service working on the files of the local data directory.
    /// </summary>
    public class EibDataService : IEibDataService
    {
        private readonly LocalConfigurator _configurator;

        private readonly EibDevicesStorage _storage;

        private EibDevicesDataModel? _devices;

        public EibDataService(LocalConfigurator configurator)
        {
            _configurator = configurator;
            _storage = new EibDevicesStorage(configurator);
        }

        public EibDevicesDataModel Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = _storage.Load();
                }

                return _devices;
            }
        }

        public void SaveDevices()
        {
            if (_devices != null)
            {
                _storage.Save(_devices);
            }
        }

        public IReadOnlyList<string> GetManufacturerFiles()
        {
            List<string> result = new List<string>();

            if (Directory.Exists(_configurator.DeviceDataDirectory))
            {
                foreach (string currentFile in Directory.GetFiles(_configurator.DeviceDataDirectory, "*.xml"))
                {
                    result.Add(Path.GetFileName(currentFile));
                }

                result.Sort(StringComparer.OrdinalIgnoreCase);
            }

            return result;
        }

        public Task<List<EibProduct>> GetProductsAsync(string manufacturerFile)
        {
            Task<List<EibProduct>> result = Task.Run(() => CreateReader(manufacturerFile).GetProducts());

            return result;
        }

        public Task<List<EibApplication>> GetApplicationsAsync(string manufacturerFile, string productId)
        {
            Task<List<EibApplication>> result = Task.Run(() =>
            {
                EibProductDataReader reader = CreateReader(manufacturerFile);
                List<string> programIds = reader.GetProgramIdsForProduct(productId);

                return reader.GetApplications(programIds);
            });

            return result;
        }

        public Task<List<EibCommunicationObject>> GetCommunicationObjectsAsync(string manufacturerFile, string programId)
        {
            Task<List<EibCommunicationObject>> result = Task.Run(() =>
            {
                EibProductDataReader reader = CreateReader(manufacturerFile);

                List<EibCommunicationObject> normalized = EibProductDataReader.NormalizeCommunicationObjects(reader.GetCommunicationObjects(programId));

                return normalized;
            });

            return result;
        }

        public Task<string> CompleteApplicationAsync(string manufacturerFile, EibApplication application)
        {
            Task<string> result = Task.Run(() =>
            {
                EibProductDataReader reader = CreateReader(manufacturerFile);
                reader.ApplyMaskData(application);

                return reader.GetManufacturerName(application.ManufacturerId);
            });

            return result;
        }

        public Task ConvertVdFileAsync(string vdFileName, string xmlFileName)
        {
            string outputFile = Path.Combine(_configurator.DeviceDataDirectory, xmlFileName);
            Task result = Task.Run(() => new VdConverter().Convert(vdFileName, outputFile));

            return result;
        }

        private EibProductDataReader CreateReader(string manufacturerFile)
        {
            EibProductDataReader result = new EibProductDataReader(Path.Combine(_configurator.DeviceDataDirectory, manufacturerFile));

            return result;
        }
    }
}

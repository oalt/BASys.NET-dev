using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Provides the database of known EIB devices and the manufacturer product data files.
    /// </summary>
    public interface IEibDataService
    {
        /// <summary>
        /// The database of known EIB devices (eib-devices.xml).
        /// </summary>
        EibDevicesDataModel Devices { get; }

        void SaveDevices();

        /// <summary>
        /// Names of the manufacturer product data files (e.g. abb.xml).
        /// </summary>
        IReadOnlyList<string> GetManufacturerFiles();

        Task<List<EibProduct>> GetProductsAsync(string manufacturerFile);

        Task<List<EibApplication>> GetApplicationsAsync(string manufacturerFile, string productId);

        Task<List<EibCommunicationObject>> GetCommunicationObjectsAsync(string manufacturerFile, string programId);

        /// <summary>
        /// Completes the application with mask data and returns the manufacturer name.
        /// </summary>
        Task<string> CompleteApplicationAsync(string manufacturerFile, EibApplication application);

        /// <summary>
        /// Converts an ETS product data file (*.vd_) into the XML format and stores it in the product data directory.
        /// </summary>
        Task ConvertVdFileAsync(string vdFileName, string xmlFileName);
    }
}

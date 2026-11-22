using BASys.DataModels;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Row of the table of devices in the EIB device database.
    /// </summary>
    public class DatabaseDeviceRowViewModel
    {
        public DatabaseDeviceRowViewModel(EibDevicesDataModel model, string deviceId)
        {
            Id = deviceId;
            Manufacturer = model.GetManufacturerName(deviceId);
            Product = model.GetName(deviceId);
            DeviceKind = XmlCodes.ParseDeviceKind(model.GetDeviceType(deviceId));
            Application = model.GetApplication(deviceId).Name;
            Location = XmlCodes.ParseInstallationLocation(model.GetInstallationType(deviceId));
        }

        public string Id { get; }

        public string Manufacturer { get; }

        public string Product { get; }

        public EibDeviceKind DeviceKind { get; }

        public string Application { get; }

        public InstallationLocation Location { get; }
    }
}

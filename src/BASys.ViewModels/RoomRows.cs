using BASys.DataModels;

namespace BASys.ViewModels
{
    /// <summary>
    /// Row of the table "installed devices" of a room.
    /// </summary>
    public class InstallationDeviceRow
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public EndDeviceType DeviceType { get; set; }

        public BusSystem BusSystem { get; set; }

        public string AssignedActuator { get; set; } = string.Empty;

        public string InstallationLocation { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;

        public string ActuatorType { get; set; } = string.Empty;

        public string FunctionGroupName { get; set; } = string.Empty;

        public string Addresses { get; set; } = string.Empty;
    }

    /// <summary>
    /// Row of the table "junction boxes" of a room.
    /// </summary>
    public class JunctionBoxRow
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Row of the table "bus devices" of a room.
    /// </summary>
    public class BusDeviceRow
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Manufacturer { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public BusSystem BusSystem { get; set; }

        public string PhysicalAddress { get; set; } = string.Empty;
    }
}

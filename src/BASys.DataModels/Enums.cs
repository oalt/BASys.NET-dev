namespace BASys.DataModels
{
    /// <summary>
    /// Kind of node in the architectural tree.
    /// </summary>
    public enum ArchitecturalNodeType
    {
        Project,
        Building,
        Floor,
        Room
    }

    /// <summary>
    /// Kind of end device that can be installed in a room. The values are the type codes of the project files.
    /// </summary>
    public enum EndDeviceType
    {
        Lamp = 1,
        DimmableLamp = 2,
        Valve = 3,
        Jalousie = 4,
        Sensor = 100
    }

    /// <summary>
    /// Supported bus systems.
    /// </summary>
    public enum BusSystem
    {
        None,
        Eib,
        Lon,
        Lcn
    }

    /// <summary>
    /// Place where a device is installed.
    /// </summary>
    public enum InstallationLocation
    {
        DinRail,
        Flush,
        ChannelBuiltIn,
        CouplerIntegrated
    }

    /// <summary>
    /// Kind of an EIB device.
    /// </summary>
    public enum EibDeviceKind
    {
        Actuator,
        Sensor
    }

    /// <summary>
    /// Type of the gateway used to connect to the EIB.
    /// </summary>
    public enum GatewayType
    {
        KnxNetIp,
        Darmstadt,
        LinuxBcu1
    }
}

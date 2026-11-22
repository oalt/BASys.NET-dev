namespace BASys.DataModels
{
    /// <summary>
    /// Conversion between enumerations and the text codes that are stored in the project and device files.
    /// </summary>
    public static class XmlCodes
    {
        public static string ToCode(BusSystem busSystem)
        {
            string result = "none";

            switch (busSystem)
            {
                case BusSystem.Eib:
                    result = "EIB";
                    break;
                case BusSystem.Lon:
                    result = "LON";
                    break;
                case BusSystem.Lcn:
                    result = "LCN";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static BusSystem ParseBusSystem(string code)
        {
            BusSystem result = BusSystem.None;

            switch (code)
            {
                case "EIB":
                    result = BusSystem.Eib;
                    break;
                case "LON":
                    result = BusSystem.Lon;
                    break;
                case "LCN":
                    result = BusSystem.Lcn;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string ToCode(InstallationLocation location)
        {
            string result = "REG";

            switch (location)
            {
                case InstallationLocation.Flush:
                    result = "UP";
                    break;
                case InstallationLocation.ChannelBuiltIn:
                    result = "CI";
                    break;
                case InstallationLocation.CouplerIntegrated:
                    result = "DEVICE";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static InstallationLocation ParseInstallationLocation(string code)
        {
            InstallationLocation result = InstallationLocation.DinRail;

            switch (code)
            {
                case "UP":
                    result = InstallationLocation.Flush;
                    break;
                case "CI":
                    result = InstallationLocation.ChannelBuiltIn;
                    break;
                case "DEVICE":
                    result = InstallationLocation.CouplerIntegrated;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string ToCode(EibDeviceKind kind)
        {
            string result = kind == EibDeviceKind.Sensor ? "sensor" : "actuator";

            return result;
        }

        public static EibDeviceKind ParseDeviceKind(string code)
        {
            EibDeviceKind result = code == "sensor" ? EibDeviceKind.Sensor : EibDeviceKind.Actuator;

            return result;
        }

        public static EndDeviceType ParseEndDeviceType(int code)
        {
            EndDeviceType result = EndDeviceType.Sensor;

            if (Enum.IsDefined(typeof(EndDeviceType), code))
            {
                result = (EndDeviceType)code;
            }

            return result;
        }
    }
}

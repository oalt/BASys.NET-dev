using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Helpers to get display names of enumeration values.
    /// </summary>
    public static class LocalizationExtensions
    {
        public static string GetName(this ILocalizationService localizer, EndDeviceType deviceType)
        {
            string key = "l.sensor";

            switch (deviceType)
            {
                case EndDeviceType.Lamp:
                    key = "l.lamp";
                    break;
                case EndDeviceType.DimmableLamp:
                    key = "l.dimlamp";
                    break;
                case EndDeviceType.Valve:
                    key = "l.valve";
                    break;
                case EndDeviceType.Jalousie:
                    key = "l.jal";
                    break;
                default:
                    break;
            }

            string result = localizer[key];

            return result;
        }

        public static string GetName(this ILocalizationService localizer, InstallationLocation location)
        {
            string key = "l.ilREG";

            switch (location)
            {
                case InstallationLocation.Flush:
                    key = "l.ilUP";
                    break;
                case InstallationLocation.ChannelBuiltIn:
                    key = "l.ilKI";
                    break;
                case InstallationLocation.CouplerIntegrated:
                    key = "l.ilByDevice";
                    break;
                default:
                    break;
            }

            string result = localizer[key];

            return result;
        }

        public static string GetName(this ILocalizationService localizer, BusSystem busSystem)
        {
            string result = localizer["l.noBusSystemPreffered"];

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

        public static string GetName(this ILocalizationService localizer, EibDeviceKind deviceKind)
        {
            string result = deviceKind == EibDeviceKind.Actuator ? localizer["l.actuator"] : localizer["l.sensor"];

            return result;
        }
    }
}

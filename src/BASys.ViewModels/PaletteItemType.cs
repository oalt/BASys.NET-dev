using BASys.DataModels;

namespace BASys.ViewModels
{
    /// <summary>
    /// Elements that can be created from the palette of the structural view.
    /// </summary>
    public enum PaletteItemType
    {
        Building,
        Floor,
        Room,
        JunctionBox,
        Lamp,
        DimmableLamp,
        Valve,
        Jalousie,
        Sensor
    }

    /// <summary>
    /// Helper methods for <see cref="PaletteItemType"/>.
    /// </summary>
    public static class PaletteItemTypeExtensions
    {
        public static bool IsEndDevice(this PaletteItemType itemType)
        {
            bool result = itemType >= PaletteItemType.Lamp;

            return result;
        }

        public static EndDeviceType ToEndDeviceType(this PaletteItemType itemType)
        {
            EndDeviceType result = EndDeviceType.Sensor;

            switch (itemType)
            {
                case PaletteItemType.Lamp:
                    result = EndDeviceType.Lamp;
                    break;
                case PaletteItemType.DimmableLamp:
                    result = EndDeviceType.DimmableLamp;
                    break;
                case PaletteItemType.Valve:
                    result = EndDeviceType.Valve;
                    break;
                case PaletteItemType.Jalousie:
                    result = EndDeviceType.Jalousie;
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns the type of node that has to contain the new element.
        /// </summary>
        public static ArchitecturalNodeType GetRequiredParentType(this PaletteItemType itemType)
        {
            ArchitecturalNodeType result = ArchitecturalNodeType.Room;

            switch (itemType)
            {
                case PaletteItemType.Building:
                    result = ArchitecturalNodeType.Project;
                    break;
                case PaletteItemType.Floor:
                    result = ArchitecturalNodeType.Building;
                    break;
                case PaletteItemType.Room:
                    result = ArchitecturalNodeType.Floor;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}

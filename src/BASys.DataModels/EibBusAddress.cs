using System.Globalization;

namespace BASys.DataModels
{
    /// <summary>
    /// An EIB bus address as it is transmitted in a frame: either a physical address (zone.line.device)
    /// or a group address (main/sub). Port of EIBPhaddress and EIBGrpaddress.
    /// </summary>
    public sealed class EibBusAddress
    {
        private EibBusAddress(bool isGroup, int high, int low)
        {
            IsGroup = isGroup;
            High = high;
            Low = low;
        }

        /// <summary>
        /// True for a group address, false for a physical address.
        /// </summary>
        public bool IsGroup { get; }

        /// <summary>
        /// The high byte of the address.
        /// </summary>
        public int High { get; }

        /// <summary>
        /// The low byte of the address.
        /// </summary>
        public int Low { get; }

        /// <summary>
        /// Creates a physical address (zone 0..15, line 0..15, device 0..255).
        /// </summary>
        public static EibBusAddress CreatePhysical(int zone, int line, int device)
        {
            EibBusAddress result = new EibBusAddress(false, ((zone & 0x0F) << 4) | (line & 0x0F), device & 0xFF);

            return result;
        }

        /// <summary>
        /// Creates a group address (main group 0..31, sub group 0..2047).
        /// </summary>
        public static EibBusAddress CreateGroup(int mainGroup, int subGroup)
        {
            int value = ((mainGroup & 0x1F) << 11) | (subGroup & 0x07FF);
            EibBusAddress result = new EibBusAddress(true, value >> 8, value & 0xFF);

            return result;
        }

        public override string ToString()
        {
            string result;

            if (IsGroup)
            {
                int subGroup = ((High & 0x07) << 8) | Low;
                result = (High >> 3).ToString(CultureInfo.InvariantCulture) + "/" + subGroup.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                result = (High >> 4).ToString(CultureInfo.InvariantCulture) + "." + (High & 0x0F).ToString(CultureInfo.InvariantCulture)
                    + "." + Low.ToString(CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Returns the address as text. Group addresses are shown as main/sub (two level) or main/middle/sub (three level);
        /// physical addresses are not affected.
        /// </summary>
        public string ToString(bool twoLevelGroup)
        {
            string result;

            if (IsGroup && !twoLevelGroup)
            {
                result = (High >> 3).ToString(CultureInfo.InvariantCulture) + "/" + (High & 0x07).ToString(CultureInfo.InvariantCulture)
                    + "/" + Low.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                result = ToString();
            }

            return result;
        }
    }
}

using System.Globalization;

namespace BASys.DataModels
{
    /// <summary>
    /// Helper methods for EIB physical (zone.line.device) and group (main/sub) addresses.
    /// </summary>
    public static class EibAddress
    {
        /// <summary>
        /// Formats the group address that belongs to the running number of a function (0x100 + count in 5/11 bit format).
        /// </summary>
        public static string FormatGroupAddress(int groupAddressCount)
        {
            int address = 0x100 + groupAddressCount;
            int high = address >> 8;
            int low = address & 0xFF;
            string result = (high >> 3).ToString(CultureInfo.InvariantCulture) + "/" + (((high & 0x07) << 8) + low).ToString(CultureInfo.InvariantCulture);

            return result;
        }

        /// <summary>
        /// Checks the format of a physical address (zone 0..15, line 0..15, device 0..255) and returns the normalized text.
        /// </summary>
        public static bool TryParsePhysicalAddress(string text, out string normalized)
        {
            bool result = false;
            normalized = string.Empty;
            string[] parts = text.Trim().Split('.');
            int zone;
            int line;
            int device;

            if (parts.Length == 3 && int.TryParse(parts[0], out zone) && int.TryParse(parts[1], out line) && int.TryParse(parts[2], out device)
                && zone >= 0 && zone <= 15 && line >= 0 && line <= 15 && device >= 0 && device <= 255)
            {
                normalized = zone.ToString(CultureInfo.InvariantCulture) + "." + line.ToString(CultureInfo.InvariantCulture) + "." + device.ToString(CultureInfo.InvariantCulture);
                result = true;
            }

            return result;
        }
    }
}

using System.Globalization;
using System.Text;

namespace BASys.DataModels
{
    /// <summary>
    /// One option of the TPCI selection: the displayed text, the TPCI value and the two APCI high bits that share the byte.
    /// </summary>
    public sealed class EibTpciOption
    {
        public EibTpciOption(string text, int tpci, int apciHighBits, bool isControl)
        {
            Text = text;
            Tpci = tpci;
            ApciHighBits = apciHighBits;
            IsControl = isControl;
        }

        public string Text { get; }

        public int Tpci { get; }

        /// <summary>
        /// The two lowest bits of the byte that holds the TPCI (they are the highest APCI bits).
        /// </summary>
        public int ApciHighBits { get; }

        /// <summary>
        /// True for connection control telegrams (connect, disconnect, ack, nack) which carry no application data.
        /// </summary>
        public bool IsControl { get; }
    }

    /// <summary>
    /// One option of the APCI selection.
    /// </summary>
    public sealed class EibApciOption
    {
        public EibApciOption(string text, int apci)
        {
            Text = text;
            Apci = apci;
        }

        public string Text { get; }

        public int Apci { get; }
    }

    /// <summary>
    /// An EIB telegram (frame) on the twisted pair bus. Port of the Java class EIBFrame.
    /// The application data array uses the Java layout: for short data it holds the 6 bit value, for long data
    /// element 0 is unused and the data bytes follow.
    /// </summary>
    public class EibFrame
    {
        public const int PrioritySystem = 0;

        private static readonly string[] PriorityNames = { "System", "High", "Alarm", "Low" };

        private static readonly Dictionary<int, string> ApciNames = new Dictionary<int, string>
        {
            { 0x000, "A_READ_VALUE_REQ_PDU" },
            { 0x040, "A_READ_VALUE_RES_PDU" },
            { 0x080, "A_WRITE_VALUE_REQ_PDU" },
            { 0x0C0, "A_SET_PHYSADDR_REQ_PDU" },
            { 0x100, "A_READ_PHYSADDR_REQ_PDU" },
            { 0x140, "A_SET_PHYSADDR_RES_PDU" },
            { 0x180, "A_READ_ADC_REQ_PDU" },
            { 0x1C0, "A_READ_ADC_RES_PDU" },
            { 0x200, "A_READ_MEMORY_REQ_PDU" },
            { 0x240, "A_READ_MEMORY_RES_PDU" },
            { 0x280, "A_WRITE_MEMORY_REQ_PDU" },
            { 0x2C0, "A_UREAD_MEMORY_REQ_PDU" },
            { 0x2C1, "A_UREAD_MEMORY_RES_PDU" },
            { 0x2C2, "A_UWRITE_MEMORY_REQ_PDU" },
            { 0x2C4, "A_UWRITE_MEMORY_BIT_REQ_PDU" },
            { 0x2C5, "A_UREAD_MFACTINFO_REQ_PDU" },
            { 0x2C6, "A_UREAD_MFACTINFO_RES_PDU" },
            { 0x300, "A_READ_MASK_VERSION_REQ_PDU" },
            { 0x340, "A_READ_MASK_VERSION_RES_PDU" },
            { 0x380, "A_RESTART_REQ_PDU" },
            { 0x3D0, "A_WRITE_MEMORY_BIT_REQ_PDU" },
            { 0x3D1, "A_AUTHORIZE_REQ_PDU" },
            { 0x3D2, "A_AUTHORIZE_RES_PDU" },
            { 0x3D3, "A_SETKEY_REQ_PDU" },
            { 0x3D4, "A_SETKEY_RES_PDU" },
            { 0x3D5, "A_READ_PROPERY_VAKUE_REQ_PDU" },
            { 0x3D6, "A_READ_PROPERY_VAKUE_RES_PDU" },
            { 0x3D7, "A_WRITE_PROPERY_VAKUE_REQ_PDU" },
            { 0x3D8, "A_READ_PROPERY_DESCRIPTION_REQ_PDU" },
            { 0x3D9, "A_READ_PROPERY_DESCRIPTION_RES_PDU" },
            { 0x3DC, "A_READ_PHYSADDR_SERNO_REQ_PDU" },
            { 0x3DD, "A_READ_PHYSADDR_SERNO_RES_PDU" },
            { 0x3DE, "A_SET_PHYSADDR_SERNO_REQ_PDU" },
            { 0x3DF, "A_SEVICE_INFORMATION_REQ_PDU" },
            { 0x3E0, "A_SET_SYS_ID_REQ_PDU" },
            { 0x3E1, "A_READ_SYS_ID_REQ_PDU" },
            { 0x3E2, "A_READ_SYS_ID_RES_PDU" },
            { 0x3E3, "A_READ_SYS_ID_SELECTIVE_REQ_PDU" },
            { 0x3FF, "FILE OPERATION" }
        };

        private static readonly List<EibTpciOption> TpciOptionList = CreateTpciOptions();

        private static readonly List<EibApciOption> ApciOptionList = CreateApciOptions();

        private int[] _applicationData;

        public EibFrame(bool repeat, int priority, EibBusAddress sourceAddress, EibBusAddress destinationAddress,
            int routingCounter, int tpci, int apci, int[] applicationData)
        {
            Repeat = repeat;
            Priority = priority;
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
            RoutingCounter = routingCounter;
            Tpci = tpci;
            Apci = apci;
            _applicationData = applicationData;
            IsShortData = applicationData.Length == 1 || (applicationData.Length > 0 && applicationData[0] != 0);
        }

        public bool Repeat { get; set; }

        /// <summary>
        /// 0 = system, 1 = high, 2 = alarm, 3 = low.
        /// </summary>
        public int Priority { get; set; }

        public EibBusAddress SourceAddress { get; set; }

        public EibBusAddress DestinationAddress { get; set; }

        public int RoutingCounter { get; set; }

        public int Tpci { get; set; }

        /// <summary>
        /// The 10 bit APCI value (the lower 6 bits are only part of it for long data telegrams).
        /// </summary>
        public int Apci { get; set; }

        /// <summary>
        /// True if the (up to 6 bit) data is part of the APCI byte.
        /// </summary>
        public bool IsShortData { get; set; }

        public int Acknowledge { get; set; }

        public int[] ApplicationData
        {
            get
            {
                return _applicationData;
            }
            set
            {
                _applicationData = value;
            }
        }

        /// <summary>
        /// The frame parity (checksum), calculated from the frame content.
        /// </summary>
        public int Parity
        {
            get
            {
                int[] frame = GetFrame();
                int checksum = frame[0];

                for (int counter = 1; counter < frame.Length; counter++)
                {
                    checksum ^= frame[counter];
                }

                int result = ~checksum & 0xFF;

                return result;
            }
        }

        public string PriorityText
        {
            get
            {
                string result = Priority >= 0 && Priority < PriorityNames.Length ? PriorityNames[Priority] : string.Empty;

                return result;
            }
        }

        /// <summary>
        /// The TPCI as text.
        /// </summary>
        public string TpciText
        {
            get
            {
                string result = string.Empty;
                int apciBits = (Apci >> 8) & 0x03;

                if (Tpci == 0)
                {
                    if (DestinationAddress.High == 0 && DestinationAddress.Low == 0)
                    {
                        result = "T_BROADCAST_DATA_REQ_PDU";
                    }
                    else if (!DestinationAddress.IsGroup)
                    {
                        result = "T_DATA_UNACK_REQ_PDU";
                    }
                    else
                    {
                        result = "T_GROUPDATA_REQ_PDU";
                    }
                }
                else if (Tpci == 0x20)
                {
                    result = apciBits == 0 ? "T_CONNECT_REQ_PDU" : apciBits == 2 ? "T_CONNECT_CONF_PDU" : apciBits == 1 ? "T_DISCONNECT_REQ_PDU" : string.Empty;
                }
                else if (((Tpci >> 4) & 0x03) == 1)
                {
                    result = "T_DATA_REQ_PDU (Seq.: " + (Tpci & 0x0F).ToString(CultureInfo.InvariantCulture) + ")";
                }
                else if (((Tpci >> 4) & 0x03) == 3)
                {
                    result = apciBits == 2 ? "T_DATA_ACK_PDU (Seq.: " + (Tpci & 0x0F).ToString(CultureInfo.InvariantCulture) + ")"
                        : apciBits == 3 ? "T_DATA_NACK_PDU (Seq.: " + (Tpci & 0x0F).ToString(CultureInfo.InvariantCulture) + ")" : string.Empty;
                }
                else
                {
                    result = "Unknown !!! " + Tpci.ToString(CultureInfo.InvariantCulture);
                }

                return result;
            }
        }

        /// <summary>
        /// The APCI as text ("-" if the frame has no application data).
        /// </summary>
        public string ApciText
        {
            get
            {
                string result;
                int comparable = IsShortData ? Apci & 0x3C0 : Apci;

                if (_applicationData.Length == 0)
                {
                    result = "-";
                }
                else if (ApciNames.TryGetValue(comparable, out string? name))
                {
                    result = name;
                }
                else
                {
                    result = "unknown !!! 0x" + Apci.ToString("x", CultureInfo.InvariantCulture);
                }

                return result;
            }
        }

        /// <summary>
        /// The application data as hex text ("-" if there is none).
        /// </summary>
        public string ApplicationDataText
        {
            get
            {
                string result;

                if (_applicationData.Length == 0)
                {
                    result = "-";
                }
                else
                {
                    StringBuilder builder = new StringBuilder();

                    for (int counter = IsShortData ? 0 : 1; counter < _applicationData.Length; counter++)
                    {
                        builder.Append(_applicationData[counter].ToString("X2", CultureInfo.InvariantCulture)).Append(' ');
                    }

                    result = builder.ToString();
                }

                return result;
            }
        }

        /// <summary>
        /// The available TPCI choices of the telegram dialog.
        /// </summary>
        public static IReadOnlyList<EibTpciOption> TpciOptions
        {
            get
            {
                return TpciOptionList;
            }
        }

        /// <summary>
        /// The available APCI choices of the telegram dialog.
        /// </summary>
        public static IReadOnlyList<EibApciOption> ApciOptions
        {
            get
            {
                return ApciOptionList;
            }
        }

        /// <summary>
        /// Returns the frame as bytes (control field, addresses, length/routing, TPCI/APCI, data), without parity.
        /// </summary>
        public int[] GetFrame()
        {
            int length = _applicationData.Length;
            int[] result = new int[length + 7];

            result[0] = (Repeat ? 0x90 : 0xB0) | (Priority << 2);
            result[1] = SourceAddress.High;
            result[2] = SourceAddress.Low;
            result[3] = DestinationAddress.High;
            result[4] = DestinationAddress.Low;
            result[5] = ((DestinationAddress.IsGroup ? 1 : 0) << 7) | (RoutingCounter << 4) | length;
            result[6] = (Tpci << 2) | (Apci >> 8);

            if (length != 0)
            {
                result[7] = IsShortData ? (Apci & 0xC0) | (_applicationData[0] & 0x3F) : Apci & 0xFF;

                for (int counter = 1; counter < length; counter++)
                {
                    result[counter + 7] = _applicationData[counter];
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the whole frame including parity and acknowledge as hex text.
        /// </summary>
        public string ToHexString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (int currentByte in GetFrame())
            {
                builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture)).Append(' ');
            }

            builder.Append(Parity.ToString("X2", CultureInfo.InvariantCulture)).Append(' ');
            builder.Append(Acknowledge.ToString("X", CultureInfo.InvariantCulture));
            string result = builder.ToString();

            return result;
        }

        /// <summary>
        /// Creates a copy of the frame (the receiver of a sent frame must not see later edits).
        /// </summary>
        public EibFrame Clone()
        {
            EibFrame result = new EibFrame(Repeat, Priority, SourceAddress, DestinationAddress, RoutingCounter, Tpci, Apci, (int[])_applicationData.Clone());
            result.IsShortData = IsShortData;
            result.Acknowledge = Acknowledge;

            return result;
        }

        private static List<EibTpciOption> CreateTpciOptions()
        {
            List<EibTpciOption> result = new List<EibTpciOption>();

            result.Add(new EibTpciOption("T_DATA_REQ_PDU (Broadcast, Group, Unack)", 0, 0, false));

            for (int sequence = 0; sequence <= 15; sequence++)
            {
                result.Add(new EibTpciOption("T_DATA_REQ_PDU (" + sequence.ToString(CultureInfo.InvariantCulture) + ")", 0x10 + sequence, 0, false));
            }

            result.Add(new EibTpciOption("T_CONNECT_REQ_PDU", 0x20, 0, true));
            result.Add(new EibTpciOption("T_CONNECT_CONF_PDU", 0x20, 2, true));
            result.Add(new EibTpciOption("T_DISCONNECT_REQ_PDU", 0x20, 1, true));

            for (int sequence = 0; sequence <= 15; sequence++)
            {
                result.Add(new EibTpciOption("T_DATA_ACK_PDU (" + sequence.ToString(CultureInfo.InvariantCulture) + ")", 0x30 + sequence, 2, true));
            }

            for (int sequence = 0; sequence <= 15; sequence++)
            {
                result.Add(new EibTpciOption("T_DATA_NACK_PDU (" + sequence.ToString(CultureInfo.InvariantCulture) + ")", 0x30 + sequence, 3, true));
            }

            return result;
        }

        private static List<EibApciOption> CreateApciOptions()
        {
            int[] values =
            {
                0x000, 0x040, 0x080, 0x0C0, 0x100, 0x140, 0x3DC, 0x3DD, 0x3DE, 0x3DF, 0x3E0, 0x3E1, 0x3E2, 0x3E3, 0x180, 0x1C0, 0x200, 0x240, 0x280
            };
            List<EibApciOption> result = new List<EibApciOption>();

            foreach (int currentValue in values)
            {
                result.Add(new EibApciOption(ApciNames[currentValue], currentValue));
            }

            return result;
        }
    }
}

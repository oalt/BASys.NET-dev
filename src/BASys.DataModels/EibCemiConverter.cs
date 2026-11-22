namespace BASys.DataModels
{
    /// <summary>
    /// Converts between <see cref="EibFrame"/> and the cEMI L_Data frames used by KNXnet/IP gateways.
    /// </summary>
    public static class EibCemiConverter
    {
        /// <summary>
        /// L_Data.req: a frame sent to the bus.
        /// </summary>
        public const int MessageCodeDataRequest = 0x11;

        /// <summary>
        /// L_Data.ind: a frame received from the bus.
        /// </summary>
        public const int MessageCodeDataIndication = 0x29;

        /// <summary>
        /// L_Data.con: confirmation of a frame sent through the gateway.
        /// </summary>
        public const int MessageCodeDataConfirmation = 0x2E;

        private const int HeaderLength = 9;

        /// <summary>
        /// Builds an L_Data.req cEMI frame from the given frame.
        /// </summary>
        public static byte[] ToCemi(EibFrame frame)
        {
            int[] frameBytes = frame.GetFrame();
            int tpduLength = frameBytes.Length - 6;
            byte[] result = new byte[HeaderLength + tpduLength];

            result[0] = MessageCodeDataRequest;
            result[1] = 0;
            result[2] = (byte)(0x90 | (frame.Repeat ? 0 : 0x20) | (frame.Priority << 2));
            result[3] = (byte)((frame.DestinationAddress.IsGroup ? 0x80 : 0) | (frame.RoutingCounter << 4));
            result[4] = (byte)frameBytes[1];
            result[5] = (byte)frameBytes[2];
            result[6] = (byte)frameBytes[3];
            result[7] = (byte)frameBytes[4];
            result[8] = (byte)frame.ApplicationData.Length;

            for (int counter = 0; counter < tpduLength; counter++)
            {
                result[HeaderLength + counter] = (byte)frameBytes[6 + counter];
            }

            return result;
        }

        /// <summary>
        /// Parses an L_Data.ind or L_Data.con cEMI frame. Returns null for all other messages and for malformed frames.
        /// </summary>
        public static EibFrame? FromCemi(byte[] cemi)
        {
            EibFrame? result = null;

            if (cemi.Length >= HeaderLength + 1
                && (cemi[0] == MessageCodeDataIndication || cemi[0] == MessageCodeDataConfirmation || cemi[0] == MessageCodeDataRequest))
            {
                int offset = cemi[1];
                int dataLength = offset + 8 < cemi.Length ? cemi[offset + 8] : -1;

                if (dataLength >= 0 && cemi.Length >= offset + HeaderLength + dataLength + 1)
                {
                    result = Parse(cemi, offset, dataLength);
                }
            }

            return result;
        }

        private static EibFrame Parse(byte[] cemi, int offset, int dataLength)
        {
            int control1 = cemi[offset + 2];
            int control2 = cemi[offset + 3];
            bool isGroup = (control2 & 0x80) != 0;
            EibBusAddress source = EibBusAddress.CreatePhysical(cemi[offset + 4] >> 4, cemi[offset + 4] & 0x0F, cemi[offset + 5]);
            EibBusAddress destination = isGroup
                ? EibBusAddress.CreateGroup(cemi[offset + 6] >> 3, ((cemi[offset + 6] & 0x07) << 8) | cemi[offset + 7])
                : EibBusAddress.CreatePhysical(cemi[offset + 6] >> 4, cemi[offset + 6] & 0x0F, cemi[offset + 7]);
            int tpduStart = offset + HeaderLength;
            int tpci = cemi[tpduStart] >> 2;
            int apci = (cemi[tpduStart] & 0x03) << 8;
            int[] data = new int[dataLength];
            bool isShort = true;

            if (dataLength > 0)
            {
                int apciLow = cemi[tpduStart + 1];
                int command = ((apci | apciLow) >> 6) & 0x0F;
                isShort = command != 0x0B && command != 0x0F;

                if (isShort)
                {
                    apci |= apciLow & 0xC0;
                    data[0] = apciLow & 0x3F;
                }
                else
                {
                    apci |= apciLow;
                }

                for (int counter = 1; counter < dataLength; counter++)
                {
                    data[counter] = cemi[tpduStart + 1 + counter];
                }
            }

            EibFrame result = new EibFrame((control1 & 0x20) == 0, (control1 >> 2) & 0x03, source, destination, (control2 >> 4) & 0x07, tpci, apci, data);
            result.IsShortData = isShort;
            result.Acknowledge = cemi[0] == MessageCodeDataIndication ? 0xC : 0;

            return result;
        }
    }
}

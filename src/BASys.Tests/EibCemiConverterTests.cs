using BASys.DataModels;
using Xunit;

namespace BASys.Tests
{
    public class EibCemiConverterTests
    {
        [Fact]
        public void DefaultFrameIsConvertedToDataRequest()
        {
            EibFrame frame = new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 2, 1), EibBusAddress.CreateGroup(1, 257), 6, 0, 0x80, new int[] { 0 });

            byte[] cemi = EibCemiConverter.ToCemi(frame);

            Assert.Equal(new byte[] { 0x11, 0x00, 0xBC, 0xE0, 0x12, 0x01, 0x09, 0x01, 0x01, 0x00, 0x80 }, cemi);
        }

        [Fact]
        public void GroupWriteIndicationIsParsed()
        {
            byte[] cemi = { 0x29, 0x00, 0xBC, 0xE0, 0x11, 0x01, 0x08, 0x01, 0x01, 0x00, 0x81 };

            EibFrame? frame = EibCemiConverter.FromCemi(cemi);

            Assert.NotNull(frame);
            Assert.Equal("1.1.1", frame.SourceAddress.ToString());
            Assert.Equal("1/1", frame.DestinationAddress.ToString());
            Assert.Equal("Low", frame.PriorityText);
            Assert.Equal("A_WRITE_VALUE_REQ_PDU", frame.ApciText);
            Assert.Equal("01 ", frame.ApplicationDataText);
            Assert.False(frame.Repeat);
        }

        [Fact]
        public void LongDataFrameSurvivesRoundTrip()
        {
            EibFrame frame = new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 1, 2), EibBusAddress.CreatePhysical(1, 1, 3), 6, 0x10, 0x3DC, new int[] { 0, 0x0A, 0xFF });
            frame.IsShortData = false;

            EibFrame? parsed = EibCemiConverter.FromCemi(EibCemiConverter.ToCemi(frame));

            Assert.NotNull(parsed);
            Assert.Equal(frame.ToHexString().Substring(0, frame.ToHexString().Length - 1), parsed.ToHexString().Substring(0, parsed.ToHexString().Length - 1));
            Assert.Equal("A_READ_PHYSADDR_SERNO_REQ_PDU", parsed.ApciText);
            Assert.Equal("0A FF ", parsed.ApplicationDataText);
        }

        [Fact]
        public void ControlTelegramSurvivesRoundTrip()
        {
            EibFrame frame = new EibFrame(true, 0, EibBusAddress.CreatePhysical(1, 1, 2), EibBusAddress.CreatePhysical(1, 1, 3), 6, 0x20, 0x200, new int[0]);

            EibFrame? parsed = EibCemiConverter.FromCemi(EibCemiConverter.ToCemi(frame));

            Assert.NotNull(parsed);
            Assert.True(parsed.Repeat);
            Assert.Equal("T_CONNECT_CONF_PDU", parsed.TpciText);
            Assert.Equal("-", parsed.ApplicationDataText);
        }

        [Fact]
        public void UnknownOrTruncatedMessagesAreIgnored()
        {
            Assert.Null(EibCemiConverter.FromCemi(new byte[] { 0x2B, 0x00, 0x01 }));
            Assert.Null(EibCemiConverter.FromCemi(new byte[] { 0x29, 0x00, 0xBC, 0xE0, 0x11, 0x01, 0x08, 0x01, 0x03, 0x00, 0x81 }));
        }
    }
}

using BASys.DataModels;
using Xunit;

namespace BASys.Tests
{
    public class EibFrameTests
    {
        [Fact]
        public void DefaultFrameOfTelegramDialogHasExpectedBytes()
        {
            EibFrame frame = new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 2, 1), EibBusAddress.CreateGroup(1, 257), 6, 0, 0x80, new int[] { 0 });

            Assert.Equal("BC 12 01 09 01 E1 00 80 39 0", frame.ToHexString());
            Assert.Equal("1.2.1", frame.SourceAddress.ToString());
            Assert.Equal("1/257", frame.DestinationAddress.ToString());
            Assert.Equal("Low", frame.PriorityText);
            Assert.Equal("T_GROUPDATA_REQ_PDU", frame.TpciText);
            Assert.Equal("A_WRITE_VALUE_REQ_PDU", frame.ApciText);
            Assert.Equal("00 ", frame.ApplicationDataText);
        }

        [Fact]
        public void LongDataFrameKeepsDataBytes()
        {
            EibFrame frame = new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 1, 2), EibBusAddress.CreatePhysical(1, 1, 3), 6, 0x10, 0x3DC, new int[] { 0, 0x0A, 0xFF });
            frame.IsShortData = false;

            Assert.Equal("0A FF ", frame.ApplicationDataText);
            Assert.Equal("A_READ_PHYSADDR_SERNO_REQ_PDU", frame.ApciText);
            Assert.Equal("T_DATA_REQ_PDU (Seq.: 0)", frame.TpciText);
            int[] bytes = frame.GetFrame();
            Assert.Equal(10, bytes.Length);
            Assert.Equal(0xDC, bytes[7]);
            Assert.Equal(0x0A, bytes[8]);
            Assert.Equal(0xFF, bytes[9]);
        }

        [Fact]
        public void ControlTelegramHasNoApplicationData()
        {
            EibTpciOption connect = EibFrame.TpciOptions.First(currentOption => currentOption.Text == "T_CONNECT_CONF_PDU");
            EibFrame frame = new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 1, 2), EibBusAddress.CreatePhysical(1, 1, 3), 6,
                connect.Tpci, connect.ApciHighBits << 8, new int[0]);

            Assert.Equal("T_CONNECT_CONF_PDU", frame.TpciText);
            Assert.Equal("-", frame.ApciText);
            Assert.Equal("-", frame.ApplicationDataText);
        }
    }
}

using System.Net;
using System.Net.Sockets;
using BASys.DataModels;
using BASys.Services;
using Xunit;

namespace BASys.Tests
{
    /// <summary>
    /// Tests the bus service against a simulated KNXnet/IP tunneling gateway on the loopback interface.
    /// </summary>
    public class EibBusServiceTests
    {
        [Fact]
        public async Task FramesAreReceivedAndSentThroughTunnel()
        {
            using UdpClient gateway = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int gatewayPort = ((IPEndPoint)gateway.Client.LocalEndPoint!).Port;
            TaskCompletionSource<byte[]> sentCemi = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            Task gatewayTask = Task.Run(() => RunGateway(gateway, sentCemi));

            using EibBusService service = new EibBusService();
            TaskCompletionSource<EibFrame> received = new TaskCompletionSource<EibFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
            service.FrameReceived += (sender, frame) => received.TrySetResult(frame);
            EibBusSettings settings = new EibBusSettings { ConnectionType = EibConnectionType.Tunneling, GatewayAddress = "127.0.0.1", GatewayPort = gatewayPort };

            await service.ConnectAsync(settings, CancellationToken.None);
            Assert.True(service.IsConnected);

            EibFrame frame = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal("1.1.1", frame.SourceAddress.ToString());
            Assert.Equal("1/1", frame.DestinationAddress.ToString());
            Assert.Equal("01 ", frame.ApplicationDataText);

            service.SendFrame(new EibFrame(false, 3, EibBusAddress.CreatePhysical(1, 2, 1), EibBusAddress.CreateGroup(1, 257), 6, 0, 0x80, new int[] { 0 }));
            byte[] cemi = await sentCemi.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(new byte[] { 0x11, 0x00, 0xBC, 0xE0, 0x12, 0x01, 0x09, 0x01, 0x01, 0x00, 0x80 }, cemi);

            service.Disconnect();
            Assert.False(service.IsConnected);
            gateway.Close();
            await gatewayTask;
        }

        [Fact]
        public async Task ConnectFailsIfGatewayDoesNotAnswer()
        {
            using UdpClient silent = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)silent.Client.LocalEndPoint!).Port;
            using EibBusService service = new EibBusService();
            EibBusSettings settings = new EibBusSettings { ConnectionType = EibConnectionType.Tunneling, GatewayAddress = "127.0.0.1", GatewayPort = port };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConnectAsync(settings, CancellationToken.None));
            Assert.False(service.IsConnected);
        }

        private static void RunGateway(UdpClient gateway, TaskCompletionSource<byte[]> sentCemi)
        {
            IPEndPoint? client = null;

            try
            {
                while (true)
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] datagram = gateway.Receive(ref remote);
                    int serviceType = (datagram[2] << 8) | datagram[3];

                    if (serviceType == 0x0205)
                    {
                        client = remote;
                        byte[] response = { 0x06, 0x10, 0x02, 0x06, 0x00, 0x14, 0x01, 0x00, 0x08, 0x01, 127, 0, 0, 1, 0x0E, 0x57, 0x04, 0x04, 0x11, 0x01 };
                        gateway.Send(response, response.Length, client);
                        byte[] indication = { 0x06, 0x10, 0x04, 0x20, 0x00, 0x15, 0x04, 0x01, 0x00, 0x00, 0x29, 0x00, 0xBC, 0xE0, 0x11, 0x01, 0x08, 0x01, 0x01, 0x00, 0x81 };
                        gateway.Send(indication, indication.Length, client);
                    }
                    else if (serviceType == 0x0420)
                    {
                        byte[] cemi = new byte[datagram.Length - 10];
                        Array.Copy(datagram, 10, cemi, 0, cemi.Length);
                        sentCemi.TrySetResult(cemi);
                        byte[] ack = { 0x06, 0x10, 0x04, 0x21, 0x00, 0x0A, 0x04, 0x01, datagram[8], 0x00 };
                        gateway.Send(ack, ack.Length, remote);
                    }
                    else if (serviceType == 0x0209)
                    {
                        byte[] response = { 0x06, 0x10, 0x02, 0x0A, 0x00, 0x08, 0x01, 0x00 };
                        gateway.Send(response, response.Length, remote);
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}

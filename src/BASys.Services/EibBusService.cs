using System.Net;
using System.Net.Sockets;
using BASys.DataModels;
using BASys.Services.Mock;
using KNXLib;

namespace BASys.Services
{
    /// <summary>
    /// Bus access based on the knx.net library (KNXnet/IP tunneling or routing). The mockup connection type
    /// uses the simulated bus instead.
    /// </summary>
    public class EibBusService : IEibBusService, IDisposable
    {
        private const int ConnectTimeoutMilliseconds = 5000;

        private readonly object _lock = new object();

        private KnxConnection? _connection;

        private MockEibBusService? _mock;

        private string _connectionType = "-";

        public string ConnectionType
        {
            get
            {
                return _connectionType;
            }
        }

        public bool IsConnected
        {
            get
            {
                bool result;

                lock (_lock)
                {
                    result = _connection != null || _mock != null;
                }

                return result;
            }
        }

        public event EventHandler? ConnectionChanged;

        public event EventHandler<EibFrame>? FrameReceived;

        public async Task ConnectAsync(EibBusSettings settings, CancellationToken cancellationToken)
        {
            Disconnect();

            if (settings.ConnectionType == EibConnectionType.Mock)
            {
                MockEibBusService mock = new MockEibBusService();
                mock.FrameReceived += OnMockFrameReceived;

                lock (_lock)
                {
                    _mock = mock;
                    _connectionType = mock.ConnectionType;
                }

                RaiseConnectionChanged();
            }
            else
            {
                await ConnectKnxAsync(settings, cancellationToken);
            }
        }

        public void Disconnect()
        {
            KnxConnection? connection;
            MockEibBusService? mock;

            lock (_lock)
            {
                connection = _connection;
                mock = _mock;
                _connection = null;
                _mock = null;
            }

            if (connection != null)
            {
                connection.CemiReceived -= OnCemiReceived;
                connection.KnxDisconnectedDelegate = null;

                try
                {
                    connection.Disconnect();
                }
                catch (Exception)
                {
                    // the connection is closed anyway
                }
            }

            if (mock != null)
            {
                mock.FrameReceived -= OnMockFrameReceived;
                mock.Dispose();
            }

            if (connection != null || mock != null)
            {
                RaiseConnectionChanged();
            }
        }

        public void SendFrame(EibFrame frame)
        {
            KnxConnection? connection;
            MockEibBusService? mock;

            lock (_lock)
            {
                connection = _connection;
                mock = _mock;
            }

            if (connection != null)
            {
                connection.SendCemi(EibCemiConverter.ToCemi(frame));
            }
            else if (mock != null)
            {
                mock.SendFrame(frame);
            }
            else
            {
                throw new InvalidOperationException("No EIB connection.");
            }
        }

        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }

        private async Task ConnectKnxAsync(EibBusSettings settings, CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            KnxConnection connection;

            try
            {
                connection = CreateConnection(settings);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(exception.Message, exception);
            }

            connection.KnxConnectedDelegate = () => connected.TrySetResult(true);
            connection.CemiReceived += OnCemiReceived;

            try
            {
                await Task.Run(connection.Connect, cancellationToken);
                Task finished = await Task.WhenAny(connected.Task, Task.Delay(ConnectTimeoutMilliseconds, cancellationToken));

                if (finished != connected.Task)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("The gateway " + settings.GatewayAddress + ":" + settings.GatewayPort + " did not answer.");
                }
            }
            catch (Exception)
            {
                connection.CemiReceived -= OnCemiReceived;
                CloseQuietly(connection);

                throw;
            }

            connection.KnxDisconnectedDelegate = () => OnKnxDisconnected(connection);

            lock (_lock)
            {
                _connection = connection;
                _connectionType = (settings.ConnectionType == EibConnectionType.Routing ? "KNXnet/IP Routing " : "KNXnet/IP Tunneling ")
                    + settings.GatewayAddress;
            }

            RaiseConnectionChanged();
        }

        private static KnxConnection CreateConnection(EibBusSettings settings)
        {
            KnxConnection result;
            string host = string.IsNullOrWhiteSpace(settings.GatewayAddress) ? EibBusSettings.DefaultRoutingAddress : settings.GatewayAddress.Trim();

            if (settings.ConnectionType == EibConnectionType.Routing)
            {
                result = new KnxConnectionRouting(host, settings.GatewayPort);
            }
            else
            {
                IPAddress remoteAddress = ResolveIPv4(host);
                IPAddress localAddress = string.IsNullOrWhiteSpace(settings.LocalAddress) ? FindLocalAddress(remoteAddress, settings.GatewayPort) : IPAddress.Parse(settings.LocalAddress.Trim());
                result = new KnxConnectionTunneling(remoteAddress.ToString(), settings.GatewayPort, localAddress.ToString(), FindFreePort(localAddress));
            }

            return result;
        }

        private static IPAddress ResolveIPv4(string host)
        {
            IPAddress? result;

            if (!IPAddress.TryParse(host, out result))
            {
                result = Dns.GetHostAddresses(host).FirstOrDefault(currentAddress => currentAddress.AddressFamily == AddressFamily.InterNetwork);
            }

            if (result == null)
            {
                throw new InvalidOperationException("Unknown host: " + host);
            }

            return result;
        }

        /// <summary>
        /// Finds the local interface address that is used to reach the gateway.
        /// </summary>
        private static IPAddress FindLocalAddress(IPAddress remoteAddress, int port)
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(remoteAddress, port);
            IPAddress result = ((IPEndPoint)socket.LocalEndPoint!).Address;

            return result;
        }

        private static int FindFreePort(IPAddress localAddress)
        {
            using UdpClient client = new UdpClient(new IPEndPoint(localAddress, 0));
            int result = ((IPEndPoint)client.Client.LocalEndPoint!).Port;

            return result;
        }

        private static void CloseQuietly(KnxConnection connection)
        {
            try
            {
                connection.Disconnect();
            }
            catch (Exception)
            {
                // nothing to clean up if the connection was never established
            }
        }

        private void OnCemiReceived(byte[] cemi)
        {
            EibFrame? frame = EibCemiConverter.FromCemi(cemi);

            if (frame != null)
            {
                FrameReceived?.Invoke(this, frame);
            }
        }

        private void OnMockFrameReceived(object? sender, EibFrame frame)
        {
            FrameReceived?.Invoke(this, frame);
        }

        private void OnKnxDisconnected(KnxConnection connection)
        {
            bool isCurrent;

            lock (_lock)
            {
                isCurrent = ReferenceEquals(_connection, connection);

                if (isCurrent)
                {
                    _connection = null;
                }
            }

            if (isCurrent)
            {
                connection.CemiReceived -= OnCemiReceived;
                RaiseConnectionChanged();
            }
        }

        private void RaiseConnectionChanged()
        {
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

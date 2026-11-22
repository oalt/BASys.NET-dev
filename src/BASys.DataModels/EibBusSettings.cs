namespace BASys.DataModels
{
    /// <summary>
    /// How the application is connected to the EIB bus.
    /// </summary>
    public enum EibConnectionType
    {
        /// <summary>
        /// Simulated bus without any hardware.
        /// </summary>
        Mock,

        /// <summary>
        /// KNXnet/IP tunneling to a gateway.
        /// </summary>
        Tunneling,

        /// <summary>
        /// KNXnet/IP routing (multicast).
        /// </summary>
        Routing
    }

    /// <summary>
    /// The configured bus connection.
    /// </summary>
    public class EibBusSettings
    {
        public const int DefaultGatewayPort = 3671;

        public const string DefaultRoutingAddress = "224.0.23.12";

        public EibConnectionType ConnectionType { get; set; } = EibConnectionType.Mock;

        /// <summary>
        /// IP address or host name of the gateway (the multicast address for routing).
        /// </summary>
        public string GatewayAddress { get; set; } = string.Empty;

        public int GatewayPort { get; set; } = DefaultGatewayPort;

        /// <summary>
        /// Local IP address to use; empty selects it automatically.
        /// </summary>
        public string LocalAddress { get; set; } = string.Empty;

        public EibBusSettings Clone()
        {
            EibBusSettings result = new EibBusSettings();
            result.ConnectionType = ConnectionType;
            result.GatewayAddress = GatewayAddress;
            result.GatewayPort = GatewayPort;
            result.LocalAddress = LocalAddress;

            return result;
        }
    }
}

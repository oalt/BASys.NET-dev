using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Access to the EIB bus: opening the connection, receiving all telegrams and sending single telegrams.
    /// </summary>
    public interface IEibBusService
    {
        /// <summary>
        /// A short text describing the connection (shown in the title of the diagnostic dialogs).
        /// </summary>
        string ConnectionType { get; }

        /// <summary>
        /// True while a connection to the bus (or the simulated bus) is open.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Raised when the connection was opened or closed (also when it was lost). May be raised on any thread.
        /// </summary>
        event EventHandler? ConnectionChanged;

        /// <summary>
        /// Raised for every telegram seen on the bus. May be raised on any thread.
        /// </summary>
        event EventHandler<EibFrame>? FrameReceived;

        /// <summary>
        /// Opens the connection. An already open connection is closed first.
        /// Throws <see cref="InvalidOperationException"/> with a descriptive message if the bus cannot be reached.
        /// </summary>
        Task ConnectAsync(EibBusSettings settings, CancellationToken cancellationToken);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a telegram to the bus. Throws <see cref="InvalidOperationException"/> if there is no connection.
        /// </summary>
        void SendFrame(EibFrame frame);
    }
}

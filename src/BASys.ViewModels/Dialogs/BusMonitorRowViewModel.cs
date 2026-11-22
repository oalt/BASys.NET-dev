using BASys.DataModels;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// One displayed telegram of the tabular bus monitor (immutable, all values are already formatted).
    /// </summary>
    public sealed class BusMonitorRowViewModel
    {
        private readonly EibBusAddress _destination;

        public BusMonitorRowViewModel(EibFrame frame)
        {
            Priority = frame.PriorityText;
            Repeat = frame.Repeat;
            SourceAddress = frame.SourceAddress.ToString();
            _destination = frame.DestinationAddress;
            RoutingCounter = frame.RoutingCounter.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Tpci = frame.TpciText;
            Apci = frame.ApciText;
            Data = frame.ApplicationDataText;
            Parity = frame.Parity.ToString("X2", System.Globalization.CultureInfo.InvariantCulture);
            Acknowledge = frame.Acknowledge.ToString("X", System.Globalization.CultureInfo.InvariantCulture);
        }

        public string Priority { get; }

        public bool Repeat { get; }

        public string SourceAddress { get; }

        public string GetDestinationAddress(bool twoLevelGroup)
        {
            string result = _destination.ToString(twoLevelGroup);

            return result;
        }

        public string RoutingCounter { get; }

        public string Tpci { get; }

        public string Apci { get; }

        public string Data { get; }

        public string Parity { get; }

        public string Acknowledge { get; }
    }
}

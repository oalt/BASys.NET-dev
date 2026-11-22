using BASys.DataModels;

namespace BASys.Services.Mock
{
    /// <summary>
    /// Simulates the EIB bus: sent telegrams are echoed back and, while somebody listens,
    /// some sample group telegrams are generated periodically.
    /// </summary>
    public class MockEibBusService : IDisposable
    {
        private readonly object _lock = new object();

        private readonly Random _random = new Random();

        private EventHandler<EibFrame>? _frameReceived;

        private int _handlerCount;

        private System.Threading.Timer? _timer;

        private int _sampleCounter;

        public string ConnectionType
        {
            get
            {
                return "Mockup";
            }
        }

        public event EventHandler<EibFrame>? FrameReceived
        {
            add
            {
                lock (_lock)
                {
                    _frameReceived += value;
                    _handlerCount++;

                    if (_timer == null)
                    {
                        _timer = new System.Threading.Timer(OnTimer, null, 1500, 2000);
                    }
                }
            }
            remove
            {
                lock (_lock)
                {
                    _frameReceived -= value;
                    _handlerCount--;

                    if (_handlerCount <= 0 && _timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                        _handlerCount = 0;
                    }
                }
            }
        }

        public void SendFrame(EibFrame frame)
        {
            Raise(frame.Clone());
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;
            }

            GC.SuppressFinalize(this);
        }

        private void OnTimer(object? state)
        {
            int counter = Interlocked.Increment(ref _sampleCounter);
            int device = 1 + _random.Next(5);
            EibBusAddress source = EibBusAddress.CreatePhysical(1, 1, device);
            EibBusAddress destination = EibBusAddress.CreateGroup(0, 257 + (counter % 3));
            EibFrame frame;

            if (counter % 4 == 0)
            {
                frame = new EibFrame(false, 3, source, destination, 6, 0, 0x000, new int[] { 0 });
            }
            else
            {
                frame = new EibFrame(false, 3, source, destination, 6, 0, 0x080, new int[] { counter % 2 });
            }

            frame.Acknowledge = 0xC;
            Raise(frame);
        }

        private void Raise(EibFrame frame)
        {
            EventHandler<EibFrame>? handler;

            lock (_lock)
            {
                handler = _frameReceived;
            }

            handler?.Invoke(this, frame);
        }
    }
}

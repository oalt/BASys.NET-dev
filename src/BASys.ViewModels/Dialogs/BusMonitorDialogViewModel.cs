using System.Collections.ObjectModel;
using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// The tabular EIB bus monitor. Shows the received telegrams, the newest one first (port of EIBTelegrammtraceframe).
    /// </summary>
    public partial class BusMonitorDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Maximum number of displayed telegrams; the oldest ones are dropped.
        /// </summary>
        public const int MaxRows = 500;

        private readonly IEibBusService _busService;

        private readonly SynchronizationContext? _context;

        private readonly ObservableCollection<BusMonitorRowViewModel> _rows = new ObservableCollection<BusMonitorRowViewModel>();

        private bool _isListening;

        public BusMonitorDialogViewModel(IEibBusService busService)
        {
            _busService = busService;
            _context = SynchronizationContext.Current;
            Title = "EIB Telegrammtracer - " + busService.ConnectionType;
            _busService.FrameReceived += OnFrameReceived;
            _isListening = true;
        }

        public override double PreferredWidth
        {
            get
            {
                return 1000;
            }
        }

        public override double PreferredHeight
        {
            get
            {
                return 420;
            }
        }

        protected override void OnClosing()
        {
            StopListening();
        }

        private bool _isTwoLevelGroup;

        /// <summary>
        /// True if group addresses are shown as main/sub instead of main/middle/sub.
        /// </summary>
        public bool IsTwoLevelGroup
        {
            get
            {
                return _isTwoLevelGroup;
            }
            set
            {
                SetProperty(ref _isTwoLevelGroup, value);
            }
        }

        public ObservableCollection<BusMonitorRowViewModel> Rows
        {
            get
            {
                return _rows;
            }
        }

        [RelayCommand]
        private void ClearData()
        {
            _rows.Clear();
        }

        [RelayCommand]
        private void CloseDialog()
        {
            Close(true);
        }

        private void StopListening()
        {
            if (_isListening)
            {
                _isListening = false;
                _busService.FrameReceived -= OnFrameReceived;
            }
        }

        private void OnFrameReceived(object? sender, EibFrame frame)
        {
            BusMonitorRowViewModel row = new BusMonitorRowViewModel(frame);

            if (_context != null)
            {
                _context.Post(AddRow, row);
            }
            else
            {
                AddRow(row);
            }
        }

        private void AddRow(object? state)
        {
            BusMonitorRowViewModel? row = state as BusMonitorRowViewModel;

            if (row != null && _isListening)
            {
                _rows.Insert(0, row);

                if (_rows.Count > MaxRows)
                {
                    _rows.RemoveAt(_rows.Count - 1);
                }
            }
        }
    }
}

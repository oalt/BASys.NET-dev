using System.Collections.ObjectModel;
using System.Globalization;
using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// The EIB telegram dialog: composes a telegram, shows its bytes and sends it (port of EIBFrameDialog).
    /// </summary>
    public partial class TelegramDialogViewModel : DialogViewModelBase
    {
        private static readonly string[] PriorityNameList = { "System", "High Priority", "Alarm", "Low Priority" };

        private static readonly string[] DataLengthNameList = { "short Data", "1", "2", "3", "4", "5", "6" };

        private readonly IEibBusService _busService;

        private readonly ObservableCollection<TelegramDataFieldViewModel> _dataFields = new ObservableCollection<TelegramDataFieldViewModel>();

        private bool _isUpdating;

        public TelegramDialogViewModel(IEibBusService busService)
        {
            _busService = busService;
            Title = "EIB Frame Dialog - " + busService.ConnectionType;

            for (int counter = 0; counter < DataLengthNameList.Length; counter++)
            {
                TelegramDataFieldViewModel field = new TelegramDataFieldViewModel();
                field.Text = counter == 0 ? "0" : string.Empty;
                field.IsEditable = counter == 0;
                field.TextChanged += OnFieldChanged;
                _dataFields.Add(field);
            }

            UpdateFrame();
        }

        public IReadOnlyList<string> PriorityNames
        {
            get
            {
                return PriorityNameList;
            }
        }

        public IReadOnlyList<string> DataLengthNames
        {
            get
            {
                return DataLengthNameList;
            }
        }

        public IReadOnlyList<EibTpciOption> TpciOptions
        {
            get
            {
                return EibFrame.TpciOptions;
            }
        }

        public IReadOnlyList<EibApciOption> ApciOptions
        {
            get
            {
                return EibFrame.ApciOptions;
            }
        }

        public ObservableCollection<TelegramDataFieldViewModel> DataFields
        {
            get
            {
                return _dataFields;
            }
        }

        private int _priorityIndex = 3;

        public int PriorityIndex
        {
            get
            {
                return _priorityIndex;
            }
            set
            {
                SetAndUpdate(ref _priorityIndex, value);
            }
        }

        private bool _repeat;

        public bool Repeat
        {
            get
            {
                return _repeat;
            }
            set
            {
                SetAndUpdate(ref _repeat, value);
            }
        }

        private string _sourceZone = "1";

        public string SourceZone
        {
            get
            {
                return _sourceZone;
            }
            set
            {
                SetAndUpdate(ref _sourceZone, value);
            }
        }

        private string _sourceLine = "2";

        public string SourceLine
        {
            get
            {
                return _sourceLine;
            }
            set
            {
                SetAndUpdate(ref _sourceLine, value);
            }
        }

        private string _sourceNode = "1";

        public string SourceNode
        {
            get
            {
                return _sourceNode;
            }
            set
            {
                SetAndUpdate(ref _sourceNode, value);
            }
        }

        private bool _isPhysicalDestination;

        public bool IsPhysicalDestination
        {
            get
            {
                return _isPhysicalDestination;
            }
            set
            {
                if (SetProperty(ref _isPhysicalDestination, value))
                {
                    OnPropertyChanged(nameof(IsDestinationPart3Visible));
                    UpdateFrame();
                }
            }
        }

        private bool _isTwoLevelGroup;

        /// <summary>
        /// True if group destination addresses are entered as main/sub (5/11 bits) instead of main/middle/sub (5/3/8 bits).
        /// </summary>
        public bool IsTwoLevelGroup
        {
            get
            {
                return _isTwoLevelGroup;
            }
            set
            {
                if (_isTwoLevelGroup != value)
                {
                    ConvertGroupParts(value);
                    SetProperty(ref _isTwoLevelGroup, value);
                    OnPropertyChanged(nameof(IsDestinationPart3Visible));
                    UpdateFrame();
                }
            }
        }

        /// <summary>
        /// The third address field is not used for two level group addresses.
        /// </summary>
        public bool IsDestinationPart3Visible
        {
            get
            {
                bool result = _isPhysicalDestination || !_isTwoLevelGroup;

                return result;
            }
        }

        private string _destinationPart1 = "0";

        public string DestinationPart1
        {
            get
            {
                return _destinationPart1;
            }
            set
            {
                SetAndUpdate(ref _destinationPart1, value);
            }
        }

        private string _destinationPart2 = "1";

        public string DestinationPart2
        {
            get
            {
                return _destinationPart2;
            }
            set
            {
                SetAndUpdate(ref _destinationPart2, value);
            }
        }

        private string _destinationPart3 = "1";

        public string DestinationPart3
        {
            get
            {
                return _destinationPart3;
            }
            set
            {
                SetAndUpdate(ref _destinationPart3, value);
            }
        }

        private int _routingCounter = 6;

        public int RoutingCounter
        {
            get
            {
                return _routingCounter;
            }
            set
            {
                SetAndUpdate(ref _routingCounter, value);
            }
        }

        private int _tpciIndex;

        public int TpciIndex
        {
            get
            {
                return _tpciIndex;
            }
            set
            {
                if (SetProperty(ref _tpciIndex, value))
                {
                    OnPropertyChanged(nameof(IsDataEnabled));
                    UpdateFrame();
                }
            }
        }

        private int _apciIndex = 2;

        public int ApciIndex
        {
            get
            {
                return _apciIndex;
            }
            set
            {
                SetAndUpdate(ref _apciIndex, value);
            }
        }

        private int _dataLengthIndex;

        public int DataLengthIndex
        {
            get
            {
                return _dataLengthIndex;
            }
            set
            {
                if (SetProperty(ref _dataLengthIndex, value))
                {
                    ResetDataFields();
                    UpdateFrame();
                }
            }
        }

        /// <summary>
        /// Connection control telegrams (connect, disconnect, ack, nack) carry no application data.
        /// </summary>
        public bool IsDataEnabled
        {
            get
            {
                bool result = !EibFrame.TpciOptions[_tpciIndex].IsControl;

                return result;
            }
        }

        private string _frameText = string.Empty;

        /// <summary>
        /// The composed telegram as hex bytes (including parity and acknowledge).
        /// </summary>
        public string FrameText
        {
            get
            {
                return _frameText;
            }
            private set
            {
                SetProperty(ref _frameText, value);
            }
        }

        private string _errorText = string.Empty;

        /// <summary>
        /// The reason why the last telegram could not be sent (empty if there was no error).
        /// </summary>
        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            private set
            {
                SetProperty(ref _errorText, value);
            }
        }

        [RelayCommand]
        private void Send()
        {
            EibFrame frame = UpdateFrame();

            try
            {
                _busService.SendFrame(frame);
                ErrorText = string.Empty;
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is System.Net.Sockets.SocketException)
            {
                ErrorText = exception.Message;
            }
        }

        [RelayCommand]
        private void CloseDialog()
        {
            Close(true);
        }

        private void SetAndUpdate<T>(ref T field, T value)
        {
            if (SetProperty(ref field, value))
            {
                UpdateFrame();
            }
        }

        private void OnFieldChanged(object? sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                UpdateFrame();
            }
        }

        /// <summary>
        /// Clears and disables the unused data fields after the data length changed.
        /// </summary>
        private void ResetDataFields()
        {
            _isUpdating = true;

            try
            {
                for (int counter = 0; counter < _dataFields.Count; counter++)
                {
                    TelegramDataFieldViewModel field = _dataFields[counter];

                    if (_dataLengthIndex == 0)
                    {
                        field.Text = counter == 0 ? "0" : string.Empty;
                        field.IsEditable = counter == 0;
                    }
                    else
                    {
                        field.Text = counter == 0 ? "---" : counter <= _dataLengthIndex ? "0" : string.Empty;
                        field.IsEditable = counter != 0 && counter <= _dataLengthIndex;
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Parses a number; if the text is invalid the field is reset (the Java dialog resets all fields of an address).
        /// </summary>
        private static bool TryParse(string text, out int value)
        {
            bool result = int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value >= 0;

            return result;
        }

        private EibBusAddress GetSourceAddress()
        {
            int zone;
            int line;
            int node;

            if (!TryParse(_sourceZone, out zone) || !TryParse(_sourceLine, out line) || !TryParse(_sourceNode, out node))
            {
                zone = 1;
                line = 2;
                node = 1;
                SetProperty(ref _sourceZone, "1", nameof(SourceZone));
                SetProperty(ref _sourceLine, "2", nameof(SourceLine));
                SetProperty(ref _sourceNode, "1", nameof(SourceNode));
            }

            EibBusAddress result = EibBusAddress.CreatePhysical(zone, line, node);

            return result;
        }

        /// <summary>
        /// Rewrites the group address fields when switching between the 3 level (main/middle/sub) and the 2 level (main/sub) notation,
        /// so that the entered address stays the same.
        /// </summary>
        private void ConvertGroupParts(bool toTwoLevel)
        {
            int main;
            int second;
            int third;

            if (!_isPhysicalDestination && TryParse(_destinationPart1, out main) && TryParse(_destinationPart2, out second) && TryParse(_destinationPart3, out third))
            {
                if (toTwoLevel)
                {
                    int sub = ((second & 0x07) << 8) | (third & 0xFF);
                    SetProperty(ref _destinationPart2, sub.ToString(CultureInfo.InvariantCulture), nameof(DestinationPart2));
                    SetProperty(ref _destinationPart3, "0", nameof(DestinationPart3));
                }
                else
                {
                    int sub = second & 0x07FF;
                    SetProperty(ref _destinationPart2, (sub >> 8).ToString(CultureInfo.InvariantCulture), nameof(DestinationPart2));
                    SetProperty(ref _destinationPart3, (sub & 0xFF).ToString(CultureInfo.InvariantCulture), nameof(DestinationPart3));
                }
            }
        }

        private EibBusAddress GetDestinationAddress()
        {
            int part1 = 0;
            int part2 = 0;
            int part3 = 0;
            bool isValid = TryParse(_destinationPart1, out part1) && TryParse(_destinationPart2, out part2) && (!IsDestinationPart3Visible || TryParse(_destinationPart3, out part3));

            if (!isValid)
            {
                part1 = 1;
                part2 = 1;
                part3 = 1;
                SetProperty(ref _destinationPart1, "1", nameof(DestinationPart1));
                SetProperty(ref _destinationPart2, "1", nameof(DestinationPart2));
                SetProperty(ref _destinationPart3, "1", nameof(DestinationPart3));
            }

            EibBusAddress result;

            if (_isPhysicalDestination)
            {
                result = EibBusAddress.CreatePhysical(part1, part2, part3);
            }
            else if (_isTwoLevelGroup)
            {
                result = EibBusAddress.CreateGroup(part1, part2);
            }
            else
            {
                result = EibBusAddress.CreateGroup(part1, (part2 << 8) | part3);
            }

            return result;
        }

        /// <summary>
        /// Returns the application data in the layout of EibFrame (short: one 6 bit value; long: unused first element and the bytes).
        /// </summary>
        private int[] GetApplicationData()
        {
            int[] result;
            int value;

            if (!IsDataEnabled)
            {
                result = new int[0];
            }
            else if (_dataLengthIndex == 0)
            {
                if (!TryParse(_dataFields[0].Text, out value))
                {
                    value = 0;
                    _dataFields[0].Text = "0";
                }

                result = new int[] { value & 0x3F };
            }
            else
            {
                result = new int[_dataLengthIndex + 1];

                for (int counter = 1; counter <= _dataLengthIndex; counter++)
                {
                    if (!TryParse(_dataFields[counter].Text, out value))
                    {
                        value = 0;
                        _dataFields[counter].Text = "0";
                    }

                    result[counter] = value & 0xFF;
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the telegram from the entered values and updates the displayed bytes.
        /// </summary>
        private EibFrame UpdateFrame()
        {
            bool wasUpdating = _isUpdating;
            _isUpdating = true;
            EibFrame result;

            try
            {
                EibTpciOption tpci = EibFrame.TpciOptions[_tpciIndex];
                int apci = tpci.IsControl ? tpci.ApciHighBits << 8 : EibFrame.ApciOptions[_apciIndex].Apci;
                int[] data = GetApplicationData();

                result = new EibFrame(_repeat, _priorityIndex, GetSourceAddress(), GetDestinationAddress(), _routingCounter, tpci.Tpci, apci, data);
                result.IsShortData = !tpci.IsControl && _dataLengthIndex == 0;
                FrameText = result.ToHexString();
            }
            finally
            {
                _isUpdating = wasUpdating;
            }

            return result;
        }
    }
}

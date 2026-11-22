using System.Collections.ObjectModel;
using System.Globalization;
using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for administering the EIB device database ("EIB Gerätedaten bearbeiten"): devices are selected from the
    /// product data of a manufacturer, their communication objects are assigned functions and the device is added to eib-devices.xml.
    /// </summary>
    public partial class EibDataDialogViewModel : DialogViewModelBase
    {
        private readonly IEibDataService _eibDataService;

        private readonly ILocalizationService _localizer;

        private readonly IDialogService _dialogService;

        private readonly ObservableCollection<ComObjectRowViewModel> _comObjects = new ObservableCollection<ComObjectRowViewModel>();

        private readonly ObservableCollection<DatabaseDeviceRowViewModel> _databaseDevices = new ObservableCollection<DatabaseDeviceRowViewModel>();

        public EibDataDialogViewModel(IEibDataService eibDataService, ILocalizationService localizer, IDialogService dialogService)
        {
            _eibDataService = eibDataService;
            _localizer = localizer;
            _dialogService = dialogService;
            Title = localizer["tit.eibdataadmin"];
            ManufacturerFiles = eibDataService.GetManufacturerFiles();
            ReloadDatabaseDevices();
        }

        public IReadOnlyList<string> ManufacturerFiles { get; }

        public ObservableCollection<ComObjectRowViewModel> ComObjects
        {
            get
            {
                return _comObjects;
            }
        }

        public ObservableCollection<DatabaseDeviceRowViewModel> DatabaseDevices
        {
            get
            {
                return _databaseDevices;
            }
        }

        private bool _isBusy;

        /// <summary>
        /// True while product data are read from a (possibly very large) file.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    AddDeviceCommand.NotifyCanExecuteChanged();
                    SetFunctionGroupCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string? _selectedManufacturerFile;

        public string? SelectedManufacturerFile
        {
            get
            {
                return _selectedManufacturerFile;
            }
            set
            {
                if (SetProperty(ref _selectedManufacturerFile, value))
                {
                    _ = LoadProductsAsync();
                }
            }
        }

        private IReadOnlyList<EibProduct> _products = new List<EibProduct>();

        public IReadOnlyList<EibProduct> Products
        {
            get
            {
                return _products;
            }
            private set
            {
                SetProperty(ref _products, value);
            }
        }

        private EibProduct? _selectedProduct;

        public EibProduct? SelectedProduct
        {
            get
            {
                return _selectedProduct;
            }
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    _ = LoadApplicationsAsync();
                }
            }
        }

        private IReadOnlyList<EibApplication> _applications = new List<EibApplication>();

        public IReadOnlyList<EibApplication> Applications
        {
            get
            {
                return _applications;
            }
            private set
            {
                SetProperty(ref _applications, value);
            }
        }

        private EibApplication? _selectedApplication;

        public EibApplication? SelectedApplication
        {
            get
            {
                return _selectedApplication;
            }
            set
            {
                if (SetProperty(ref _selectedApplication, value))
                {
                    AddDeviceCommand.NotifyCanExecuteChanged();
                    SetFunctionGroupCommand.NotifyCanExecuteChanged();
                    _ = LoadComObjectsAsync();
                }
            }
        }

        private EibDeviceKind _deviceKind = EibDeviceKind.Sensor;

        public EibDeviceKind DeviceKind
        {
            get
            {
                return _deviceKind;
            }
            set
            {
                SetProperty(ref _deviceKind, value);
            }
        }

        private InstallationLocation _location = InstallationLocation.DinRail;

        public InstallationLocation Location
        {
            get
            {
                return _location;
            }
            set
            {
                SetProperty(ref _location, value);
            }
        }

        private DatabaseDeviceRowViewModel? _selectedDatabaseDevice;

        public DatabaseDeviceRowViewModel? SelectedDatabaseDevice
        {
            get
            {
                return _selectedDatabaseDevice;
            }
            set
            {
                if (SetProperty(ref _selectedDatabaseDevice, value))
                {
                    DeleteCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public void ToggleSelection(ComObjectRowViewModel row)
        {
            row.IsSelected = !row.IsSelected;
        }

        private async Task LoadProductsAsync()
        {
            Products = new List<EibProduct>();
            SelectedProduct = null;

            if (_selectedManufacturerFile != null)
            {
                IsBusy = true;

                try
                {
                    Products = await _eibDataService.GetProductsAsync(_selectedManufacturerFile);
                }
                catch (Exception exception) when (exception is IOException || exception is System.Xml.XmlException)
                {
                    await _dialogService.ShowMessageAsync(_localizer["tit.info"], exception.Message);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task LoadApplicationsAsync()
        {
            Applications = new List<EibApplication>();
            SelectedApplication = null;

            if (_selectedManufacturerFile != null && _selectedProduct != null)
            {
                IsBusy = true;

                try
                {
                    Applications = await _eibDataService.GetApplicationsAsync(_selectedManufacturerFile, _selectedProduct.Id);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task LoadComObjectsAsync()
        {
            _comObjects.Clear();

            if (_selectedManufacturerFile != null && _selectedApplication != null)
            {
                IsBusy = true;

                try
                {
                    List<EibCommunicationObject> communicationObjects = await _eibDataService.GetCommunicationObjectsAsync(_selectedManufacturerFile, _selectedApplication.ProgramId);

                    foreach (EibCommunicationObject currentObject in communicationObjects)
                    {
                        _comObjects.Add(new ComObjectRowViewModel(currentObject));
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Groups the selected communication objects to a function group (only if none of them already belongs to a group).
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditFunctions))]
        private void SetFunctionGroup()
        {
            List<ComObjectRowViewModel> selectedRows = _comObjects.Where(currentRow => currentRow.IsSelected).ToList();
            bool alreadyGrouped = selectedRows.Any(currentRow => currentRow.RelatedNumbers.Count > 0);

            if (selectedRows.Count > 0 && !alreadyGrouped)
            {
                List<string> numbers = selectedRows.Select(currentRow => currentRow.CommunicationObject.Number).ToList();

                foreach (ComObjectRowViewModel currentRow in selectedRows)
                {
                    currentRow.RelatedNumbers = numbers;
                    currentRow.IsSelected = false;
                }
            }
        }

        private bool CanEditFunctions()
        {
            bool result = !_isBusy && _selectedApplication != null;

            return result;
        }

        [RelayCommand(CanExecute = nameof(CanEditFunctions))]
        private async Task AddDeviceAsync()
        {
            if (_selectedManufacturerFile != null && _selectedProduct != null && _selectedApplication != null)
            {
                IsBusy = true;

                try
                {
                    string manufacturerName = await _eibDataService.CompleteApplicationAsync(_selectedManufacturerFile, _selectedApplication);
                    EibDevicesDataModel model = _eibDataService.Devices;
                    string deviceId = model.AddDevice(_selectedProduct.Name);
                    model.SetManufacturer(deviceId, manufacturerName);
                    model.SetInstallationType(deviceId, XmlCodes.ToCode(_location));
                    model.SetDeviceType(deviceId, XmlCodes.ToCode(_deviceKind));
                    model.SetApplication(deviceId, _selectedApplication);

                    foreach (List<EibDeviceFunction> currentGroup in GetDeviceFunctions())
                    {
                        string groupId = model.AddFunctionGroup(deviceId);

                        foreach (EibDeviceFunction currentFunction in currentGroup)
                        {
                            model.AddFunction(groupId, currentFunction);
                        }
                    }

                    _eibDataService.SaveDevices();
                    ReloadDatabaseDevices();
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Builds the function groups from the function types and groupings chosen in the table of communication objects.
        /// </summary>
        private List<List<EibDeviceFunction>> GetDeviceFunctions()
        {
            List<List<EibDeviceFunction>> result = new List<List<EibDeviceFunction>>();
            bool[] done = new bool[_comObjects.Count];

            for (int startIndex = 0; startIndex < _comObjects.Count; startIndex++)
            {
                List<EibDeviceFunction> group = new List<EibDeviceFunction>();
                List<string>? relatedNumbers = null;

                for (int index = startIndex; index < _comObjects.Count; index++)
                {
                    ComObjectRowViewModel currentRow = _comObjects[index];
                    bool used = currentRow.FunctionType != "unused";

                    if (currentRow.RelatedNumbers.Count == 0 && !done[index] && group.Count == 0 && used)
                    {
                        group.Add(CreateFunction(currentRow));
                        done[index] = true;
                        break;
                    }

                    relatedNumbers ??= currentRow.RelatedNumbers;

                    foreach (string currentNumber in relatedNumbers)
                    {
                        if (currentNumber == currentRow.CommunicationObject.Number && !done[index] && used)
                        {
                            group.Add(CreateFunction(currentRow));
                            done[index] = true;
                        }
                    }
                }

                if (group.Count > 0)
                {
                    result.Add(group);
                }
            }

            return result;
        }

        private static EibDeviceFunction CreateFunction(ComObjectRowViewModel row)
        {
            int comObject;
            int eisType;
            int.TryParse(row.CommunicationObject.Number, NumberStyles.Integer, CultureInfo.InvariantCulture, out comObject);
            int.TryParse(row.CommunicationObject.Type, NumberStyles.Integer, CultureInfo.InvariantCulture, out eisType);
            EibDeviceFunction result = new EibDeviceFunction
            {
                Name = row.CommunicationObject.Name,
                ComObject = comObject,
                EisType = eisType,
                Type = row.FunctionType,
                FunctionName = row.CommunicationObject.Function
            };

            return result;
        }

        [RelayCommand(CanExecute = nameof(HasDatabaseDeviceSelection))]
        private void Delete()
        {
            if (_selectedDatabaseDevice != null)
            {
                _eibDataService.Devices.RemoveData(_selectedDatabaseDevice.Id);
                _eibDataService.SaveDevices();
                SelectedDatabaseDevice = null;
                ReloadDatabaseDevices();
            }
        }

        private bool HasDatabaseDeviceSelection()
        {
            bool result = _selectedDatabaseDevice != null;

            return result;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            _eibDataService.SaveDevices();
            Close(true);
        }

        private void ReloadDatabaseDevices()
        {
            _databaseDevices.Clear();

            foreach (string currentId in _eibDataService.Devices.GetDeviceIds())
            {
                _databaseDevices.Add(new DatabaseDeviceRowViewModel(_eibDataService.Devices, currentId));
            }
        }
    }
}

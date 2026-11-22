using BASys.DataModels;
using BASys.Services;
using BASys.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels
{
    /// <summary>
    /// View model of the main window: menu, toolbar, status bar and the opened project.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private const string ApplicationTitle = "BASys 2003";

        private readonly ILocalizationService _localizer;

        private readonly IAppSettingsService _settings;

        private readonly IProjectService _projectService;

        private readonly IEibDataService _eibDataService;

        private readonly IEibProgrammingService _programmingService;

        private readonly IEibBusService _busService;

        private readonly IFileDialogService _fileDialogs;

        private readonly DialogHostViewModel _dialogs;

        private readonly IToolWindowService _toolWindows;

        private readonly SynchronizationContext? _context;

        private BusMonitorDialogViewModel? _busMonitorDialog;

        private TelegramDialogViewModel? _telegramDialog;

        private readonly AddEndDeviceWorkflow _addEndDeviceWorkflow;

        private bool _isInitialized;

        public MainViewModel(ILocalizationService localizer, IAppSettingsService settings, IProjectService projectService,
            IEibDataService eibDataService, IEibProgrammingService programmingService, IEibBusService busService,
            IFileDialogService fileDialogs, DialogHostViewModel dialogs, IToolWindowService toolWindows)
        {
            _busService = busService;
            _localizer = localizer;
            _settings = settings;
            _projectService = projectService;
            _eibDataService = eibDataService;
            _programmingService = programmingService;
            _fileDialogs = fileDialogs;
            _dialogs = dialogs;
            _toolWindows = toolWindows;
            _addEndDeviceWorkflow = new AddEndDeviceWorkflow(localizer, dialogs, eibDataService);
            _statusText = localizer["stat.ready"];
            _context = SynchronizationContext.Current;
            _busService.ConnectionChanged += OnBusConnectionChanged;
        }

        /// <summary>
        /// Raised when the application should be closed.
        /// </summary>
        public event EventHandler? ExitRequested;

        public DialogHostViewModel Dialogs
        {
            get
            {
                return _dialogs;
            }
        }

        private ProjectViewModel? _project;

        public ProjectViewModel? Project
        {
            get
            {
                return _project;
            }
            private set
            {
                ProjectViewModel? oldProject = _project;

                if (SetProperty(ref _project, value))
                {
                    if (oldProject != null)
                    {
                        oldProject.Modified -= OnProjectModified;
                    }

                    if (_project != null)
                    {
                        _project.Modified += OnProjectModified;
                    }

                    OnPropertyChanged(nameof(IsProjectOpen));
                    OnPropertyChanged(nameof(IsModified));
                    OnPropertyChanged(nameof(Title));
                    RefreshCommandStates();
                }
            }
        }

        public bool IsProjectOpen
        {
            get
            {
                return _project != null;
            }
        }

        public bool IsModified
        {
            get
            {
                bool result = _project != null && _project.Model.IsModified;

                return result;
            }
        }

        public string Title
        {
            get
            {
                string result = ApplicationTitle;

                if (_project != null)
                {
                    result += " - " + _project.Name;

                    if (IsModified)
                    {
                        result += " *";
                    }
                }

                return result;
            }
        }

        private string _statusText;

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                SetProperty(ref _statusText, value);
            }
        }

        private GatewayType _selectedGateway = GatewayType.KnxNetIp;

        public GatewayType SelectedGateway
        {
            get
            {
                return _selectedGateway;
            }
            set
            {
                SetProperty(ref _selectedGateway, value);
            }
        }

        private bool _isGatewayRunning;

        public bool IsGatewayRunning
        {
            get
            {
                return _isGatewayRunning;
            }
            private set
            {
                if (SetProperty(ref _isGatewayRunning, value))
                {
                    OnPropertyChanged(nameof(CanSelectGateway));
                    RefreshCommandStates();
                }
            }
        }

        public bool CanSelectGateway
        {
            get
            {
                bool result = !_isGatewayRunning;

                return result;
            }
        }

        /// <summary>
        /// Called once when the main window is shown: applies the configured language or, at the first start,
        /// asks for the language and creates the local data directory.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                if (_settings.IsConfigured)
                {
                    _localizer.SetCulture(_settings.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "de");
                }
                else
                {
                    await _dialogs.ShowAsync(new LanguageDialogViewModel(_localizer, _settings));
                    string message = _localizer["mess.welcome"] + _settings.BaseDirectory;
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], message);
                }

                RefreshLanguage();
            }
        }

        /// <summary>
        /// Resets the status bar text after a language change.
        /// </summary>
        public void RefreshLanguage()
        {
            StatusText = _localizer["stat.ready"];
        }

        // ---------------------------------------------------------------- file menu

        [RelayCommand]
        private async Task NewProjectAsync()
        {
            if (await CloseProjectCoreAsync())
            {
                NewProjectDialogViewModel dialog = new NewProjectDialogViewModel(_localizer);

                if (await _dialogs.ShowAsync(dialog))
                {
                    OpenProjectModel(_projectService.CreateProject(dialog.ProjectName.Trim(), dialog.PreferredBusSystem));
                }
            }
        }

        [RelayCommand]
        private async Task OpenProjectAsync()
        {
            if (await CloseProjectCoreAsync())
            {
                string? directory = _fileDialogs.PickFolder(_localizer["mi.openproject"], _projectService.ProjectsDirectory);

                if (directory != null)
                {
                    await OpenProjectDirectoryAsync(directory);
                }
            }
        }

        private async Task OpenProjectDirectoryAsync(string directory)
        {
            if (!_projectService.IsProjectPath(directory))
            {
                await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.wrongProjectPath"]);
            }
            else
            {
                try
                {
                    OpenProjectModel(_projectService.OpenProject(directory));
                }
                catch (Exception exception) when (exception is IOException || exception is System.Xml.XmlException)
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], exception.Message);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(IsProjectOpen))]
        private async Task CloseProjectAsync()
        {
            await CloseProjectCoreAsync();
        }

        /// <summary>
        /// Closes the project after asking whether unsaved changes should be saved. Returns false if the user cancelled.
        /// </summary>
        private async Task<bool> CloseProjectCoreAsync()
        {
            bool result = true;

            if (_project != null)
            {
                if (_project.Model.IsModified)
                {
                    SaveChoice choice = await _dialogs.ShowConfirmSaveAsync(_localizer["tit.question"], _localizer["mess.saveChanges"]);

                    if (choice == SaveChoice.Cancel)
                    {
                        result = false;
                    }
                    else if (choice == SaveChoice.Save)
                    {
                        result = await SaveCoreAsync(null);
                    }
                }

                if (result)
                {
                    Project = null;
                    StatusText = _localizer["stat.ready"];
                }
            }

            return result;
        }

        [RelayCommand(CanExecute = nameof(IsModified))]
        private async Task SaveAsync()
        {
            await SaveCoreAsync(null);
        }

        [RelayCommand(CanExecute = nameof(IsProjectOpen))]
        private async Task SaveAsAsync()
        {
            string? directory = _fileDialogs.PickFolder(_localizer["mi.saveas"], _projectService.ProjectsDirectory);

            if (directory != null)
            {
                await SaveCoreAsync(directory);
            }
        }

        private async Task<bool> SaveCoreAsync(string? targetDirectory)
        {
            bool result = false;

            if (_project != null)
            {
                try
                {
                    _projectService.SaveProject(_project.Model, targetDirectory);
                    StatusText = _localizer["mess.saved"] + " " + _project.Model.ProjectPath;
                    result = true;
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], exception.Message);
                }

                OnPropertyChanged(nameof(IsModified));
                OnPropertyChanged(nameof(Title));
                RefreshCommandStates();
            }

            return result;
        }

        [RelayCommand]
        private async Task ExitAsync()
        {
            if (await CloseProjectCoreAsync())
            {
                _eibDataService.SaveDevices();
                ExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // ---------------------------------------------------------------- edit menu (not yet functional)

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Print()
        {
        }

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Undo()
        {
        }

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Redo()
        {
        }

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Cut()
        {
        }

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Copy()
        {
        }

        [RelayCommand(CanExecute = nameof(IsNeverEnabled))]
        private void Paste()
        {
        }

        private bool IsNeverEnabled()
        {
            bool result = false;

            return result;
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void Delete()
        {
            _project?.DeleteSelectedNode();
            RefreshCommandStates();
        }

        private bool CanDelete()
        {
            bool result = _project != null && _project.SelectedNode != null && _project.SelectedNode.Parent != null;

            return result;
        }

        // ---------------------------------------------------------------- commissioning, diagnostic and data menu

        [RelayCommand(CanExecute = nameof(IsProjectOpen))]
        private async Task ProgramEibDevicesAsync()
        {
            if (_project != null)
            {
                EibProgrammingDialogViewModel dialog = new EibProgrammingDialogViewModel(_project.Model, _localizer, _programmingService, _dialogs);
                await _dialogs.ShowAsync(dialog);
            }
        }

        /// <summary>
        /// Opens the tabular bus monitor (only one instance at a time, it does not block the main window).
        /// </summary>
        [RelayCommand]
        private async Task OpenBusMonitorAsync()
        {
            if (_busMonitorDialog == null && await EnsureBusConnectionAsync())
            {
                BusMonitorDialogViewModel dialog = new BusMonitorDialogViewModel(_busService);
                dialog.CloseRequested += OnBusMonitorClosed;
                _busMonitorDialog = dialog;
                _toolWindows.Show(dialog);
            }
        }

        private void OnBusMonitorClosed(object? sender, bool dialogResult)
        {
            _busMonitorDialog = null;
        }

        /// <summary>
        /// Opens the EIB telegram dialog (only one instance at a time, it does not block the main window).
        /// </summary>
        [RelayCommand]
        private async Task OpenTelegramDialogAsync()
        {
            if (_telegramDialog == null && await EnsureBusConnectionAsync())
            {
                TelegramDialogViewModel dialog = new TelegramDialogViewModel(_busService);
                dialog.CloseRequested += OnTelegramDialogClosed;
                _telegramDialog = dialog;
                await _dialogs.ShowAsync(dialog);
            }
        }

        private void OnTelegramDialogClosed(object? sender, bool dialogResult)
        {
            _telegramDialog = null;
        }

        [RelayCommand]
        private async Task EditEibDataAsync()
        {
            EibDataDialogViewModel dialog = new EibDataDialogViewModel(_eibDataService, _localizer, _dialogs);
            await _dialogs.ShowAsync(dialog);
        }

        /// <summary>
        /// Converts an ETS product data file (*.vd_) into the XML format used for the EIB device data.
        /// </summary>
        [RelayCommand]
        private async Task ConvertVdFileAsync()
        {
            string? vdFile = _fileDialogs.PickFile(_localizer["tit.chooseVDFile"], "ETS product data (*.vd_)|*.vd_", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            string? xmlName = vdFile != null ? await _dialogs.ShowPromptAsync(_localizer["tit.input"], _localizer["mess.inputXMLname"], Path.GetFileNameWithoutExtension(vdFile)) : null;

            if (vdFile != null && !string.IsNullOrWhiteSpace(xmlName))
            {
                string xmlFileName = xmlName.Trim().EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? xmlName.Trim() : xmlName.Trim() + ".xml";
                StatusText = _localizer["stat.converting"];

                try
                {
                    await _eibDataService.ConvertVdFileAsync(vdFile, xmlFileName);
                    StatusText = _localizer["stat.ready"];
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.conversionDone"] + " " + xmlFileName);
                }
                catch (Exception exception) when (exception is IOException || exception is InvalidDataException || exception is UnauthorizedAccessException)
                {
                    StatusText = _localizer["stat.ready"];
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], exception.Message);
                }
            }
        }

        [RelayCommand]
        private async Task NotImplementedAsync()
        {
            await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.notImplemented"]);
        }

        [RelayCommand]
        private void SelectGateway(GatewayType gatewayType)
        {
            SelectedGateway = gatewayType;
        }

        [RelayCommand(CanExecute = nameof(CanSelectGateway))]
        private async Task StartGatewayAsync()
        {
            EibBusSettings settings = _settings.LoadBusSettings();
            bool canConnect = true;

            if (SelectedGateway == GatewayType.KnxNetIp)
            {
                string? address = await _dialogs.ShowPromptAsync(_localizer["mi.gatewayKNXnetIP"], _localizer["l.gatewayAddress"], settings.GatewayAddress);
                canConnect = !string.IsNullOrWhiteSpace(address);

                if (canConnect)
                {
                    settings.ConnectionType = EibConnectionType.Tunneling;
                    settings.GatewayAddress = address!.Trim();
                    _settings.SaveBusSettings(settings);
                }
            }
            else
            {
                settings.ConnectionType = EibConnectionType.Mock;
            }

            if (canConnect)
            {
                StatusText = _localizer["stat.gatewayConnecting"];

                try
                {
                    await _busService.ConnectAsync(settings, CancellationToken.None);
                    IsGatewayRunning = true;
                    StatusText = _localizer["stat.gatewayStarted"] + " " + _busService.ConnectionType;
                }
                catch (InvalidOperationException exception)
                {
                    StatusText = _localizer["stat.ready"];
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noEIBConnectionAvaliable"] + "\n\n" + exception.Message);
                }
                catch (Exception exception) when (exception is System.Net.Sockets.SocketException || exception is OperationCanceledException)
                {
                    StatusText = _localizer["stat.ready"];
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noEIBConnectionAvaliable"] + "\n\n" + exception.Message);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(IsGatewayRunning))]
        private void StopGateway()
        {
            _busService.Disconnect();
            IsGatewayRunning = false;
            StatusText = _localizer["stat.gatewayStopped"];
        }

        /// <summary>
        /// The connection may be closed by the bus service itself (for example when the gateway disappears).
        /// </summary>
        private void OnBusConnectionChanged(object? sender, EventArgs e)
        {
            if (_context != null)
            {
                _context.Post(UpdateGatewayState, null);
            }
            else
            {
                UpdateGatewayState(null);
            }
        }

        private void UpdateGatewayState(object? state)
        {
            if (IsGatewayRunning && !_busService.IsConnected)
            {
                IsGatewayRunning = false;
                StatusText = _localizer["stat.gatewayLost"];
            }
        }

        /// <summary>
        /// Shows a message and returns false if no EIB connection is open (the diagnostic dialogs need one).
        /// </summary>
        private async Task<bool> EnsureBusConnectionAsync()
        {
            bool result = _busService.IsConnected;

            if (!result)
            {
                await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noEIBConnectionOpen"]);
            }

            return result;
        }

        // ---------------------------------------------------------------- help menu

        [RelayCommand]
        private async Task AboutAsync()
        {
            await _dialogs.ShowAsync(new AboutDialogViewModel(_localizer));
        }

        [RelayCommand]
        private async Task SelectLanguageAsync()
        {
            await _dialogs.ShowAsync(new LanguageDialogViewModel(_localizer, _settings));
        }

        // ---------------------------------------------------------------- structural view

        /// <summary>
        /// Adds a new element from the palette to the selected node.
        /// </summary>
        [RelayCommand]
        private async Task AddElementAsync(PaletteItemType itemType)
        {
            await AddElementToAsync(itemType, null);
        }

        /// <summary>
        /// Adds a new element to the given target node (e.g. after a drop on the tree).
        /// </summary>
        public async Task AddElementToAsync(PaletteItemType itemType, ArchitecturalNodeViewModel? target)
        {
            if (_project != null)
            {
                ArchitecturalNodeViewModel? host = _project.FindHost(itemType, target);

                if (host == null)
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer[GetMissingHostMessageKey(itemType)]);
                }
                else if (itemType == PaletteItemType.Sensor)
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.notImplemented"]);
                }
                else if (itemType.IsEndDevice())
                {
                    await _addEndDeviceWorkflow.ExecuteAsync(_project.Model, host.Id, itemType.ToEndDeviceType());
                    _project.RefreshSelectedRoom();
                }
                else
                {
                    string? name = await _dialogs.ShowPromptAsync(_localizer["tit.input"], _localizer["mess.inputname"], string.Empty);

                    if (name != null)
                    {
                        _project.AddStructuralElement(host, itemType, string.IsNullOrWhiteSpace(name) ? _localizer["noname"] : name.Trim());
                    }
                }
            }
        }

        private void OpenProjectModel(Project model)
        {
            Project = new ProjectViewModel(model);
            StatusText = _localizer["stat.ready"];
        }

        private void OnProjectModified(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(Title));
            RefreshCommandStates();
        }

        private void RefreshCommandStates()
        {
            CloseProjectCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            SaveAsCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
            ProgramEibDevicesCommand.NotifyCanExecuteChanged();
            StartGatewayCommand.NotifyCanExecuteChanged();
            StopGatewayCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Must be called when the selection in the project tree changed.
        /// </summary>
        public void NotifySelectionChanged()
        {
            DeleteCommand.NotifyCanExecuteChanged();
        }

        private static string GetMissingHostMessageKey(PaletteItemType itemType)
        {
            string result = "mess.selectRoom";

            switch (itemType)
            {
                case PaletteItemType.Floor:
                    result = "mess.selectBuilding";
                    break;
                case PaletteItemType.Room:
                    result = "mess.selectFloor";
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}

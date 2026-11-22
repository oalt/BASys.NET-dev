using System.Collections.ObjectModel;
using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for programming EIB devices (the programming is simulated in the mockup).
    /// </summary>
    public partial class EibProgrammingDialogViewModel : DialogViewModelBase
    {
        private readonly Project _project;

        private readonly ILocalizationService _localizer;

        private readonly IEibProgrammingService _programmingService;

        private readonly IDialogService _dialogService;

        private readonly ObservableCollection<ProgrammingDeviceRowViewModel> _devices = new ObservableCollection<ProgrammingDeviceRowViewModel>();

        private CancellationTokenSource? _cancellationSource;

        public EibProgrammingDialogViewModel(Project project, ILocalizationService localizer,
            IEibProgrammingService programmingService, IDialogService dialogService)
        {
            _project = project;
            _localizer = localizer;
            _programmingService = programmingService;
            _dialogService = dialogService;
            Title = localizer["tit.programEIB"];

            foreach (string currentDeviceId in project.Installation.GetUnprogrammedEibDeviceIds())
            {
                _devices.Add(new ProgrammingDeviceRowViewModel(project, currentDeviceId));
            }
        }

        public ObservableCollection<ProgrammingDeviceRowViewModel> Devices
        {
            get
            {
                return _devices;
            }
        }

        private bool _addressOnly;

        public bool AddressOnly
        {
            get
            {
                return _addressOnly;
            }
            set
            {
                SetProperty(ref _addressOnly, value);
            }
        }

        private bool _isProgramming;

        public bool IsProgramming
        {
            get
            {
                return _isProgramming;
            }
            private set
            {
                if (SetProperty(ref _isProgramming, value))
                {
                    ProgramAllCommand.NotifyCanExecuteChanged();
                    ProgramSelectedCommand.NotifyCanExecuteChanged();
                    CancelProgrammingCommand.NotifyCanExecuteChanged();
                    CloseDialogCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private int _deviceProgress;

        public int DeviceProgress
        {
            get
            {
                return _deviceProgress;
            }
            private set
            {
                SetProperty(ref _deviceProgress, value);
            }
        }

        private int _totalProgress;

        public int TotalProgress
        {
            get
            {
                return _totalProgress;
            }
            private set
            {
                SetProperty(ref _totalProgress, value);
            }
        }

        private string _statusLine1 = " ";

        public string StatusLine1
        {
            get
            {
                return _statusLine1;
            }
            private set
            {
                SetProperty(ref _statusLine1, value);
            }
        }

        private string _statusLine2 = " ";

        public string StatusLine2
        {
            get
            {
                return _statusLine2;
            }
            private set
            {
                SetProperty(ref _statusLine2, value);
            }
        }

        public void ToggleSelection(ProgrammingDeviceRowViewModel row)
        {
            if (!_isProgramming)
            {
                row.IsSelected = !row.IsSelected;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartProgramming))]
        private async Task ProgramAllAsync()
        {
            await ProgramDevicesAsync(_devices.ToList());
        }

        [RelayCommand(CanExecute = nameof(CanStartProgramming))]
        private async Task ProgramSelectedAsync()
        {
            List<ProgrammingDeviceRowViewModel> selectedRows = _devices.Where(currentRow => currentRow.IsSelected).ToList();

            await ProgramDevicesAsync(selectedRows);
        }

        private bool CanStartProgramming()
        {
            bool result = !_isProgramming;

            return result;
        }

        [RelayCommand(CanExecute = nameof(IsProgramming))]
        private void CancelProgramming()
        {
            _cancellationSource?.Cancel();
        }

        [RelayCommand(CanExecute = nameof(CanStartProgramming))]
        private void CloseDialog()
        {
            Close(true);
        }

        private void OnDeviceProgress(int percent)
        {
            DeviceProgress = percent;
        }

        private async Task ProgramDevicesAsync(List<ProgrammingDeviceRowViewModel> rows)
        {
            if (rows.Count == 0)
            {
                await _dialogService.ShowMessageAsync(_localizer["tit.programEIB"], _localizer["mess.nodevicesToProgram"]);
            }
            else
            {
                _cancellationSource = new CancellationTokenSource();
                IsProgramming = true;
                TotalProgress = 0;
                StatusLine2 = _localizer["mess.progStart"];

                try
                {
                    for (int counter = 0; counter < rows.Count; counter++)
                    {
                        ProgrammingDeviceRowViewModel currentRow = rows[counter];
                        StatusLine1 = _localizer["stat.actualDevice"] + " " + currentRow.Name;
                        Progress<int> progress = new Progress<int>(OnDeviceProgress);
                        await _programmingService.ProgramDeviceAsync(_project, currentRow.Id, _addressOnly, progress, _cancellationSource.Token);
                        currentRow.RefreshState();
                        TotalProgress = (counter + 1) * 100 / rows.Count;
                    }

                    StatusLine1 = _localizer["mess.progEnd"];
                    StatusLine2 = _localizer["stat.progSucc"];
                }
                catch (OperationCanceledException)
                {
                    StatusLine2 = _localizer["mess.stopProg"];
                }
                finally
                {
                    IsProgramming = false;
                    DeviceProgress = 0;
                }
            }
        }
    }
}

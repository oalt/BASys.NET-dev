using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for creating a new end device (lamp, valve, ...).
    /// </summary>
    public partial class NewEndDeviceDialogViewModel : DialogViewModelBase
    {
        private readonly EndDeviceType _deviceType;

        public NewEndDeviceDialogViewModel(EndDeviceType deviceType, bool showBusSystemChoice, BusSystem busSystem, ILocalizationService localizer)
        {
            _deviceType = deviceType;
            _name = localizer["noname"];
            ShowBusSystemChoice = showBusSystemChoice;
            _busSystem = busSystem == BusSystem.None ? BusSystem.Eib : busSystem;

            switch (deviceType)
            {
                case EndDeviceType.Lamp:
                    Title = localizer["tit.newLamp"];
                    IconName = "lightbulb";
                    break;
                case EndDeviceType.DimmableLamp:
                    Title = localizer["tit.newDimmableLamp"];
                    IconName = "lightbulb-fill";
                    break;
                case EndDeviceType.Valve:
                    Title = localizer["tit.newValve"];
                    IconName = "droplet";
                    break;
                case EndDeviceType.Jalousie:
                    Title = localizer["tit.newJal"];
                    IconName = "distribute-vertical";
                    break;
                default:
                    Title = localizer["tit.newSensor"];
                    IconName = "broadcast-pin";
                    break;
            }
        }

        public EndDeviceType DeviceType
        {
            get
            {
                return _deviceType;
            }
        }

        public string IconName { get; }

        /// <summary>
        /// The bus system is only asked if the project has no preferred bus system.
        /// </summary>
        public bool ShowBusSystemChoice { get; }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OkCommand.NotifyCanExecuteChanged();
                }
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

        private BusSystem _busSystem;

        public BusSystem BusSystem
        {
            get
            {
                return _busSystem;
            }
            set
            {
                SetProperty(ref _busSystem, value);
            }
        }

        [RelayCommand(CanExecute = nameof(CanOk))]
        private void Ok()
        {
            Close(true);
        }

        private bool CanOk()
        {
            bool result = _name.Trim().Length > 0;

            return result;
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}

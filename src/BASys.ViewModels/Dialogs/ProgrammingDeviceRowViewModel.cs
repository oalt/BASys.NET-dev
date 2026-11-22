using BASys.DataModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Row of the programming table: an EIB actuator or sensor of the project.
    /// </summary>
    public class ProgrammingDeviceRowViewModel : ObservableObject
    {
        private readonly Project _project;

        public ProgrammingDeviceRowViewModel(Project project, string deviceId)
        {
            _project = project;
            Id = deviceId;
        }

        public string Id { get; }

        public string Name
        {
            get
            {
                return _project.Installation.GetName(Id);
            }
        }

        public string Manufacturer
        {
            get
            {
                return _project.Installation.GetProperty(Id, "manufacturer");
            }
        }

        public string Product
        {
            get
            {
                return _project.Installation.GetProperty(Id, "device-name");
            }
        }

        public string PhysicalAddress
        {
            get
            {
                return _project.Installation.GetProperty(Id, "eib-physical-address");
            }
        }

        public string State
        {
            get
            {
                return _project.Installation.GetProperty(Id, "device-state");
            }
        }

        public string InstallationLocation
        {
            get
            {
                return _project.Architectural.GetInstallationLocationName(Id);
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        public void RefreshState()
        {
            OnPropertyChanged(nameof(State));
        }
    }
}

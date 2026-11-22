using BASys.DataModels;
using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for creating a new project.
    /// </summary>
    public partial class NewProjectDialogViewModel : DialogViewModelBase
    {
        public NewProjectDialogViewModel(ILocalizationService localizer)
        {
            Title = localizer["tit.newProject"];
        }

        private string _projectName = string.Empty;

        public string ProjectName
        {
            get
            {
                return _projectName;
            }
            set
            {
                if (SetProperty(ref _projectName, value))
                {
                    CreateCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private BusSystem _preferredBusSystem = BusSystem.Eib;

        public BusSystem PreferredBusSystem
        {
            get
            {
                return _preferredBusSystem;
            }
            set
            {
                SetProperty(ref _preferredBusSystem, value);
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void Create()
        {
            Close(true);
        }

        private bool CanCreate()
        {
            bool result = _projectName.Trim().Length > 0;

            return result;
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}

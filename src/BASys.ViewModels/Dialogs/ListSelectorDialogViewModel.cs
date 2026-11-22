using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for selecting one entry of a list (displayed text and value).
    /// </summary>
    public partial class ListSelectorDialogViewModel : DialogViewModelBase
    {
        public ListSelectorDialogViewModel(string title, IReadOnlyList<KeyValuePair<string, string>> items, ILocalizationService localizer)
        {
            Title = title;
            Items = items;
            ContinueText = localizer["blabel.continue"];
            CancelText = localizer["blabel.cancel"];
        }

        public IReadOnlyList<KeyValuePair<string, string>> Items { get; }

        public string ContinueText { get; }

        public string CancelText { get; }

        private string? _selectedValue;

        public string? SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                if (SetProperty(ref _selectedValue, value))
                {
                    ContinueCommand.NotifyCanExecuteChanged();
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanContinue))]
        private void Continue()
        {
            Close(true);
        }

        private bool CanContinue()
        {
            bool result = _selectedValue != null;

            return result;
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}

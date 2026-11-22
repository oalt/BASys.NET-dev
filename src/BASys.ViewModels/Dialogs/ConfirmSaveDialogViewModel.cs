using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Answer to the question whether changes should be saved.
    /// </summary>
    public enum SaveChoice
    {
        Save,
        Discard,
        Cancel
    }

    /// <summary>
    /// Dialog that asks whether unsaved changes should be saved (yes, no, cancel).
    /// </summary>
    public partial class ConfirmSaveDialogViewModel : DialogViewModelBase
    {
        public ConfirmSaveDialogViewModel(string title, string message, ILocalizationService localizer)
        {
            Title = title;
            Message = message;
            YesText = localizer["blabel.yes"];
            NoText = localizer["blabel.no"];
            CancelText = localizer["blabel.cancel"];
        }

        public string Message { get; }

        public string YesText { get; }

        public string NoText { get; }

        public string CancelText { get; }

        private SaveChoice _choice = SaveChoice.Cancel;

        public SaveChoice Choice
        {
            get
            {
                return _choice;
            }
        }

        [RelayCommand]
        private void Save()
        {
            _choice = SaveChoice.Save;
            Close(true);
        }

        [RelayCommand]
        private void Discard()
        {
            _choice = SaveChoice.Discard;
            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            _choice = SaveChoice.Cancel;
            Close(false);
        }
    }
}

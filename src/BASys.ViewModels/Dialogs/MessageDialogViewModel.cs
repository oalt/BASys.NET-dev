using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Simple message box (information or yes/no question).
    /// </summary>
    public partial class MessageDialogViewModel : DialogViewModelBase
    {
        public MessageDialogViewModel(string title, string message, bool isQuestion, ILocalizationService localizer)
        {
            Title = title;
            Message = message;
            IsQuestion = isQuestion;
            OkText = isQuestion ? localizer["blabel.yes"] : localizer["blabel.ok"];
            CancelText = localizer["blabel.no"];
        }

        public string Message { get; }

        public bool IsQuestion { get; }

        public string OkText { get; }

        public string CancelText { get; }

        [RelayCommand]
        private void Confirm()
        {
            Close(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            Close(false);
        }
    }
}

using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog that asks for a single text.
    /// </summary>
    public partial class TextPromptDialogViewModel : DialogViewModelBase
    {
        public TextPromptDialogViewModel(string title, string message, string defaultText, ILocalizationService localizer)
        {
            Title = title;
            Message = message;
            _text = defaultText;
            OkText = localizer["blabel.ok"];
            CancelText = localizer["blabel.cancel"];
        }

        public string Message { get; }

        public string OkText { get; }

        public string CancelText { get; }

        private string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                SetProperty(ref _text, value);
            }
        }

        [RelayCommand]
        private void Ok()
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

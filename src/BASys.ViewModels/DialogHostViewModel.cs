using BASys.Services;
using BASys.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels
{
    /// <summary>
    /// Keeps the stack of open dialogs. The UI displays all open dialogs, the topmost one in front.
    /// </summary>
    public class DialogHostViewModel : ObservableObject, IDialogService
    {
        private readonly ILocalizationService _localizer;

        private readonly List<DialogViewModelBase> _dialogStack = new List<DialogViewModelBase>();

        private readonly Dictionary<DialogViewModelBase, TaskCompletionSource<bool>> _completionSources = new Dictionary<DialogViewModelBase, TaskCompletionSource<bool>>();

        public DialogHostViewModel(ILocalizationService localizer)
        {
            _localizer = localizer;
        }

        private DialogViewModelBase? _currentDialog;

        /// <summary>
        /// The dialog that is currently displayed in front or null if there is none.
        /// </summary>
        public DialogViewModelBase? CurrentDialog
        {
            get
            {
                return _currentDialog;
            }
            private set
            {
                SetProperty(ref _currentDialog, value);
            }
        }

        /// <summary>
        /// All open dialogs, the bottom one first.
        /// </summary>
        public IReadOnlyList<DialogViewModelBase> OpenDialogs
        {
            get
            {
                IReadOnlyList<DialogViewModelBase> result = _dialogStack.ToList();

                return result;
            }
        }

        public Task<bool> ShowAsync(DialogViewModelBase dialog)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _completionSources[dialog] = completionSource;
            dialog.CloseRequested += OnDialogCloseRequested;

            _dialogStack.Add(dialog);
            CurrentDialog = dialog;
            OnPropertyChanged(nameof(OpenDialogs));

            Task<bool> result = completionSource.Task;

            return result;
        }

        private void OnDialogCloseRequested(object? sender, bool dialogResult)
        {
            DialogViewModelBase? dialog = sender as DialogViewModelBase;

            if (dialog != null)
            {
                dialog.CloseRequested -= OnDialogCloseRequested;
                _dialogStack.Remove(dialog);
                CurrentDialog = _dialogStack.Count > 0 ? _dialogStack[_dialogStack.Count - 1] : null;
                OnPropertyChanged(nameof(OpenDialogs));
                _completionSources[dialog].TrySetResult(dialogResult);
                _completionSources.Remove(dialog);
            }
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            MessageDialogViewModel dialog = new MessageDialogViewModel(title, message, false, _localizer);
            await ShowAsync(dialog);
        }

        public async Task<bool> ShowQuestionAsync(string title, string message)
        {
            MessageDialogViewModel dialog = new MessageDialogViewModel(title, message, true, _localizer);
            bool result = await ShowAsync(dialog);

            return result;
        }

        public async Task<SaveChoice> ShowConfirmSaveAsync(string title, string message)
        {
            ConfirmSaveDialogViewModel dialog = new ConfirmSaveDialogViewModel(title, message, _localizer);
            await ShowAsync(dialog);

            SaveChoice result = dialog.Choice;

            return result;
        }

        public async Task<string?> ShowPromptAsync(string title, string message, string defaultText)
        {
            TextPromptDialogViewModel dialog = new TextPromptDialogViewModel(title, message, defaultText, _localizer);
            string? result = await ShowAsync(dialog) ? dialog.Text : null;

            return result;
        }

        public async Task<string?> ShowSelectionAsync(string title, IReadOnlyList<KeyValuePair<string, string>> items)
        {
            ListSelectorDialogViewModel dialog = new ListSelectorDialogViewModel(title, items, _localizer);
            string? result = await ShowAsync(dialog) ? dialog.SelectedValue : null;

            return result;
        }
    }
}

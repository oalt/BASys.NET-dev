using BASys.ViewModels.Dialogs;

namespace BASys.ViewModels
{
    /// <summary>
    /// Shows modal dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a dialog. The result is true if the dialog was confirmed.
        /// </summary>
        Task<bool> ShowAsync(DialogViewModelBase dialog);

        Task ShowMessageAsync(string title, string message);

        Task<bool> ShowQuestionAsync(string title, string message);

        /// <summary>
        /// Asks whether changes should be saved (yes, no or cancel).
        /// </summary>
        Task<SaveChoice> ShowConfirmSaveAsync(string title, string message);

        /// <summary>
        /// Asks for a text. Returns null if the dialog was cancelled.
        /// </summary>
        Task<string?> ShowPromptAsync(string title, string message, string defaultText);

        /// <summary>
        /// Lets the user select one entry of a list of (displayed text, value) pairs. Returns the value or null if cancelled.
        /// </summary>
        Task<string?> ShowSelectionAsync(string title, IReadOnlyList<KeyValuePair<string, string>> items);
    }
}

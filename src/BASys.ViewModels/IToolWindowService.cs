using BASys.ViewModels.Dialogs;

namespace BASys.ViewModels
{
    /// <summary>
    /// Shows a dialog in its own, non-modal window (for example the bus monitor), so that it can be used
    /// together with the main window and other tool windows.
    /// </summary>
    public interface IToolWindowService
    {
        /// <summary>
        /// Opens a window for the dialog. The window is closed when the dialog closes itself,
        /// and the dialog is closed when the user closes the window.
        /// </summary>
        void Show(DialogViewModelBase dialog);
    }
}

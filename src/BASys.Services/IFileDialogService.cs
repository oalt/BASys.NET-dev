namespace BASys.Services
{
    /// <summary>
    /// Native file and folder selection dialogs (implemented by the host application).
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Lets the user select a folder. Returns null if the dialog was cancelled.
        /// </summary>
        string? PickFolder(string title, string initialDirectory);

        /// <summary>
        /// Lets the user select a file. The filter has the Windows form "Description|*.ext". Returns null if cancelled.
        /// </summary>
        string? PickFile(string title, string filter, string initialDirectory);
    }
}

using System.IO;
using BASys.Services;
using Microsoft.Win32;

namespace BASys.Apps.Wpf
{
    /// <summary>
    /// File and folder selection with the native Windows dialogs.
    /// </summary>
    public class WpfFileDialogService : IFileDialogService
    {
        public string? PickFolder(string title, string initialDirectory)
        {
            OpenFolderDialog dialog = new OpenFolderDialog { Title = title };

            if (Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            string? result = dialog.ShowDialog() == true ? dialog.FolderName : null;

            return result;
        }

        public string? PickFile(string title, string filter, string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog { Title = title, Filter = filter };

            if (Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            string? result = dialog.ShowDialog() == true ? dialog.FileName : null;

            return result;
        }
    }
}

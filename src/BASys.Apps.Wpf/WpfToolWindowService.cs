using BASys.ViewModels;
using BASys.ViewModels.Dialogs;

namespace BASys.Apps.Wpf
{
    /// <summary>
    /// Opens dialogs in their own WPF windows, each hosting its own Blazor web view.
    /// </summary>
    public class WpfToolWindowService : IToolWindowService
    {
        private readonly IServiceProvider _services;

        private readonly List<ToolWindow> _windows = new List<ToolWindow>();

        public WpfToolWindowService(IServiceProvider services)
        {
            _services = services;
        }

        public void Show(DialogViewModelBase dialog)
        {
            ToolWindow window = new ToolWindow(_services, dialog);
            window.Closed += OnWindowClosed;
            _windows.Add(window);
            window.Show();
        }

        /// <summary>
        /// Closes all tool windows (when the main window is closed).
        /// </summary>
        public void CloseAll()
        {
            foreach (ToolWindow currentWindow in _windows.ToList())
            {
                currentWindow.Close();
            }
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            ToolWindow? window = sender as ToolWindow;

            if (window != null)
            {
                _windows.Remove(window);
            }
        }
    }
}

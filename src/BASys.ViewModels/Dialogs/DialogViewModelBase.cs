using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Base class of all dialog view models.
    /// </summary>
    public abstract class DialogViewModelBase : ObservableObject
    {
        /// <summary>
        /// Raised when the dialog wants to be closed. The argument is the dialog result.
        /// </summary>
        public event EventHandler<bool>? CloseRequested;

        /// <summary>
        /// Preferred width of the window if the dialog is shown in its own window (0 = default).
        /// </summary>
        public virtual double PreferredWidth
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Preferred height of the window if the dialog is shown in its own window (0 = default).
        /// </summary>
        public virtual double PreferredHeight
        {
            get
            {
                return 0;
            }
        }

        private string _title = string.Empty;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                SetProperty(ref _title, value);
            }
        }

        private bool _isClosed;

        /// <summary>
        /// Closes the dialog. Further calls (e.g. from repeated clicks) are ignored.
        /// </summary>
        protected void Close(bool dialogResult)
        {
            if (!_isClosed)
            {
                _isClosed = true;
                OnClosing();
                CloseRequested?.Invoke(this, dialogResult);
            }
        }

        /// <summary>
        /// Closes the dialog on behalf of the view (for example when the user closes the dialog's own window).
        /// </summary>
        public void CloseFromView()
        {
            Close(false);
        }

        /// <summary>
        /// Called once when the dialog is closed, before <see cref="CloseRequested"/> is raised.
        /// </summary>
        protected virtual void OnClosing()
        {
        }
    }
}

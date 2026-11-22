using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// One of the seven data byte fields of the telegram dialog.
    /// </summary>
    public class TelegramDataFieldViewModel : ObservableObject
    {
        private string _text = string.Empty;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (SetProperty(ref _text, value))
                {
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool _isEditable;

        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                SetProperty(ref _isEditable, value);
            }
        }

        /// <summary>
        /// Raised when the text was changed (by the user or by the dialog).
        /// </summary>
        public event EventHandler? TextChanged;
    }
}

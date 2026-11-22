using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.Views.Menu
{
    /// <summary>
    /// Shared state of the menu bar: which drop down menu is currently open.
    /// </summary>
    public class MenuBarState : ObservableObject
    {
        private string? _openMenuId;

        public string? OpenMenuId
        {
            get
            {
                return _openMenuId;
            }
            set
            {
                SetProperty(ref _openMenuId, value);
            }
        }
    }
}

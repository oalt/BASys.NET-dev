using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Menu
{
    public partial class MenuBar
    {
        private readonly MenuBarState _state = new MenuBarState();

        [Parameter, EditorRequired]
        public MainViewModel ViewModel { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(_state);
        }

        private void CloseMenus()
        {
            _state.OpenMenuId = null;
        }
    }
}

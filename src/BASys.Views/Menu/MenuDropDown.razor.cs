using Microsoft.AspNetCore.Components;

namespace BASys.Views.Menu
{
    public partial class MenuDropDown
    {
        [CascadingParameter]
        public MenuBarState State { get; set; } = default!;

        [Parameter, EditorRequired]
        public string Id { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private bool IsOpen
        {
            get
            {
                bool result = State.OpenMenuId == Id;

                return result;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(State);
        }

        private void OnTitleClick()
        {
            State.OpenMenuId = IsOpen ? null : Id;
        }

        private void OnTitleHover()
        {
            if (State.OpenMenuId != null)
            {
                State.OpenMenuId = Id;
            }
        }
    }
}

using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class DialogFrame
    {
        [Parameter]
        public string Title { get; set; } = string.Empty;

        [Parameter]
        public string Width { get; set; } = "420px";

        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}

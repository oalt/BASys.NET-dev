using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class Icon
    {
        [Parameter, EditorRequired]
        public string Name { get; set; } = string.Empty;

        [Parameter]
        public int Size { get; set; } = 16;

        [Parameter]
        public string CssClass { get; set; } = string.Empty;
    }
}

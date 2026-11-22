using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class ContextMenuEntry
    {
        [Parameter]
        public string Text { get; set; } = string.Empty;

        [Parameter, EditorRequired]
        public PaletteItemType Item { get; set; }

        [Parameter]
        public ArchitecturalNodeViewModel? Target { get; set; }

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        [Parameter]
        public EventCallback OnExecuted { get; set; }

        private async Task OnClick()
        {
            await OnExecuted.InvokeAsync();
            await Main.AddElementToAsync(Item, Target);
        }
    }
}

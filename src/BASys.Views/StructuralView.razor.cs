using BASys.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views
{
    public partial class StructuralView
    {
        [Parameter, EditorRequired]
        public ProjectViewModel Project { get; set; } = default!;

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        [Parameter]
        public EventCallback<ContextMenuRequest> OnContextMenuRequested { get; set; }

        protected override void OnParametersSet()
        {
            Observe(Project);
        }

        private async Task OnContextMenu(MouseEventArgs e)
        {
            await OnContextMenuRequested.InvokeAsync(new ContextMenuRequest(e.ClientX, e.ClientY, Project.SelectedNode));
        }
    }
}

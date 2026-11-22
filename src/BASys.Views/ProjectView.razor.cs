using BASys.DataModels;
using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class ProjectView
    {
        private ContextMenuRequest? _contextMenuRequest;

        [Parameter, EditorRequired]
        public ProjectViewModel Project { get; set; } = default!;

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        protected override void OnParametersSet()
        {
            Observe(Project);
        }

        private void OnContextMenuRequested(ContextMenuRequest request)
        {
            _contextMenuRequest = request;
        }

        private void OnContextMenuClosed()
        {
            _contextMenuRequest = null;
        }
    }
}

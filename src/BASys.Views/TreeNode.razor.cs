using BASys.DataModels;
using BASys.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views
{
    public partial class TreeNode
    {
        [Inject]
        private DragDropState DragState { get; set; } = default!;

        private readonly string _targetId = "tree-" + Guid.NewGuid().ToString("N");

        private bool _isEditing;

        [Parameter, EditorRequired]
        public ArchitecturalNodeViewModel Node { get; set; } = default!;

        [Parameter, EditorRequired]
        public ProjectViewModel Project { get; set; } = default!;

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        [Parameter]
        public EventCallback<ContextMenuRequest> OnContextMenuRequested { get; set; }

        private string EditName { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DragState.RegisterTarget(_targetId, OnItemDropped);
            Observe(DragState);
        }

        protected override void OnParametersSet()
        {
            Observe(Node);
            Observe(Node.Children);
        }

        public override void Dispose()
        {
            DragState.UnregisterTarget(_targetId);
            base.Dispose();
        }

        private string GetToggleSymbol()
        {
            string result = string.Empty;

            if (Node.Children.Count > 0)
            {
                result = Node.IsExpanded ? "▾" : "▸";
            }

            return result;
        }

        private void OnSelect()
        {
            Project.Select(Node);
            Main.NotifySelectionChanged();
        }

        private void OnToggle()
        {
            Node.IsExpanded = !Node.IsExpanded;
        }

        private void OnStartEdit()
        {
            EditName = Node.Name;
            _isEditing = true;
        }

        private void OnEditKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                OnEndEdit();
            }
            else if (e.Key == "Escape")
            {
                _isEditing = false;
            }
        }

        private void OnEndEdit()
        {
            if (_isEditing && EditName.Trim().Length > 0)
            {
                Node.Name = EditName.Trim();
            }

            _isEditing = false;
        }

        private async Task OnContextMenu(MouseEventArgs e)
        {
            OnSelect();
            await OnContextMenuRequested.InvokeAsync(new ContextMenuRequest(e.ClientX, e.ClientY, Node));
        }

        private async Task OnItemDropped(PaletteItemType itemType)
        {
            await Main.AddElementToAsync(itemType, Node);
        }
    }
}

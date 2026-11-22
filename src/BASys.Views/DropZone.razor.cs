using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    /// <summary>
    /// A drop target for palette elements (the tables of the structural view). It marks itself while an
    /// element it accepts is dragged and reports the dropped element.
    /// </summary>
    public partial class DropZone
    {
        private readonly string _targetId = "zone-" + Guid.NewGuid().ToString("N");

        [Inject]
        private DragDropState DragState { get; set; } = default!;

        [Parameter]
        public Func<PaletteItemType, bool> Accepts { get; set; } = AcceptNothing;

        [Parameter]
        public EventCallback<PaletteItemType> OnItemDropped { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        private bool CanDrop
        {
            get
            {
                PaletteItemType? draggedItem = DragState.DraggedItem;
                bool result = draggedItem.HasValue && Accepts(draggedItem.Value);

                return result;
            }
        }

        private string CssClass
        {
            get
            {
                string result = CanDrop ? "drop-zone drop-active" : "drop-zone";

                return result;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DragState.RegisterTarget(_targetId, OnDroppedAsync);
            Observe(DragState);
        }

        public override void Dispose()
        {
            DragState.UnregisterTarget(_targetId);
            base.Dispose();
        }

        private static bool AcceptNothing(PaletteItemType itemType)
        {
            return false;
        }

        private async Task OnDroppedAsync(PaletteItemType itemType)
        {
            if (Accepts(itemType))
            {
                await OnItemDropped.InvokeAsync(itemType);
            }
        }
    }
}

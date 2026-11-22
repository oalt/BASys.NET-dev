using BASys.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.JSInterop;

namespace BASys.Views
{
    /// <summary>
    /// Keeps the palette element that is currently dragged and the registered drop targets.
    /// The dragging itself is done by <c>js/dragdrop.js</c> with pointer events (the HTML5 drag and drop
    /// of WebView2 does not deliver drag events to the page when it is hosted in WPF); the script reports
    /// start, drop and end of a drag through the JSInvokable methods.
    /// </summary>
    public class DragDropState : ObservableObject
    {
        private readonly Dictionary<string, Func<PaletteItemType, Task>> _targets = new Dictionary<string, Func<PaletteItemType, Task>>();

        private PaletteItemType? _draggedItem;

        /// <summary>
        /// The palette element that is currently dragged or null.
        /// </summary>
        public PaletteItemType? DraggedItem
        {
            get
            {
                return _draggedItem;
            }
            private set
            {
                SetProperty(ref _draggedItem, value);
            }
        }

        public void RegisterTarget(string targetId, Func<PaletteItemType, Task> dropHandler)
        {
            _targets[targetId] = dropHandler;
        }

        public void UnregisterTarget(string targetId)
        {
            _targets.Remove(targetId);
        }

        [JSInvokable]
        public void BeginDrag(string itemName)
        {
            PaletteItemType itemType;

            if (Enum.TryParse(itemName, out itemType))
            {
                DraggedItem = itemType;
            }
        }

        [JSInvokable]
        public async Task Drop(string targetId)
        {
            PaletteItemType? draggedItem = DraggedItem;
            DraggedItem = null;

            if (draggedItem.HasValue && _targets.TryGetValue(targetId, out Func<PaletteItemType, Task>? dropHandler))
            {
                await dropHandler(draggedItem.Value);
            }
        }

        [JSInvokable]
        public void EndDrag()
        {
            DraggedItem = null;
        }
    }
}

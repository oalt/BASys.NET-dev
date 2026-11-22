using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    /// <summary>
    /// A draggable object of the palette. It is dragged (see <c>js/dragdrop.js</c>) onto a table of the
    /// structural view or a node of the tree to create the element.
    /// </summary>
    public partial class PaletteItem
    {
        [Parameter, EditorRequired]
        public PaletteItemType Item { get; set; }

        [Parameter]
        public string Icon { get; set; } = string.Empty;

        [Parameter]
        public string Text { get; set; } = string.Empty;

        private string IconClass
        {
            get
            {
                string result = "icon-" + Item.ToString().ToLowerInvariant();

                return result;
            }
        }
    }
}

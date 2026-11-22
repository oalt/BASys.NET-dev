using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class ListBox<TItem> where TItem : class
    {
        [Parameter, EditorRequired]
        public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

        [Parameter]
        public TItem? SelectedItem { get; set; }

        [Parameter]
        public EventCallback<TItem?> SelectedItemChanged { get; set; }

        [Parameter, EditorRequired]
        public Func<TItem, string> TextProvider { get; set; } = default!;

        [Parameter]
        public int Size { get; set; } = 6;

        private async Task OnChanged(ChangeEventArgs e)
        {
            TItem? newItem = null;
            int index;

            if (int.TryParse(e.Value?.ToString(), out index) && index >= 0 && index < Items.Count)
            {
                newItem = Items[index];
            }

            await SelectedItemChanged.InvokeAsync(newItem);
        }
    }
}

using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class ContextMenu
    {
        private readonly PaletteItemType[] _items =
        {
            PaletteItemType.Building, PaletteItemType.Floor, PaletteItemType.Room, PaletteItemType.JunctionBox,
            PaletteItemType.Lamp, PaletteItemType.DimmableLamp, PaletteItemType.Valve, PaletteItemType.Jalousie, PaletteItemType.Sensor
        };

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        [Parameter]
        public ContextMenuRequest? Request { get; set; }

        [Parameter]
        public EventCallback OnClosed { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(Main.DeleteCommand);
        }

        private string GetText(PaletteItemType item)
        {
            string result = T("km.newsensor");

            switch (item)
            {
                case PaletteItemType.Building:
                    result = T("km.newbuilding");
                    break;
                case PaletteItemType.Floor:
                    result = T("km.newfloor");
                    break;
                case PaletteItemType.Room:
                    result = T("km.newroom");
                    break;
                case PaletteItemType.JunctionBox:
                    result = T("km.newjunctionbox");
                    break;
                case PaletteItemType.Lamp:
                    result = T("km.newlamp");
                    break;
                case PaletteItemType.DimmableLamp:
                    result = T("km.newdimlamp");
                    break;
                case PaletteItemType.Valve:
                    result = T("km.newvalve");
                    break;
                case PaletteItemType.Jalousie:
                    result = T("km.newjal");
                    break;
                default:
                    break;
            }

            return result;
        }

        private async Task OnClose()
        {
            await OnClosed.InvokeAsync();
        }

        private async Task OnDelete()
        {
            if (Main.DeleteCommand.CanExecute(null))
            {
                Main.DeleteCommand.Execute(null);
            }

            await OnClose();
        }
    }
}

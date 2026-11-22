using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class RoomView
    {
        [Parameter, EditorRequired]
        public ArchitecturalNodeViewModel Room { get; set; } = default!;

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        protected override void OnParametersSet()
        {
            Observe(Room);
            Observe(Room.InstallationDevices);
            Observe(Room.JunctionBoxes);
            Observe(Room.BusDevices);
        }

        private bool AcceptsDevice(PaletteItemType itemType)
        {
            bool result = itemType.IsEndDevice();

            return result;
        }

        private bool AcceptsJunctionBox(PaletteItemType itemType)
        {
            bool result = itemType == PaletteItemType.JunctionBox;

            return result;
        }

        private async Task OnItemDropped(PaletteItemType itemType)
        {
            await Main.AddElementToAsync(itemType, Room);
        }
    }
}

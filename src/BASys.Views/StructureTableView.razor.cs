using BASys.DataModels;
using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    /// <summary>
    /// The table of the children of a project, building or floor (buildings, floors, rooms). New elements are
    /// created by dropping them from the palette onto the table.
    /// </summary>
    public partial class StructureTableView
    {
        [Parameter, EditorRequired]
        public ArchitecturalNodeViewModel Node { get; set; } = default!;

        [Parameter, EditorRequired]
        public ProjectViewModel Project { get; set; } = default!;

        [Parameter, EditorRequired]
        public MainViewModel Main { get; set; } = default!;

        private string TitleKey
        {
            get
            {
                string result = "l.rooms";

                if (Node.NodeType == ArchitecturalNodeType.Project)
                {
                    result = "l.buildings";
                }
                else if (Node.NodeType == ArchitecturalNodeType.Building)
                {
                    result = "l.floors";
                }

                return result;
            }
        }

        protected override void OnParametersSet()
        {
            Observe(Node);
            Observe(Node.Children);

            foreach (ArchitecturalNodeViewModel currentChild in Node.Children)
            {
                Observe(currentChild);
            }
        }

        private bool Accepts(PaletteItemType itemType)
        {
            bool result = !itemType.IsEndDevice() && itemType.GetRequiredParentType() == Node.NodeType;

            return result;
        }

        private async Task OnItemDropped(PaletteItemType itemType)
        {
            await Main.AddElementToAsync(itemType, Node);
        }

        private void OnRowClick(ArchitecturalNodeViewModel child)
        {
            Project.Select(child);
        }
    }
}

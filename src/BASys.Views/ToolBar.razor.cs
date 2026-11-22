using BASys.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class ToolBar
    {
        [Parameter, EditorRequired]
        public MainViewModel ViewModel { get; set; } = default!;
    }
}

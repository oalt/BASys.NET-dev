using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    /// <summary>
    /// Root component of a tool window: shows one dialog (bus monitor, telegram dialog) filling the whole window.
    /// </summary>
    public partial class ToolWindowRoot
    {
        [Parameter, EditorRequired]
        public DialogViewModelBase Dialog { get; set; } = default!;
    }
}

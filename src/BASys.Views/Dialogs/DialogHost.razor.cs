using BASys.ViewModels;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class DialogHost
    {
        [Parameter, EditorRequired]
        public DialogHostViewModel Host { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(Host);
        }
    }
}

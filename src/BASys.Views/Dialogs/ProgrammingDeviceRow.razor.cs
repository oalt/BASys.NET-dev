using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class ProgrammingDeviceRow
    {
        [Parameter, EditorRequired]
        public ProgrammingDeviceRowViewModel Row { get; set; } = default!;

        [Parameter, EditorRequired]
        public EibProgrammingDialogViewModel Dialog { get; set; } = default!;

        protected override void OnParametersSet()
        {
            Observe(Row);
        }

        private void OnClick()
        {
            Dialog.ToggleSelection(Row);
        }
    }
}

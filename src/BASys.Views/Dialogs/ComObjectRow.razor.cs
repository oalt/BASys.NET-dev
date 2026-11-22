using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class ComObjectRow
    {
        [Parameter, EditorRequired]
        public ComObjectRowViewModel Row { get; set; } = default!;

        [Parameter, EditorRequired]
        public EibDataDialogViewModel Dialog { get; set; } = default!;

        protected override void OnParametersSet()
        {
            Observe(Row);
        }

        private void OnClick()
        {
            Dialog.ToggleSelection(Row);
        }

        private void OnFunctionTypeChanged(ChangeEventArgs e)
        {
            Row.FunctionType = e.Value?.ToString() ?? "unused";
        }
    }
}

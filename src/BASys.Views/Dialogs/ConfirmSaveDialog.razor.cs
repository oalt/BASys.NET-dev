using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class ConfirmSaveDialog
    {
        [Parameter, EditorRequired]
        public ConfirmSaveDialogViewModel ViewModel { get; set; } = default!;

        private void OnSave()
        {
            ViewModel.SaveCommand.Execute(null);
        }

        private void OnDiscard()
        {
            ViewModel.DiscardCommand.Execute(null);
        }

        private void OnCancel()
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}

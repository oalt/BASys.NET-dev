using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class MessageDialog
    {
        [Parameter, EditorRequired]
        public MessageDialogViewModel ViewModel { get; set; } = default!;

        private void OnConfirm()
        {
            ViewModel.ConfirmCommand.Execute(null);
        }

        private void OnCancel()
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}

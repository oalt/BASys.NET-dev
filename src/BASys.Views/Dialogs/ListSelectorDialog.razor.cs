using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class ListSelectorDialog
    {
        [Parameter, EditorRequired]
        public ListSelectorDialogViewModel ViewModel { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(ViewModel.ContinueCommand);
        }

        private void OnSelectionChanged(ChangeEventArgs e)
        {
            ViewModel.SelectedValue = e.Value?.ToString();
        }

        private void OnContinue()
        {
            ViewModel.ContinueCommand.Execute(null);
        }

        private void OnCancel()
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}

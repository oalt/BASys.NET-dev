using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class BusMonitorDialog
    {
        [Parameter, EditorRequired]
        public BusMonitorDialogViewModel ViewModel { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(ViewModel.Rows);
        }

        private bool IsTwoLevelGroup
        {
            get { return ViewModel.IsTwoLevelGroup; }
            set { ViewModel.IsTwoLevelGroup = value; }
        }

        private void OnClear()
        {
            ViewModel.ClearDataCommand.Execute(null);
        }

        private void OnClose()
        {
            ViewModel.CloseDialogCommand.Execute(null);
        }
    }
}

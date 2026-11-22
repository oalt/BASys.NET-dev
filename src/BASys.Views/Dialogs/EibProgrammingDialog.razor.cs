using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class EibProgrammingDialog
    {
        [Parameter, EditorRequired]
        public EibProgrammingDialogViewModel ViewModel { get; set; } = default!;

        private bool AddressOnly
        {
            get
            {
                return ViewModel.AddressOnly;
            }
            set
            {
                ViewModel.AddressOnly = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(ViewModel.Devices);
            Observe(ViewModel.ProgramAllCommand);
            Observe(ViewModel.ProgramSelectedCommand);
            Observe(ViewModel.CancelProgrammingCommand);
            Observe(ViewModel.CloseDialogCommand);
        }

        private void OnProgramAll()
        {
            ViewModel.ProgramAllCommand.Execute(null);
        }

        private void OnProgramSelected()
        {
            ViewModel.ProgramSelectedCommand.Execute(null);
        }

        private void OnCancelProgramming()
        {
            ViewModel.CancelProgrammingCommand.Execute(null);
        }

        private void OnClose()
        {
            ViewModel.CloseDialogCommand.Execute(null);
        }
    }
}

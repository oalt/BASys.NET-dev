using BASys.Services;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class AboutDialog
    {
        [Inject]
        private ILocalizationService Localizer { get; set; } = default!;

        [Parameter, EditorRequired]
        public AboutDialogViewModel ViewModel { get; set; } = default!;

        private void OnOk()
        {
            ViewModel.OkCommand.Execute(null);
        }
    }
}

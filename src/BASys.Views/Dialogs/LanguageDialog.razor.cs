using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BASys.Views.Dialogs
{
    public partial class LanguageDialog
    {
        private ElementReference _focusElement;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        [Parameter, EditorRequired]
        public LanguageDialogViewModel ViewModel { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JsRuntime.InvokeVoidAsync("basysFocus.focusAndSelect", _focusElement);
            }
        }

        private string Language
        {
            get
            {
                return ViewModel.SelectedLanguage;
            }
            set
            {
                ViewModel.SelectedLanguage = value;
            }
        }

        private void OnOk()
        {
            ViewModel.OkCommand.Execute(null);
        }
    }
}

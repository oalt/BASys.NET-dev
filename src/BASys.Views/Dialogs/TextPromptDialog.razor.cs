using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class TextPromptDialog
    {
        private ElementReference _focusElement;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        [Parameter, EditorRequired]
        public TextPromptDialogViewModel ViewModel { get; set; } = default!;

        private string Text
        {
            get
            {
                return ViewModel.Text;
            }
            set
            {
                ViewModel.Text = value;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JsRuntime.InvokeVoidAsync("basysFocus.focusAndSelect", _focusElement);
            }
        }

        private void OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                OnOk();
            }
        }

        private void OnOk()
        {
            ViewModel.OkCommand.Execute(null);
        }

        private void OnCancel()
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}

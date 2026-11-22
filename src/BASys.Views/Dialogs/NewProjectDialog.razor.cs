using BASys.DataModels;
using BASys.Services;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BASys.Views.Dialogs
{
    public partial class NewProjectDialog
    {
        private ElementReference _focusElement;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        private readonly BusSystem[] _busSystems = { BusSystem.Eib, BusSystem.Lon, BusSystem.Lcn, BusSystem.None };

        [Parameter, EditorRequired]
        public NewProjectDialogViewModel ViewModel { get; set; } = default!;

        private string ProjectName
        {
            get
            {
                return ViewModel.ProjectName;
            }
            set
            {
                ViewModel.ProjectName = value;
            }
        }

        private BusSystem BusSystem
        {
            get
            {
                return ViewModel.PreferredBusSystem;
            }
            set
            {
                ViewModel.PreferredBusSystem = value;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JsRuntime.InvokeVoidAsync("basysFocus.focusAndSelect", _focusElement);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(ViewModel.CreateCommand);
        }

        private string GetBusSystemText(BusSystem busSystem)
        {
            string result = Localizer.GetName(busSystem);

            return result;
        }

        private void OnCreate()
        {
            ViewModel.CreateCommand.Execute(null);
        }

        private void OnCancel()
        {
            ViewModel.CancelCommand.Execute(null);
        }
    }
}

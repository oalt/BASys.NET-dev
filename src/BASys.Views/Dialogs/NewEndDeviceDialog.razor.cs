using BASys.DataModels;
using BASys.Services;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BASys.Views.Dialogs
{
    public partial class NewEndDeviceDialog
    {
        private ElementReference _focusElement;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        private readonly InstallationLocation[] _locations =
        {
            InstallationLocation.DinRail, InstallationLocation.Flush, InstallationLocation.ChannelBuiltIn, InstallationLocation.CouplerIntegrated
        };

        private readonly BusSystem[] _busSystems = { BusSystem.Eib, BusSystem.Lon, BusSystem.Lcn };

        [Parameter, EditorRequired]
        public NewEndDeviceDialogViewModel ViewModel { get; set; } = default!;

        private string DeviceName
        {
            get
            {
                return ViewModel.Name;
            }
            set
            {
                ViewModel.Name = value;
            }
        }

        private InstallationLocation Location
        {
            get
            {
                return ViewModel.Location;
            }
            set
            {
                ViewModel.Location = value;
            }
        }

        private BusSystem BusSystem
        {
            get
            {
                return ViewModel.BusSystem;
            }
            set
            {
                ViewModel.BusSystem = value;
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
            Observe(ViewModel.OkCommand);
        }

        private string GetLocationText(InstallationLocation location)
        {
            string result = Localizer.GetName(location);

            return result;
        }

        private string GetBusSystemText(BusSystem busSystem)
        {
            string result = Localizer.GetName(busSystem);

            return result;
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

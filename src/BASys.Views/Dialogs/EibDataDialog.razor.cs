using BASys.DataModels;
using BASys.Services;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class EibDataDialog
    {
        private readonly EibDeviceKind[] _deviceKinds = { EibDeviceKind.Actuator, EibDeviceKind.Sensor };

        private readonly InstallationLocation[] _locations =
        {
            InstallationLocation.DinRail, InstallationLocation.Flush, InstallationLocation.ChannelBuiltIn, InstallationLocation.CouplerIntegrated
        };

        [Parameter, EditorRequired]
        public EibDataDialogViewModel ViewModel { get; set; } = default!;

        private EibDeviceKind DeviceKind
        {
            get
            {
                return ViewModel.DeviceKind;
            }
            set
            {
                ViewModel.DeviceKind = value;
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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Observe(ViewModel.ComObjects);
            Observe(ViewModel.DatabaseDevices);
            Observe(ViewModel.SetFunctionGroupCommand);
            Observe(ViewModel.AddDeviceCommand);
            Observe(ViewModel.DeleteCommand);
        }

        private string GetFileText(string fileName)
        {
            return fileName;
        }

        private string GetProductText(EibProduct product)
        {
            string result = product.Name;

            return result;
        }

        private string GetApplicationText(EibApplication application)
        {
            string result = application.Name;

            return result;
        }

        private string GetDeviceKindText(EibDeviceKind deviceKind)
        {
            string result = Localizer.GetName(deviceKind);

            return result;
        }

        private string GetLocationText(InstallationLocation location)
        {
            string result = Localizer.GetName(location);

            return result;
        }

        private void OnSetFunctionGroup()
        {
            ViewModel.SetFunctionGroupCommand.Execute(null);
        }

        private void OnAddDevice()
        {
            ViewModel.AddDeviceCommand.Execute(null);
        }

        private void OnDelete()
        {
            ViewModel.DeleteCommand.Execute(null);
        }

        private void OnClose()
        {
            ViewModel.CloseDialogCommand.Execute(null);
        }
    }
}

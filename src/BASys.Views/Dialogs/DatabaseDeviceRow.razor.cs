using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Dialogs
{
    public partial class DatabaseDeviceRow
    {
        [Parameter, EditorRequired]
        public DatabaseDeviceRowViewModel Device { get; set; } = default!;

        [Parameter, EditorRequired]
        public EibDataDialogViewModel Dialog { get; set; } = default!;

        private bool IsSelected
        {
            get
            {
                bool result = ReferenceEquals(Dialog.SelectedDatabaseDevice, Device);

                return result;
            }
        }

        protected override void OnParametersSet()
        {
            Observe(Dialog);
        }

        private void OnClick()
        {
            Dialog.SelectedDatabaseDevice = Device;
        }
    }
}

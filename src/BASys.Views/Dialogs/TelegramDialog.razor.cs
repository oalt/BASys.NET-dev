using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views.Dialogs
{
    public partial class TelegramDialog
    {
        [Parameter, EditorRequired]
        public TelegramDialogViewModel ViewModel { get; set; } = default!;

        private int PriorityIndex
        {
            get { return ViewModel.PriorityIndex; }
            set { ViewModel.PriorityIndex = value; }
        }

        private bool Repeat
        {
            get { return ViewModel.Repeat; }
            set { ViewModel.Repeat = value; }
        }

        private string SourceZone
        {
            get { return ViewModel.SourceZone; }
            set { ViewModel.SourceZone = value; }
        }

        private string SourceLine
        {
            get { return ViewModel.SourceLine; }
            set { ViewModel.SourceLine = value; }
        }

        private string SourceNode
        {
            get { return ViewModel.SourceNode; }
            set { ViewModel.SourceNode = value; }
        }

        private bool IsTwoLevelGroup
        {
            get { return ViewModel.IsTwoLevelGroup; }
            set { ViewModel.IsTwoLevelGroup = value; }
        }

        private bool IsPhysicalDestination
        {
            get { return ViewModel.IsPhysicalDestination; }
            set { ViewModel.IsPhysicalDestination = value; }
        }

        private string DestinationPart1
        {
            get { return ViewModel.DestinationPart1; }
            set { ViewModel.DestinationPart1 = value; }
        }

        private string DestinationPart2
        {
            get { return ViewModel.DestinationPart2; }
            set { ViewModel.DestinationPart2 = value; }
        }

        private string DestinationPart3
        {
            get { return ViewModel.DestinationPart3; }
            set { ViewModel.DestinationPart3 = value; }
        }

        private int RoutingCounter
        {
            get { return ViewModel.RoutingCounter; }
            set { ViewModel.RoutingCounter = value; }
        }

        private int TpciIndex
        {
            get { return ViewModel.TpciIndex; }
            set { ViewModel.TpciIndex = value; }
        }

        private int ApciIndex
        {
            get { return ViewModel.ApciIndex; }
            set { ViewModel.ApciIndex = value; }
        }

        private int DataLengthIndex
        {
            get { return ViewModel.DataLengthIndex; }
            set { ViewModel.DataLengthIndex = value; }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);

            foreach (TelegramDataFieldViewModel currentField in ViewModel.DataFields)
            {
                Observe(currentField);
            }
        }

        private void OnDataInput(TelegramDataFieldViewModel field, ChangeEventArgs args)
        {
            field.Text = args.Value?.ToString() ?? string.Empty;
        }

        private void OnSend()
        {
            ViewModel.SendCommand.Execute(null);
        }

        private void OnClose()
        {
            ViewModel.CloseDialogCommand.Execute(null);
        }
    }
}

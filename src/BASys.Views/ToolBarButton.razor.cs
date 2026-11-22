using System.Windows.Input;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    public partial class ToolBarButton
    {
        [Parameter, EditorRequired]
        public string Icon { get; set; } = string.Empty;

        [Parameter]
        public string ColorClass { get; set; } = string.Empty;

        [Parameter]
        public string ToolTip { get; set; } = string.Empty;

        [Parameter]
        public ICommand? Command { get; set; }

        private bool IsEnabled
        {
            get
            {
                bool result = Command != null && Command.CanExecute(null);

                return result;
            }
        }

        protected override void OnParametersSet()
        {
            Observe(Command);
        }

        private void OnClick()
        {
            if (IsEnabled && Command != null)
            {
                Command.Execute(null);
            }
        }
    }
}

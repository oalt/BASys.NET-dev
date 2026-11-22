using System.Windows.Input;
using BASys.DataModels;
using BASys.ViewModels.Dialogs;
using Microsoft.AspNetCore.Components;

namespace BASys.Views.Menu
{
    public partial class MenuItemView
    {
        [CascadingParameter]
        public MenuBarState State { get; set; } = default!;

        [Parameter]
        public string Text { get; set; } = string.Empty;

        [Parameter]
        public string Shortcut { get; set; } = string.Empty;

        [Parameter]
        public ICommand? Command { get; set; }

        [Parameter]
        public object? CommandParameter { get; set; }

        [Parameter]
        public bool IsChecked { get; set; }

        [Parameter]
        public bool IsSeparator { get; set; }

        [Parameter]
        public bool IsHeader { get; set; }

        /// <summary>
        /// Allows forcing the item to be disabled independently of the command.
        /// </summary>
        [Parameter]
        public bool ForceDisabled { get; set; }

        private bool IsEnabled
        {
            get
            {
                bool result = !ForceDisabled && Command != null && Command.CanExecute(CommandParameter);

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
                Command.Execute(CommandParameter);
            }

            State.OpenMenuId = null;
        }
    }
}

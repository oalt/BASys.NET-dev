using BASys.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views
{
    public partial class MainView
    {
        [Inject]
        private MainViewModel ViewModel { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ViewModel.InitializeAsync();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Observe(ViewModel);
            Localizer.CultureChanged += OnCultureChanged;
        }

        private void OnCultureChanged(object? sender, EventArgs e)
        {
            ViewModel.RefreshLanguage();
        }

        private void OnKeyDown(KeyboardEventArgs e)
        {
            if (e.CtrlKey && e.Key.Equals("s", StringComparison.OrdinalIgnoreCase) && ViewModel.SaveCommand.CanExecute(null))
            {
                ViewModel.SaveCommand.Execute(null);
            }
            else if (e.Key == "Delete" && ViewModel.DeleteCommand.CanExecute(null))
            {
                ViewModel.DeleteCommand.Execute(null);
            }
        }

        public override void Dispose()
        {
            Localizer.CultureChanged -= OnCultureChanged;
            base.Dispose();
        }
    }
}

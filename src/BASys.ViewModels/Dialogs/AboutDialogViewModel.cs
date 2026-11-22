using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// "About" dialog.
    /// </summary>
    public partial class AboutDialogViewModel : DialogViewModelBase
    {
        public AboutDialogViewModel(ILocalizationService localizer)
        {
            Title = localizer["tit.about"];
            Description = localizer["l.about.description"];
            Copyright = localizer["l.about.copyright"];
            Version = localizer["l.about.version"] + " 2.0 (.NET Mockup)";
        }

        public string Description { get; }

        public string Copyright { get; }

        public string Version { get; }

        [RelayCommand]
        private void Ok()
        {
            Close(true);
        }
    }
}

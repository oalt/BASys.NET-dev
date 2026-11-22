using BASys.Services;
using CommunityToolkit.Mvvm.Input;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Dialog for selecting the user interface language. The selection is stored in the local configuration.
    /// </summary>
    public partial class LanguageDialogViewModel : DialogViewModelBase
    {
        private readonly ILocalizationService _localizer;

        private readonly IAppSettingsService _settings;

        public LanguageDialogViewModel(ILocalizationService localizer, IAppSettingsService settings)
        {
            _localizer = localizer;
            _settings = settings;
            Title = localizer["tit.language"];
            _selectedLanguage = localizer.CurrentCulture.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// Available languages as pairs of culture name and display name.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> Languages { get; } = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("de", "Deutsch"),
            new KeyValuePair<string, string>("en", "English")
        };

        private string _selectedLanguage;

        public string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                SetProperty(ref _selectedLanguage, value);
            }
        }

        [RelayCommand]
        private void Ok()
        {
            _settings.SetLanguage(_selectedLanguage == "en" ? "en_EN" : "de_DE");
            _localizer.SetCulture(_selectedLanguage);
            Close(true);
        }
    }
}

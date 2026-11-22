using System.Globalization;
using BASys.Resources;

namespace BASys.Services
{
    /// <summary>
    /// Localization service based on the resource files of BASys.Resources.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private CultureInfo _currentCulture = new CultureInfo("de");

        public CultureInfo CurrentCulture
        {
            get
            {
                return _currentCulture;
            }
        }

        public event EventHandler? CultureChanged;

        public string this[string key]
        {
            get
            {
                string result = ResourceProvider.GetString(key, _currentCulture);

                return result;
            }
        }

        public void SetCulture(string cultureName)
        {
            _currentCulture = new CultureInfo(cultureName);
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

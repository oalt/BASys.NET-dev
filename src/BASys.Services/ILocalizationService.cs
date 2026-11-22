using System.Globalization;

namespace BASys.Services
{
    /// <summary>
    /// Provides localized texts and allows switching the UI language.
    /// </summary>
    public interface ILocalizationService
    {
        CultureInfo CurrentCulture { get; }

        event EventHandler? CultureChanged;

        string this[string key] { get; }

        void SetCulture(string cultureName);
    }
}

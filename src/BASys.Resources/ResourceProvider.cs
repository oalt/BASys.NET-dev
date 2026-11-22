using System.Globalization;
using System.Resources;

namespace BASys.Resources
{
    /// <summary>
    /// Gives access to the localized strings of the application.
    /// </summary>
    public static class ResourceProvider
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("BASys.Resources.Strings", typeof(ResourceProvider).Assembly);

        /// <summary>
        /// Returns the string for the given key in the given culture or the key itself if it is unknown.
        /// </summary>
        public static string GetString(string key, CultureInfo culture)
        {
            string? text = _resourceManager.GetString(key, culture);
            string result = text ?? key;

            return result;
        }
    }
}

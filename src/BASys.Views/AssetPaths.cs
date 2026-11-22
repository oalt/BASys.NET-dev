namespace BASys.Views
{
    /// <summary>
    /// Helper to build URLs of the static assets of this library.
    /// </summary>
    public static class AssetPaths
    {
        public static string Image(string fileName)
        {
            string result = "_content/BASys.Views/images/" + fileName;

            return result;
        }
    }
}

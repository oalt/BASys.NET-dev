using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BASys.DataAccess
{
    /// <summary>
    /// Loads and saves XML documents in the format of the Java application (ISO-8859-1, indented by two blanks).
    /// </summary>
    public static class XmlFile
    {
        /// <summary>
        /// Loads the document. Throws if the file does not exist or is not valid XML.
        /// </summary>
        public static XDocument Load(string fileName)
        {
            XDocument result = XDocument.Load(fileName);

            return result;
        }

        public static void Save(XDocument document, string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.Latin1,
                Indent = true,
                IndentChars = "  "
            };

            string? directory = Path.GetDirectoryName(fileName);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                document.Save(writer);
            }
        }
    }
}

using System.Xml.Linq;

namespace BASys.DataModels
{
    /// <summary>
    /// Base class of all models that are stored as XML documents (port of the Java XMLDataModel).
    /// Every data element has an "id" attribute of the form "tagname-number" and "name" and "comment" children.
    /// </summary>
    public abstract class XmlDataModel
    {
        private readonly XDocument _document;

        protected XmlDataModel()
        {
            _document = new XDocument();
        }

        protected XmlDataModel(XDocument document)
        {
            _document = document;
        }

        /// <summary>
        /// Raised after the content of the model was changed.
        /// </summary>
        public event EventHandler? Changed;

        public XDocument Document
        {
            get
            {
                return _document;
            }
        }

        public string GetName(string id)
        {
            string result = ReadValue(GetDataRootElement(id), "name");

            return result;
        }

        public void SetName(string id, string name)
        {
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                WriteValue(element, "name", name);
            }
        }

        public string GetComment(string id)
        {
            string result = ReadValue(GetDataRootElement(id), "comment");

            return result;
        }

        public void SetComment(string id, string comment)
        {
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                WriteValue(element, "comment", comment);
            }
        }

        /// <summary>
        /// Finds the element that has the given id. The element name is derived from the id prefix.
        /// </summary>
        public XElement? GetDataRootElement(string id)
        {
            XElement? result = null;
            int separatorIndex = id.IndexOf('-');
            string elementName = separatorIndex != -1 ? id.Substring(0, separatorIndex) : id;

            foreach (XElement currentElement in _document.Descendants(elementName))
            {
                if ((string?)currentElement.Attribute("id") == id)
                {
                    result = currentElement;
                    break;
                }
            }

            return result;
        }

        public void RemoveData(string id)
        {
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                element.Remove();
                OnChanged();
            }
        }

        /// <summary>
        /// Returns the ids of all descendants of the node that have the given element name.
        /// </summary>
        protected List<string> GetIdList(XElement? node, string childName)
        {
            List<string> result = new List<string>();

            if (node != null)
            {
                foreach (XElement currentElement in node.Descendants(childName))
                {
                    string? id = (string?)currentElement.Attribute("id");

                    if (id != null)
                    {
                        result.Add(id);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a new data element with a free id and empty name and comment children.
        /// </summary>
        protected XElement CreateChild(string tagName)
        {
            string id = tagName + "-" + GetNewId(tagName);
            XElement result = new XElement(tagName, new XAttribute("id", id), new XElement("name"), new XElement("comment"));

            return result;
        }

        protected static string GetChildId(XElement childElement)
        {
            string result = (string?)childElement.Attribute("id") ?? string.Empty;

            return result;
        }

        private int GetNewId(string tagName)
        {
            int result = 0;
            string prefix = tagName + "-";

            foreach (XElement currentElement in _document.Descendants(tagName))
            {
                string? id = (string?)currentElement.Attribute("id");
                int number;

                if (id != null && id.StartsWith(prefix, StringComparison.Ordinal)
                    && int.TryParse(id.Substring(prefix.Length), out number) && number > result)
                {
                    result = number;
                }
            }

            result = result + 1;

            return result;
        }

        /// <summary>
        /// Reads the text of the element at the given path ("child/grandchild"). Returns an empty string if it does not exist.
        /// </summary>
        public static string ReadValue(XElement? element, string path)
        {
            XElement? current = element;
            string[] pathParts = path.Split('/');

            for (int counter = 0; counter < pathParts.Length && current != null; counter++)
            {
                current = current.Element(pathParts[counter]);
            }

            string result = current != null ? current.Value : string.Empty;

            return result;
        }

        /// <summary>
        /// Writes the text of the element at the given path and creates missing elements.
        /// </summary>
        public void WriteValue(XElement element, string path, string value)
        {
            XElement current = element;

            foreach (string currentPart in path.Split('/'))
            {
                XElement? next = current.Element(currentPart);

                if (next == null)
                {
                    next = new XElement(currentPart);
                    current.Add(next);
                }

                current = next;
            }

            current.Value = value;
            OnChanged();
        }

        protected void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}

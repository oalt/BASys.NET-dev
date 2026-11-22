using System.Xml.Linq;

namespace BASys.DataModels
{
    /// <summary>
    /// Architectural structure of a project (structure.xml): project, buildings, floors, rooms, junction boxes,
    /// end devices and the bus devices installed at a location.
    /// </summary>
    public class ArchitecturalDataModel : XmlDataModel
    {
        public const string RootId = "project-1";

        public const string ProjectPrefix = "project";

        public const string BuildingPrefix = "building";

        public const string FloorPrefix = "floor";

        public const string RoomPrefix = "room";

        public const string JunctionBoxPrefix = "junctionbox";

        public ArchitecturalDataModel(string projectName)
        {
            XElement projectElement = new XElement(ProjectPrefix, new XAttribute("id", RootId),
                new XElement("name", projectName),
                new XElement("comment"),
                new XElement("directoryname", CreateDirectoryName(projectName)),
                new XElement("preffered-bus-system"));
            Document.Add(new XElement("architectural-data", projectElement));
        }

        public ArchitecturalDataModel(XDocument document) : base(document)
        {
        }

        public string ProjectName
        {
            get
            {
                string result = GetName(RootId);

                return result;
            }
            set
            {
                SetName(RootId, value);
            }
        }

        /// <summary>
        /// Preferred bus system: "EIB", "LON", "LCN" or "none".
        /// </summary>
        public string PreferredBusSystem
        {
            get
            {
                string result = ReadValue(GetDataRootElement(RootId), "preffered-bus-system");

                return result;
            }
            set
            {
                XElement? projectElement = GetDataRootElement(RootId);

                if (projectElement != null)
                {
                    WriteValue(projectElement, "preffered-bus-system", value);
                }
            }
        }

        public string ProjectDirectoryName
        {
            get
            {
                string result = ReadValue(GetDataRootElement(RootId), "directoryname");

                return result;
            }
        }

        /// <summary>
        /// Returns the ids of the direct structural children (buildings of the project, floors of a building, ...).
        /// </summary>
        public List<string> GetChildIds(string parentId)
        {
            List<string> result = new List<string>();
            string? childName = GetChildElementName(parentId);
            XElement? parentElement = GetDataRootElement(parentId);

            if (childName != null && parentElement != null)
            {
                foreach (XElement currentChild in parentElement.Elements(childName))
                {
                    result.Add(GetChildId(currentChild));
                }
            }

            return result;
        }

        public string GetParentId(string childId)
        {
            string result = string.Empty;
            XElement? parentElement = GetDataRootElement(childId)?.Parent;

            if (parentElement != null)
            {
                result = (string?)parentElement.Attribute("id") ?? string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Adds a child of the next lower level (building to project, floor to building, room to floor, junction box to room).
        /// Returns the id of the new element or null if the parent cannot have children.
        /// </summary>
        public string? AddChild(string parentId, string name)
        {
            string? result = null;
            string? childName = GetChildElementName(parentId);
            XElement? parentElement = GetDataRootElement(parentId);

            if (childName != null && parentElement != null)
            {
                XElement childElement = CreateChild(childName);
                childElement.Element("name")!.Value = name;
                parentElement.Add(childElement);
                result = GetChildId(childElement);
                OnChanged();
            }

            return result;
        }

        /// <summary>
        /// Adds an end device (reference to a passive installation component) to a room.
        /// </summary>
        public void AddEndDevice(string roomId, string endDeviceId, int deviceType)
        {
            XElement? roomElement = GetDataRootElement(roomId);

            if (roomElement != null)
            {
                roomElement.Add(new XElement("enddevice", new XAttribute("id", endDeviceId), new XElement("type", deviceType)));
                OnChanged();
            }
        }

        /// <summary>
        /// Returns the end devices of a room as pairs of id and device type code.
        /// </summary>
        public List<KeyValuePair<string, int>> GetEndDevices(string roomId)
        {
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
            XElement? roomElement = GetDataRootElement(roomId);

            if (roomElement != null)
            {
                foreach (XElement currentDevice in roomElement.Elements("enddevice"))
                {
                    int deviceType;
                    int.TryParse(ReadValue(currentDevice, "type"), out deviceType);
                    result.Add(new KeyValuePair<string, int>(GetChildId(currentDevice), deviceType));
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a bus device (actuator or sensor of the installation model) to an installation location (room or junction box).
        /// </summary>
        public void AddBusDevice(string locationId, string deviceId)
        {
            XElement? locationElement = GetDataRootElement(locationId);

            if (locationElement != null)
            {
                locationElement.Add(new XElement("busdevice", deviceId));
                OnChanged();
            }
        }

        /// <summary>
        /// Returns the ids of the bus devices of a location. With includeNested the devices of nested locations
        /// (junction boxes of a room) are included.
        /// </summary>
        public List<string> GetBusDeviceIds(string locationId, bool includeNested)
        {
            List<string> result = new List<string>();
            XElement? locationElement = GetDataRootElement(locationId);

            if (locationElement != null)
            {
                IEnumerable<XElement> deviceElements = includeNested ? locationElement.Descendants("busdevice") : locationElement.Elements("busdevice");

                foreach (XElement currentDevice in deviceElements)
                {
                    result.Add(currentDevice.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the name of the location in which the bus device is installed.
        /// </summary>
        public string GetInstallationLocationName(string busDeviceId)
        {
            string result = string.Empty;

            foreach (XElement currentDevice in Document.Descendants("busdevice"))
            {
                if (currentDevice.Value == busDeviceId && currentDevice.Parent != null)
                {
                    result = ReadValue(currentDevice.Parent, "name");
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all junction boxes of the project as pairs of id and name.
        /// </summary>
        public List<KeyValuePair<string, string>> GetJunctionBoxList()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            foreach (string currentId in GetIdList(Document.Root, JunctionBoxPrefix))
            {
                result.Add(new KeyValuePair<string, string>(currentId, GetName(currentId)));
            }

            return result;
        }

        /// <summary>
        /// Returns the ids of all end devices and bus devices referenced below the given element.
        /// </summary>
        public void CollectReferencedDevices(string id, List<string> endDeviceIds, List<string> busDeviceIds)
        {
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                foreach (XElement currentDevice in element.Descendants("enddevice"))
                {
                    endDeviceIds.Add(GetChildId(currentDevice));
                }

                foreach (XElement currentDevice in element.Descendants("busdevice"))
                {
                    busDeviceIds.Add(currentDevice.Value);
                }
            }
        }

        private static string? GetChildElementName(string parentId)
        {
            string? result = null;

            if (parentId.StartsWith(ProjectPrefix, StringComparison.Ordinal))
            {
                result = BuildingPrefix;
            }
            else if (parentId.StartsWith(BuildingPrefix, StringComparison.Ordinal))
            {
                result = FloorPrefix;
            }
            else if (parentId.StartsWith(FloorPrefix, StringComparison.Ordinal))
            {
                result = RoomPrefix;
            }
            else if (parentId.StartsWith(RoomPrefix, StringComparison.Ordinal))
            {
                result = JunctionBoxPrefix;
            }

            return result;
        }

        private static string CreateDirectoryName(string projectName)
        {
            string result = projectName.Replace(" ", "_").Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue");

            return result;
        }
    }
}

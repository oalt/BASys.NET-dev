using System.Xml.Linq;

namespace BASys.DataModels
{
    /// <summary>
    /// Installation components of a project (installation.xml): sensors, actuators and passive components
    /// (end devices) with their properties, function groups, functions and connections.
    /// </summary>
    public class InstallationModel : XmlDataModel
    {
        private readonly XElement _sensorsElement;

        private readonly XElement _actuatorsElement;

        private readonly XElement _passiveElement;

        public InstallationModel()
        {
            _sensorsElement = new XElement("sensors");
            _actuatorsElement = new XElement("actuators");
            _passiveElement = new XElement("passive-components");
            Document.Add(new XElement("installation-components", new XElement("active-components", _sensorsElement, _actuatorsElement), _passiveElement));
        }

        public InstallationModel(XDocument document) : base(document)
        {
            _sensorsElement = GetOrCreate(document, "sensors");
            _actuatorsElement = GetOrCreate(document, "actuators");
            _passiveElement = GetOrCreate(document, "passive-components");
        }

        private static XElement GetOrCreate(XDocument document, string name)
        {
            XElement? result = document.Descendants(name).FirstOrDefault();

            if (result == null)
            {
                result = new XElement(name);
                document.Root!.Add(result);
            }

            return result;
        }

        public string AddSensor(string name)
        {
            XElement element = CreateChild("sensor");
            element.Add(new XElement("sensor-type"), new XElement("properties"), new XElement("connections"));
            _sensorsElement.Add(element);

            string result = FinishAdd(element, name);

            return result;
        }

        public string AddActuator(string name)
        {
            XElement element = CreateChild("actuator");
            element.Add(new XElement("properties"), new XElement("connections"));
            _actuatorsElement.Add(element);

            string result = FinishAdd(element, name);

            return result;
        }

        public string AddPassiveComponent(string name)
        {
            XElement element = CreateChild("passive");
            element.Add(new XElement("passive-type"), new XElement("properties"), new XElement("connections"));
            _passiveElement.Add(element);

            string result = FinishAdd(element, name);

            return result;
        }

        private string FinishAdd(XElement element, string name)
        {
            string result = GetChildId(element);
            element.Element("name")!.Value = name;
            OnChanged();

            return result;
        }

        public string AddFunctionGroup(string parentId)
        {
            XElement element = CreateChild("functiongroup");
            GetDataRootElement(parentId)!.Add(element);
            OnChanged();

            string result = GetChildId(element);

            return result;
        }

        public string AddFunction(string functionGroupId)
        {
            XElement element = CreateChild("function");
            GetDataRootElement(functionGroupId)!.Add(element);
            OnChanged();

            string result = GetChildId(element);

            return result;
        }

        /// <summary>
        /// Returns the ids of the passive components that are installation devices (type below 100).
        /// </summary>
        public List<string> GetInstallationDeviceIds()
        {
            List<string> result = new List<string>();

            foreach (string currentId in GetIdList(_passiveElement, "passive"))
            {
                int type;

                if (int.TryParse(GetPropertyByName(currentId, "type"), out type) && type < 100)
                {
                    result.Add(currentId);
                }
            }

            return result;
        }

        public List<string> GetActuatorIds()
        {
            List<string> result = GetIdList(_actuatorsElement, "actuator");

            return result;
        }

        public List<string> GetSensorIds()
        {
            List<string> result = GetIdList(_sensorsElement, "sensor");

            return result;
        }

        public void SetProperty(string id, string propertyName, string value)
        {
            XElement element = GetDataRootElement(id)!;
            XElement propertiesElement = element.Element("properties") ?? AddElement(element, "properties");
            XElement? propertyElement = FindProperty(propertiesElement, propertyName);

            if (propertyElement == null)
            {
                propertyElement = new XElement("property", new XElement("name", propertyName), new XElement("value"));
                propertiesElement.Add(propertyElement);
            }

            propertyElement.Element("value")!.Value = value;
            OnChanged();
        }

        private static XElement? FindProperty(XElement propertiesElement, string propertyName)
        {
            XElement? result = null;

            foreach (XElement currentProperty in propertiesElement.Elements("property"))
            {
                if (ReadValue(currentProperty, "name") == propertyName)
                {
                    result = currentProperty;
                    break;
                }
            }

            return result;
        }

        private static XElement AddElement(XElement parent, string name)
        {
            XElement result = new XElement(name);
            parent.Add(result);

            return result;
        }

        public List<KeyValuePair<string, string>> GetProperties(string id)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                foreach (XElement currentProperty in element.Descendants("property"))
                {
                    result.Add(new KeyValuePair<string, string>(ReadValue(currentProperty, "name"), ReadValue(currentProperty, "value")));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the property or null if the property does not exist.
        /// </summary>
        public string? GetPropertyByName(string id, string propertyName)
        {
            string? result = null;

            foreach (KeyValuePair<string, string> currentProperty in GetProperties(id))
            {
                if (currentProperty.Key == propertyName)
                {
                    result = currentProperty.Value;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the property or an empty string if it does not exist.
        /// </summary>
        public string GetProperty(string id, string propertyName)
        {
            string result = GetPropertyByName(id, propertyName) ?? string.Empty;

            return result;
        }

        public void AddConnection(string sourceId, string destinationId)
        {
            XElement element = GetDataRootElement(sourceId)!;
            XElement connectionsElement = element.Element("connections") ?? AddElement(element, "connections");
            bool exists = false;

            foreach (XElement currentConnection in connectionsElement.Elements("connection"))
            {
                if (currentConnection.Value == destinationId)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                connectionsElement.Add(new XElement("connection", destinationId));
                OnChanged();
            }
        }

        public List<string> GetConnections(string id)
        {
            List<string> result = new List<string>();
            XElement? connectionsElement = GetDataRootElement(id)?.Element("connections");

            if (connectionsElement != null)
            {
                foreach (XElement currentConnection in connectionsElement.Elements("connection"))
                {
                    result.Add(currentConnection.Value);
                }
            }

            return result;
        }

        public void RemoveConnection(string sourceId, string destinationId)
        {
            XElement? connectionsElement = GetDataRootElement(sourceId)?.Element("connections");

            if (connectionsElement != null)
            {
                foreach (XElement currentConnection in connectionsElement.Elements("connection").ToList())
                {
                    if (currentConnection.Value == destinationId)
                    {
                        currentConnection.Remove();
                        OnChanged();
                    }
                }
            }
        }

        public void RemoveAllConnections(string sourceId)
        {
            XElement? connectionsElement = GetDataRootElement(sourceId)?.Element("connections");

            if (connectionsElement != null)
            {
                connectionsElement.RemoveNodes();
                OnChanged();
            }
        }

        /// <summary>
        /// Removes a component and all connections that point to it or to one of its function groups.
        /// </summary>
        public void RemoveComponent(string id)
        {
            XElement? element = GetDataRootElement(id);

            if (element != null)
            {
                List<string> removedIds = GetIdList(element, "functiongroup");
                removedIds.Add(id);
                element.Remove();

                foreach (XElement currentConnection in Document.Descendants("connection").ToList())
                {
                    if (removedIds.Contains(currentConnection.Value))
                    {
                        currentConnection.Remove();
                    }
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Finds devices of the given location and type that have a function group whose unused functions
        /// contain all required function types.
        /// </summary>
        public List<string> FindMatchingDevices(string installationType, string[] functions, string deviceType, IEnumerable<string> deviceIds)
        {
            List<string> result = new List<string>();

            foreach (string currentId in deviceIds)
            {
                if (GetProperty(currentId, "installation-location") == installationType && currentId.StartsWith(deviceType, StringComparison.Ordinal)
                    && GetMatchingFunctionGroups(currentId, functions).Count > 0)
                {
                    result.Add(currentId);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the function types of the function group an end device is connected to.
        /// </summary>
        public string[] GetFunctionsForEndDevice(string passiveId)
        {
            List<string> functionTypes = new List<string>();
            List<string> connections = GetConnections(passiveId);

            if (connections.Count > 0)
            {
                foreach (string currentFunctionId in GetIdList(GetDataRootElement(connections[0]), "function"))
                {
                    string type = ReadValue(GetDataRootElement(currentFunctionId), "type");

                    if (type != "unused")
                    {
                        functionTypes.Add(type);
                    }
                }
            }

            string[] result = functionTypes.ToArray();

            return result;
        }

        public string GetEibDeviceId(string functionGroupId)
        {
            string result = (string?)GetDataRootElement(functionGroupId)?.Parent?.Attribute("id") ?? string.Empty;

            return result;
        }

        /// <summary>
        /// Returns the ids of all EIB actuators and sensors whose state is not "ready".
        /// </summary>
        public List<string> GetUnprogrammedEibDeviceIds()
        {
            List<string> result = new List<string>();

            foreach (string currentId in GetActuatorIds().Concat(GetSensorIds()))
            {
                if (GetProperty(currentId, "bussystem") == "EIB" && GetProperty(currentId, "device-state") != "ready")
                {
                    result.Add(currentId);
                }
            }

            return result;
        }

        public int FindFreeEibGroupAddress()
        {
            int result = 0;

            foreach (XElement currentFunction in Document.Descendants("function"))
            {
                int count;

                if (int.TryParse(ReadValue(currentFunction, "eib-group-address-count"), out count) && count >= result)
                {
                    result = count;
                }
            }

            result = result + 1;

            return result;
        }

        /// <summary>
        /// Assigns new group addresses to the unused functions of the function group that match the function types.
        /// </summary>
        public void SetGroupAddresses(string[] functions, string functionGroupId)
        {
            foreach (string currentType in functions)
            {
                foreach (string currentFunctionId in GetIdList(GetDataRootElement(functionGroupId), "function"))
                {
                    XElement functionElement = GetDataRootElement(currentFunctionId)!;

                    if (ReadValue(functionElement, "type") == currentType)
                    {
                        WriteValue(functionElement, "eib-group-address-count", FindFreeEibGroupAddress().ToString());
                        WriteValue(functionElement, "state", "addressed");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Copies the group addresses of the actuator function group to the matching functions of a sensor function group.
        /// </summary>
        public void SetSensorGroupAddresses(string actuatorFunctionGroupId, string sensorFunctionGroupId)
        {
            foreach (string currentActuatorFunctionId in GetIdList(GetDataRootElement(actuatorFunctionGroupId), "function"))
            {
                XElement actuatorFunction = GetDataRootElement(currentActuatorFunctionId)!;
                string actuatorType = ReadValue(actuatorFunction, "type");

                foreach (string currentSensorFunctionId in GetIdList(GetDataRootElement(sensorFunctionGroupId), "function"))
                {
                    XElement sensorFunction = GetDataRootElement(currentSensorFunctionId)!;

                    if (ReadValue(sensorFunction, "type") == actuatorType && actuatorType != "unused")
                    {
                        WriteValue(sensorFunction, "eib-group-address-count", ReadValue(actuatorFunction, "eib-group-address-count"));
                        WriteValue(sensorFunction, "state", "addressed");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the function groups of a device that are unused, as pairs of function group id and function name.
        /// </summary>
        public List<KeyValuePair<string, string>> GetFunctionGroupList(string busDeviceId)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            foreach (string currentGroupId in GetIdList(GetDataRootElement(busDeviceId), "functiongroup"))
            {
                List<string> functionIds = GetIdList(GetDataRootElement(currentGroupId), "function");

                if (functionIds.Count > 0 && ReadValue(GetDataRootElement(functionIds[0]), "state") == "unused")
                {
                    result.Add(new KeyValuePair<string, string>(currentGroupId, GetName(functionIds[0])));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the group addresses (as text like 0/257) of all addressed functions of a function group.
        /// </summary>
        public List<string> GetGroupAddresses(string functionGroupId)
        {
            List<string> result = new List<string>();

            foreach (string currentFunctionId in GetIdList(GetDataRootElement(functionGroupId), "function"))
            {
                XElement functionElement = GetDataRootElement(currentFunctionId)!;
                int count;

                if (ReadValue(functionElement, "state") == "addressed" && int.TryParse(ReadValue(functionElement, "eib-group-address-count"), out count))
                {
                    result.Add(EibAddress.FormatGroupAddress(count));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the function groups of a device whose unused functions contain all required function types.
        /// </summary>
        public List<string> GetMatchingFunctionGroups(string busDeviceId, string[] functions)
        {
            List<string> result = new List<string>();

            foreach (string currentGroupId in GetIdList(GetDataRootElement(busDeviceId), "functiongroup"))
            {
                List<string> functionIds = GetIdList(GetDataRootElement(currentGroupId), "function");
                bool isIncluded = true;

                foreach (string currentType in functions)
                {
                    bool match = false;

                    foreach (string currentFunctionId in functionIds)
                    {
                        XElement functionElement = GetDataRootElement(currentFunctionId)!;

                        if (ReadValue(functionElement, "type") == currentType && ReadValue(functionElement, "state") == "unused")
                        {
                            match = true;
                            break;
                        }
                    }

                    isIncluded = isIncluded && match;
                }

                if (isIncluded)
                {
                    result.Add(currentGroupId);
                }
            }

            return result;
        }

        public bool IsEibPhysicalAddressInUse(string physicalAddress)
        {
            bool result = false;

            foreach (string currentId in GetActuatorIds().Concat(GetSensorIds()))
            {
                if (GetProperty(currentId, "bussystem") == "EIB" && GetProperty(currentId, "eib-physical-address") == physicalAddress)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}

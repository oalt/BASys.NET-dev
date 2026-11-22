using System.Xml.Linq;

namespace BASys.DataModels
{
    /// <summary>
    /// Database of known EIB devices (eib-devices.xml) with function groups and application data.
    /// </summary>
    public class EibDevicesDataModel : XmlDataModel
    {
        private readonly XElement _devicesElement;

        public EibDevicesDataModel()
        {
            _devicesElement = new XElement("eib-devices");
            Document.Add(_devicesElement);
        }

        public EibDevicesDataModel(XDocument document) : base(document)
        {
            XElement? devicesElement = document.Root;

            if (devicesElement == null || devicesElement.Name != "eib-devices")
            {
                devicesElement = new XElement("eib-devices");
                document.RemoveNodes();
                document.Add(devicesElement);
            }

            _devicesElement = devicesElement;
        }

        public string AddDevice(string name)
        {
            XElement element = CreateChild("eibdevice");
            element.Element("name")!.Value = name;
            element.Add(new XElement("functions"));
            _devicesElement.Add(element);
            OnChanged();

            string result = GetChildId(element);

            return result;
        }

        public void SetApplication(string deviceId, EibApplication application)
        {
            XElement? element = GetDataRootElement(deviceId);

            if (element != null)
            {
                element.Element("application")?.Remove();
                element.Add(application.ToXElement());
                OnChanged();
            }
        }

        public EibApplication GetApplication(string deviceId)
        {
            XElement? applicationElement = GetDataRootElement(deviceId)?.Element("application");
            EibApplication result = applicationElement != null ? EibApplication.FromXElement(applicationElement) : new EibApplication();

            return result;
        }

        public string GetDeviceType(string id)
        {
            string result = ReadValue(GetDataRootElement(id), "type");

            return result;
        }

        public void SetDeviceType(string id, string value)
        {
            WriteValue(GetDataRootElement(id)!, "type", value);
        }

        public string GetInstallationType(string id)
        {
            string result = ReadValue(GetDataRootElement(id), "installation-type");

            return result;
        }

        public void SetInstallationType(string id, string value)
        {
            WriteValue(GetDataRootElement(id)!, "installation-type", value);
        }

        public string GetManufacturerName(string deviceId)
        {
            string result = ReadValue(GetDataRootElement(deviceId), "manufacturer");

            return result;
        }

        public void SetManufacturer(string deviceId, string name)
        {
            WriteValue(GetDataRootElement(deviceId)!, "manufacturer", name);
        }

        public string AddFunctionGroup(string deviceId)
        {
            XElement element = CreateChild("functiongroup");
            XElement functionsElement = GetDataRootElement(deviceId)!.Element("functions")!;
            functionsElement.Add(element);
            OnChanged();

            string result = GetChildId(element);

            return result;
        }

        public void AddFunction(string functionGroupId, EibDeviceFunction function)
        {
            XElement element = CreateChild("function");
            GetDataRootElement(functionGroupId)!.Add(element);
            element.Element("name")!.Value = function.Name;
            element.Add(new XElement("type", function.Type),
                new XElement("com-object", function.ComObject),
                new XElement("function-name", function.FunctionName),
                new XElement("eis-type", function.EisType));
            OnChanged();
        }

        public List<string> GetFunctionGroupIds(string deviceId)
        {
            List<string> result = GetIdList(GetDataRootElement(deviceId), "functiongroup");

            return result;
        }

        public List<string> GetFunctionIds(string functionGroupId)
        {
            List<string> result = GetIdList(GetDataRootElement(functionGroupId), "function");

            return result;
        }

        public List<string> GetDeviceIds()
        {
            List<string> result = GetIdList(_devicesElement, "eibdevice");

            return result;
        }

        public int DeviceCount
        {
            get
            {
                int result = GetDeviceIds().Count;

                return result;
            }
        }

        /// <summary>
        /// Finds database devices of the installation type and device type that have a function group containing all function types.
        /// </summary>
        public List<string> FindMatchingDevices(string installationType, string[] functions, string deviceType)
        {
            List<string> result = new List<string>();

            foreach (string currentId in GetDeviceIds())
            {
                if (GetInstallationType(currentId) == installationType && GetDeviceType(currentId) == deviceType)
                {
                    foreach (string currentGroupId in GetFunctionGroupIds(currentId))
                    {
                        if (ContainsAllFunctions(currentGroupId, functions))
                        {
                            result.Add(currentId);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private bool ContainsAllFunctions(string functionGroupId, string[] functions)
        {
            bool result = true;
            List<string> functionIds = GetFunctionIds(functionGroupId);

            foreach (string currentType in functions)
            {
                bool match = false;

                foreach (string currentFunctionId in functionIds)
                {
                    if (ReadValue(GetDataRootElement(currentFunctionId), "type") == currentType)
                    {
                        match = true;
                        break;
                    }
                }

                result = result && match;
            }

            return result;
        }
    }
}

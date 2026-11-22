using System.Globalization;
using System.Xml.Linq;

namespace BASys.DataModels
{
    /// <summary>
    /// Application program of an EIB product (data of the product database and the "application" element of eib-devices.xml).
    /// </summary>
    public class EibApplication
    {
        private string _programId = string.Empty;

        public string ProgramId
        {
            get
            {
                return _programId;
            }
            set
            {
                _programId = value;
            }
        }

        private string _maskId = string.Empty;

        public string MaskId
        {
            get
            {
                return _maskId;
            }
            set
            {
                _maskId = value;
            }
        }

        private string _maskVersion = string.Empty;

        public string MaskVersion
        {
            get
            {
                return _maskVersion;
            }
            set
            {
                _maskVersion = value;
            }
        }

        private string _maskVersionName = string.Empty;

        public string MaskVersionName
        {
            get
            {
                return _maskVersionName;
            }
            set
            {
                _maskVersionName = value;
            }
        }

        private string _name = string.Empty;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _version = string.Empty;

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        private string _deviceType = "0";

        public string DeviceType
        {
            get
            {
                return _deviceType;
            }
            set
            {
                _deviceType = value;
            }
        }

        private string _peiType = "0";

        public string PeiType
        {
            get
            {
                return _peiType;
            }
            set
            {
                _peiType = value;
            }
        }

        private string _addressTableAddress = "0";

        public string AddressTableAddress
        {
            get
            {
                return _addressTableAddress;
            }
            set
            {
                _addressTableAddress = value;
            }
        }

        private string _addressTableSize = "0";

        public string AddressTableSize
        {
            get
            {
                return _addressTableSize;
            }
            set
            {
                _addressTableSize = value;
            }
        }

        private string _associationTableAddress = "0";

        public string AssociationTableAddress
        {
            get
            {
                return _associationTableAddress;
            }
            set
            {
                _associationTableAddress = value;
            }
        }

        private string _associationTableSize = "0";

        public string AssociationTableSize
        {
            get
            {
                return _associationTableSize;
            }
            set
            {
                _associationTableSize = value;
            }
        }

        private string _associationTablePointer = "0";

        public string AssociationTablePointer
        {
            get
            {
                return _associationTablePointer;
            }
            set
            {
                _associationTablePointer = value;
            }
        }

        private string _communicationTableAddress = "0";

        public string CommunicationTableAddress
        {
            get
            {
                return _communicationTableAddress;
            }
            set
            {
                _communicationTableAddress = value;
            }
        }

        private string _communicationTableSize = "0";

        public string CommunicationTableSize
        {
            get
            {
                return _communicationTableSize;
            }
            set
            {
                _communicationTableSize = value;
            }
        }

        private string _communicationTablePointer = "0";

        public string CommunicationTablePointer
        {
            get
            {
                return _communicationTablePointer;
            }
            set
            {
                _communicationTablePointer = value;
            }
        }

        private string _manufacturerId = string.Empty;

        public string ManufacturerId
        {
            get
            {
                return _manufacturerId;
            }
            set
            {
                _manufacturerId = value;
            }
        }

        private string _originalManufacturerId = string.Empty;

        public string OriginalManufacturerId
        {
            get
            {
                return _originalManufacturerId;
            }
            set
            {
                _originalManufacturerId = value;
            }
        }

        private string _eepromData = string.Empty;

        public string EepromData
        {
            get
            {
                return _eepromData;
            }
            set
            {
                _eepromData = value;
            }
        }

        private string _userEepromStart = "0";

        public string UserEepromStart
        {
            get
            {
                return _userEepromStart;
            }
            set
            {
                _userEepromStart = value;
            }
        }

        private string _userEepromEnd = "0";

        public string UserEepromEnd
        {
            get
            {
                return _userEepromEnd;
            }
            set
            {
                _userEepromEnd = value;
            }
        }

        private string _userRamStart = "0";

        public string UserRamStart
        {
            get
            {
                return _userRamStart;
            }
            set
            {
                _userRamStart = value;
            }
        }

        private string _userRamEnd = "0";

        public string UserRamEnd
        {
            get
            {
                return _userRamEnd;
            }
            set
            {
                _userRamEnd = value;
            }
        }

        private string _runErrorAddress = "0";

        public string RunErrorAddress
        {
            get
            {
                return _runErrorAddress;
            }
            set
            {
                _runErrorAddress = value;
            }
        }

        private string _manufacturerDataAddress = "0";

        public string ManufacturerDataAddress
        {
            get
            {
                return _manufacturerDataAddress;
            }
            set
            {
                _manufacturerDataAddress = value;
            }
        }

        private string _manufacturerDataSize = "0";

        public string ManufacturerDataSize
        {
            get
            {
                return _manufacturerDataSize;
            }
            set
            {
                _manufacturerDataSize = value;
            }
        }

        private string _manufacturerIdAddress = "0";

        public string ManufacturerIdAddress
        {
            get
            {
                return _manufacturerIdAddress;
            }
            set
            {
                _manufacturerIdAddress = value;
            }
        }

        private string _manufacturerIdProtected = "true";

        public string ManufacturerIdProtected
        {
            get
            {
                return _manufacturerIdProtected;
            }
            set
            {
                _manufacturerIdProtected = value;
            }
        }

        private string _routeCounterAddress = "0";

        public string RouteCounterAddress
        {
            get
            {
                return _routeCounterAddress;
            }
            set
            {
                _routeCounterAddress = value;
            }
        }

        public override string ToString()
        {
            string result = _name;

            return result;
        }

        /// <summary>
        /// Creates the "application" element as stored in eib-devices.xml.
        /// </summary>
        public XElement ToXElement()
        {
            XElement result = new XElement("application", new XAttribute("id", _programId),
                new XElement("mask-version", _maskVersion),
                new XElement("mask-version-name", _maskVersionName),
                new XElement("name", _name),
                new XElement("version", _version),
                new XElement("device-type", _deviceType),
                new XElement("pei-type", _peiType),
                new XElement("addr-table-address", _addressTableAddress),
                new XElement("addr-table-size", _addressTableSize),
                new XElement("association-table-address", _associationTableAddress),
                new XElement("association-table-size", _associationTableSize),
                new XElement("association-table-pointer", _associationTablePointer),
                new XElement("communication-table-address", _communicationTableAddress),
                new XElement("communication-table-size", _communicationTableSize),
                new XElement("communication-table-pointer", _communicationTablePointer),
                new XElement("manufacturer", _manufacturerId),
                new XElement("original-manufacturer", _originalManufacturerId),
                new XElement("eeprom-data", _eepromData),
                new XElement("eeprom-start", _userEepromStart),
                new XElement("eeprom-end", _userEepromEnd),
                new XElement("uram-start", _userRamStart),
                new XElement("uram-end", _userRamEnd),
                new XElement("run-error-address", _runErrorAddress),
                new XElement("manufacturer-data-address", _manufacturerDataAddress),
                new XElement("manufacturer-data-size", _manufacturerDataSize),
                new XElement("manufacturer-id-address", _manufacturerIdAddress),
                new XElement("manufacturer-id-protected", _manufacturerIdProtected),
                new XElement("route-counter-address", _routeCounterAddress));

            return result;
        }

        /// <summary>
        /// Reads an "application" element of eib-devices.xml.
        /// </summary>
        public static EibApplication FromXElement(XElement element)
        {
            EibApplication result = new EibApplication
            {
                ProgramId = (string?)element.Attribute("id") ?? string.Empty,
                MaskVersion = XmlDataModel.ReadValue(element, "mask-version"),
                MaskVersionName = XmlDataModel.ReadValue(element, "mask-version-name"),
                Name = XmlDataModel.ReadValue(element, "name"),
                Version = XmlDataModel.ReadValue(element, "version"),
                DeviceType = XmlDataModel.ReadValue(element, "device-type"),
                PeiType = XmlDataModel.ReadValue(element, "pei-type"),
                AddressTableAddress = XmlDataModel.ReadValue(element, "addr-table-address"),
                AddressTableSize = XmlDataModel.ReadValue(element, "addr-table-size"),
                AssociationTableAddress = XmlDataModel.ReadValue(element, "association-table-address"),
                AssociationTableSize = XmlDataModel.ReadValue(element, "association-table-size"),
                AssociationTablePointer = XmlDataModel.ReadValue(element, "association-table-pointer"),
                CommunicationTableAddress = XmlDataModel.ReadValue(element, "communication-table-address"),
                CommunicationTableSize = XmlDataModel.ReadValue(element, "communication-table-size"),
                CommunicationTablePointer = XmlDataModel.ReadValue(element, "communication-table-pointer"),
                ManufacturerId = XmlDataModel.ReadValue(element, "manufacturer"),
                OriginalManufacturerId = XmlDataModel.ReadValue(element, "original-manufacturer"),
                EepromData = XmlDataModel.ReadValue(element, "eeprom-data"),
                UserEepromStart = XmlDataModel.ReadValue(element, "eeprom-start"),
                UserEepromEnd = XmlDataModel.ReadValue(element, "eeprom-end"),
                UserRamStart = XmlDataModel.ReadValue(element, "uram-start"),
                UserRamEnd = XmlDataModel.ReadValue(element, "uram-end"),
                RunErrorAddress = XmlDataModel.ReadValue(element, "run-error-address"),
                ManufacturerDataAddress = XmlDataModel.ReadValue(element, "manufacturer-data-address"),
                ManufacturerDataSize = XmlDataModel.ReadValue(element, "manufacturer-data-size"),
                ManufacturerIdAddress = XmlDataModel.ReadValue(element, "manufacturer-id-address"),
                ManufacturerIdProtected = XmlDataModel.ReadValue(element, "manufacturer-id-protected"),
                RouteCounterAddress = XmlDataModel.ReadValue(element, "route-counter-address")
            };

            return result;
        }
    }
}

using System.Text;
using System.Xml;
using System.Xml.Linq;
using BASys.DataModels;

namespace BASys.DataAccess
{
    /// <summary>
    /// Reads a manufacturer product data file (the XML form of an ETS *.vd_ file, see <see cref="VdConverter"/>).
    /// The files are large, therefore every query streams through the file instead of loading it completely.
    /// </summary>
    public class EibProductDataReader
    {
        private readonly string _fileName;

        public EibProductDataReader(string fileName)
        {
            _fileName = fileName;
        }

        public List<EibManufacturer> GetManufacturers()
        {
            List<EibManufacturer> result = new List<EibManufacturer>();

            foreach (Dictionary<string, string> currentRecord in ReadRecords("manufacturer", null))
            {
                result.Add(new EibManufacturer { Id = Get(currentRecord, "MANUFACTURER_ID"), Name = Get(currentRecord, "MANUFACTURER_NAME") });
            }

            return result;
        }

        public string GetManufacturerName(string manufacturerId)
        {
            string result = string.Empty;

            foreach (EibManufacturer currentManufacturer in GetManufacturers())
            {
                if (currentManufacturer.Id == manufacturerId)
                {
                    result = currentManufacturer.Name;
                    break;
                }
            }

            return result;
        }

        public List<EibProduct> GetProducts()
        {
            List<EibProduct> result = new List<EibProduct>();

            foreach (Dictionary<string, string> currentRecord in ReadRecords("hw_product", null))
            {
                result.Add(new EibProduct { Id = Get(currentRecord, "PRODUCT_ID"), Name = Get(currentRecord, "PRODUCT_NAME") });
            }

            return result;
        }

        /// <summary>
        /// Returns the ids of the application programs that belong to the product.
        /// </summary>
        public List<string> GetProgramIdsForProduct(string productId)
        {
            List<string> result = new List<string>();

            foreach (Dictionary<string, string> currentRecord in ReadRecords("product_to_program", "PRODUCT_ID=" + productId))
            {
                string programId = Get(currentRecord, "PROGRAM_ID");

                if (programId.Length > 0)
                {
                    result.Add(programId);
                }
            }

            return result;
        }

        public List<EibApplication> GetApplications(IReadOnlyCollection<string> programIds)
        {
            List<EibApplication> result = new List<EibApplication>();

            foreach (Dictionary<string, string> currentRecord in ReadRecords("application_program", null))
            {
                if (programIds.Contains(Get(currentRecord, "PROGRAM_ID")))
                {
                    result.Add(new EibApplication
                    {
                        ProgramId = Get(currentRecord, "PROGRAM_ID"),
                        MaskId = Get(currentRecord, "MASK_ID"),
                        Name = Get(currentRecord, "PROGRAM_NAME"),
                        Version = Get(currentRecord, "PROGRAM_VERSION"),
                        DeviceType = Get(currentRecord, "DEVICE_TYPE", "0"),
                        PeiType = Get(currentRecord, "PEI_TYPE", "0"),
                        AddressTableSize = Get(currentRecord, "ADDRESS_TAB_SIZE", "0"),
                        AssociationTableAddress = Get(currentRecord, "ASSOCTAB_ADDRESS", "0"),
                        AssociationTableSize = Get(currentRecord, "ASSOCTAB_SIZE", "0"),
                        CommunicationTableAddress = Get(currentRecord, "COMMSTAB_ADDRESS", "0"),
                        CommunicationTableSize = Get(currentRecord, "COMMSTAB_SIZE", "0"),
                        ManufacturerId = Get(currentRecord, "MANUFACTURER_ID"),
                        EepromData = Get(currentRecord, "EEPROM_DATA"),
                        OriginalManufacturerId = Get(currentRecord, "ORIGINAL_MANUFACTURER_ID")
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Completes the application with the data of its mask version.
        /// </summary>
        public void ApplyMaskData(EibApplication application)
        {
            foreach (Dictionary<string, string> currentRecord in ReadRecords("mask", "MASK_ID=" + application.MaskId))
            {
                application.MaskVersion = Get(currentRecord, "MASK_VERSION");
                application.UserRamStart = Get(currentRecord, "USER_RAM_START", "0");
                application.UserRamEnd = Get(currentRecord, "USER_RAM_END", "0");
                application.UserEepromStart = Get(currentRecord, "USER_EEPROM_START", "0");
                application.UserEepromEnd = Get(currentRecord, "USER_EEPROM_END", "0");
                application.RunErrorAddress = Get(currentRecord, "RUN_ERROR_ADDRESS", "0");
                application.AddressTableAddress = Get(currentRecord, "ADDRESS_TAB_ADDRESS", "0");
                application.AssociationTablePointer = Get(currentRecord, "ASSOCTABPTR_ADDRESS", "0");
                application.CommunicationTablePointer = Get(currentRecord, "COMMSTABPTR_ADDRESS", "0");
                application.ManufacturerDataAddress = Get(currentRecord, "MANUFACTURER_DATA_ADDRESS", "0");
                application.ManufacturerDataSize = Get(currentRecord, "MANUFACTURER_DATA_SIZE", "0");
                application.ManufacturerIdAddress = Get(currentRecord, "MANUFACTURER_ID_ADDRESS", "0");
                application.RouteCounterAddress = Get(currentRecord, "ROUTECNT_ADDRESS", "0");
                application.ManufacturerIdProtected = Get(currentRecord, "MANUFACTURER_ID_PROTECTED") == "1" ? "true" : "false";
                application.MaskVersionName = Get(currentRecord, "MASK_VERSION_NAME");
            }
        }

        /// <summary>
        /// Returns the communication objects of an application program in file order.
        /// </summary>
        public List<EibCommunicationObject> GetCommunicationObjects(string programId)
        {
            List<EibCommunicationObject> result = new List<EibCommunicationObject>();

            foreach (Dictionary<string, string> currentRecord in ReadRecords("communication_object", "PROGRAM_ID=" + programId))
            {
                result.Add(new EibCommunicationObject
                {
                    Id = Get(currentRecord, "OBJECT_ID"),
                    Name = Get(currentRecord, "OBJECT_NAME"),
                    Function = Get(currentRecord, "OBJECT_FUNCTION"),
                    Number = Get(currentRecord, "OBJECT_NUMBER"),
                    Type = Get(currentRecord, "OBJECT_TYPE"),
                    ReadEnabled = Get(currentRecord, "OBJECT_READENABLED"),
                    WriteEnabled = Get(currentRecord, "OBJECT_WRITEENABLED"),
                    CommunicationEnabled = Get(currentRecord, "OBJECT_COMMENABLED")
                });
            }

            return result;
        }

        /// <summary>
        /// Merges communication objects with the same number into one object (functions joined by " * ")
        /// and sorts the result by the object number.
        /// </summary>
        public static List<EibCommunicationObject> NormalizeCommunicationObjects(IEnumerable<EibCommunicationObject> communicationObjects)
        {
            List<EibCommunicationObject> result = new List<EibCommunicationObject>();
            Dictionary<int, EibCommunicationObject> mergedObjects = new Dictionary<int, EibCommunicationObject>();

            foreach (EibCommunicationObject currentObject in communicationObjects)
            {
                int number;
                int.TryParse(currentObject.Number, out number);
                EibCommunicationObject? mergedObject;

                if (mergedObjects.TryGetValue(number, out mergedObject))
                {
                    mergedObject.Function = mergedObject.Function + " * " + currentObject.Function;
                }
                else
                {
                    mergedObject = new EibCommunicationObject
                    {
                        Name = currentObject.Name,
                        Type = currentObject.Type,
                        Function = currentObject.Function,
                        Number = currentObject.Number
                    };
                    mergedObjects[number] = mergedObject;
                }
            }

            foreach (int currentNumber in mergedObjects.Keys.OrderBy(number => number))
            {
                result.Add(mergedObjects[currentNumber]);
            }

            return result;
        }

        private static string Get(Dictionary<string, string> record, string fieldName)
        {
            string result = Get(record, fieldName, string.Empty);

            return result;
        }

        private static string Get(Dictionary<string, string> record, string fieldName, string defaultValue)
        {
            string? value;
            string result = record.TryGetValue(fieldName, out value) ? value : defaultValue;

            return result;
        }

        /// <summary>
        /// Streams through the file and returns the records of the given type. The optional filter has the form "FIELD=value".
        /// </summary>
        private IEnumerable<Dictionary<string, string>> ReadRecords(string recordName, string? filter)
        {
            string? filterField = null;
            string? filterValue = null;

            if (filter != null)
            {
                int separatorIndex = filter.IndexOf('=');
                filterField = filter.Substring(0, separatorIndex);
                filterValue = filter.Substring(separatorIndex + 1);
            }

            XmlReaderSettings settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true, CheckCharacters = false };

            using (FileStream stream = File.OpenRead(_fileName))
            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                reader.MoveToContent();
                reader.Read();

                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == recordName)
                    {
                        XElement recordElement = (XElement)XNode.ReadFrom(reader);
                        Dictionary<string, string> record = new Dictionary<string, string>();

                        foreach (XElement currentField in recordElement.Elements())
                        {
                            record[currentField.Name.LocalName] = currentField.Value;
                        }

                        if (filterField == null || (record.TryGetValue(filterField, out string? fieldValue) && fieldValue == filterValue))
                        {
                            yield return record;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        reader.Skip();
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }
    }
}

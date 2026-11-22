using BASys.DataAccess;
using BASys.DataModels;

namespace BASys.Tests
{
    public class ProductDataTests : IDisposable
    {
        private readonly string _tempDirectory;

        public ProductDataTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "basys-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirectory, true);
        }

        private static EibProductDataReader CreateReader()
        {
            EibProductDataReader result = new EibProductDataReader(Path.Combine(AppContext.BaseDirectory, "TestData", "products-excerpt.xml"));

            return result;
        }

        [Fact]
        public void Reader_ReadsProductsApplicationsAndCommunicationObjects()
        {
            EibProductDataReader reader = CreateReader();

            List<EibManufacturer> manufacturers = reader.GetManufacturers();
            Assert.Contains(manufacturers, manufacturer => manufacturer.Name == "Somfy");
            Assert.Equal("Somfy", reader.GetManufacturerName("78"));

            List<EibProduct> products = reader.GetProducts();
            Assert.Equal(2, products.Count);

            List<string> programIds = reader.GetProgramIdsForProduct("24356");
            Assert.Contains("24461", programIds);

            List<EibApplication> applications = reader.GetApplications(programIds);
            Assert.NotEmpty(applications);
            Assert.False(string.IsNullOrEmpty(applications[0].Name));

            reader.ApplyMaskData(applications[0]);
            Assert.False(string.IsNullOrEmpty(applications[0].MaskVersion));
            Assert.Contains(applications[0].ManufacturerIdProtected, new string[] { "true", "false" });

            List<EibCommunicationObject> communicationObjects = reader.GetCommunicationObjects("24461");
            Assert.NotEmpty(communicationObjects);
            List<EibCommunicationObject> normalized = EibProductDataReader.NormalizeCommunicationObjects(communicationObjects);
            Assert.True(normalized.Count <= communicationObjects.Count);
        }

        [Fact]
        public void Normalize_MergesObjectsWithSameNumberAndSortsThem()
        {
            List<EibCommunicationObject> objects = new List<EibCommunicationObject>
            {
                new EibCommunicationObject { Number = "2", Name = "B", Function = "f2", Type = "1" },
                new EibCommunicationObject { Number = "1", Name = "A", Function = "f1a", Type = "3" },
                new EibCommunicationObject { Number = "1", Name = "A", Function = "f1b", Type = "3" }
            };

            List<EibCommunicationObject> normalized = EibProductDataReader.NormalizeCommunicationObjects(objects);

            Assert.Equal(2, normalized.Count);
            Assert.Equal("1", normalized[0].Number);
            Assert.Equal("f1a * f1b", normalized[0].Function);
            Assert.Equal("2", normalized[1].Number);
        }

        [Fact]
        public void VdConverter_ConvertsSampleToReadableXml()
        {
            string output = Path.Combine(_tempDirectory, "sample.xml");

            new VdConverter().Convert(Path.Combine(AppContext.BaseDirectory, "TestData", "sample.vd_"), output);

            System.Xml.Linq.XDocument document = System.Xml.Linq.XDocument.Load(output);
            List<System.Xml.Linq.XElement> widgets = document.Root!.Elements("widget").ToList();
            Assert.Equal(2, widgets.Count);
            Assert.Equal("1", widgets[0].Element("ID")!.Value);
            Assert.Equal("Lamp & <Switch>", widgets[0].Element("NAME")!.Value);
            Assert.Equal("Valve", widgets[1].Element("NAME")!.Value);
        }

        [Fact]
        public void VdConverter_RejectsOtherFiles()
        {
            string input = Path.Combine(_tempDirectory, "wrong.vd_");
            File.WriteAllText(input, "something else\n");

            Assert.Throws<InvalidDataException>(() => new VdConverter().Convert(input, Path.Combine(_tempDirectory, "out.xml")));
        }
    }
}

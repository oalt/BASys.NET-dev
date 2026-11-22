using System.Xml.Linq;
using BASys.DataAccess;
using BASys.DataModels;

namespace BASys.Tests
{
    public class StorageTests : IDisposable
    {
        private readonly string _tempDirectory;

        public StorageTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "basys-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            Directory.Delete(_tempDirectory, true);
        }

        [Fact]
        public void LocalConfigurator_CreatesDirectoriesAndConfig()
        {
            LocalConfigurator configurator = new LocalConfigurator(Path.Combine(_tempDirectory, "basys"));
            Assert.False(configurator.IsValidConfig());

            configurator.CreateConfig("en_EN");

            Assert.True(configurator.IsValidConfig());
            Assert.True(Directory.Exists(configurator.ProjectsDirectory));
            Assert.True(Directory.Exists(configurator.DeviceDataDirectory));
            Assert.Equal("en_EN", configurator.GetLanguage());
            configurator.SetLanguage("de_DE");
            Assert.Equal("de_DE", configurator.GetLanguage());
        }

        [Fact]
        public void Project_SaveAndLoad_RoundTrip()
        {
            LocalConfigurator configurator = new LocalConfigurator(Path.Combine(_tempDirectory, "basys"));
            ProjectStorage storage = new ProjectStorage(configurator);
            Project project = new Project("Mein Büro €", "EIB");
            string buildingId = project.Architectural.AddChild(ArchitecturalDataModel.RootId, "Haus")!;
            string floorId = project.Architectural.AddChild(buildingId, "1. OG")!;
            string roomId = project.Architectural.AddChild(floorId, "Wohnzimmer")!;
            string boxId = project.Architectural.AddChild(roomId, "Hauptverteiler")!;

            string actuatorId = project.Installation.AddActuator("Aktor 1");
            project.Installation.SetProperty(actuatorId, "bussystem", "EIB");
            project.Installation.SetProperty(actuatorId, "device-state", "unprogrammed");
            project.Installation.SetProperty(actuatorId, "eib-physical-address", "1.1.2");
            string groupId = project.Installation.AddFunctionGroup(actuatorId);
            string functionId = project.Installation.AddFunction(groupId);
            project.Installation.SetName(functionId, "Ausgang A");
            project.Installation.WriteValue(project.Installation.GetDataRootElement(functionId)!, "type", "switching");
            project.Installation.WriteValue(project.Installation.GetDataRootElement(functionId)!, "state", "unused");
            project.Architectural.AddBusDevice(boxId, actuatorId);

            string lampId = project.Installation.AddPassiveComponent("Deckenlampe");
            project.Installation.SetProperty(lampId, "type", "1");
            project.Installation.AddConnection(lampId, groupId);
            project.Architectural.AddEndDevice(roomId, lampId, 1);

            storage.Save(project, null);

            string expectedDirectory = Path.Combine(configurator.ProjectsDirectory, "Mein_Buero_€");
            Assert.True(storage.IsProjectPath(expectedDirectory));
            Assert.Equal(expectedDirectory, project.ProjectPath);
            Assert.False(project.IsModified);

            Project loaded = storage.Load(expectedDirectory);
            Assert.Equal("Mein Büro €", loaded.Name);
            Assert.Equal("EIB", loaded.Architectural.PreferredBusSystem);
            Assert.Equal(new List<string> { buildingId }, loaded.Architectural.GetChildIds(ArchitecturalDataModel.RootId));
            Assert.Equal(new List<string> { floorId }, loaded.Architectural.GetChildIds(buildingId));
            Assert.Equal(new List<string> { roomId }, loaded.Architectural.GetChildIds(floorId));
            Assert.Equal(new List<string> { boxId }, loaded.Architectural.GetChildIds(roomId));
            Assert.Equal("Hauptverteiler", loaded.Architectural.GetInstallationLocationName(actuatorId));
            Assert.Equal(lampId, loaded.Architectural.GetEndDevices(roomId)[0].Key);
            Assert.Equal(new List<string> { actuatorId }, loaded.Installation.GetUnprogrammedEibDeviceIds());
            Assert.Equal(groupId, loaded.Installation.GetConnections(lampId)[0]);
            Assert.Equal(actuatorId, loaded.Installation.GetEibDeviceId(groupId));
            Assert.True(loaded.Installation.IsEibPhysicalAddressInUse("1.1.2"));
        }

        [Fact]
        public void Project_RemoveStructuralElement_RemovesInstallationComponents()
        {
            Project project = new Project("Test", "EIB");
            string buildingId = project.Architectural.AddChild(ArchitecturalDataModel.RootId, "Haus")!;
            string floorId = project.Architectural.AddChild(buildingId, "EG")!;
            string roomId = project.Architectural.AddChild(floorId, "Raum")!;
            string lampId = project.Installation.AddPassiveComponent("Lampe");
            project.Architectural.AddEndDevice(roomId, lampId, 1);

            project.RemoveStructuralElement(floorId);

            Assert.Empty(project.Architectural.GetChildIds(buildingId));
            Assert.Null(project.Installation.GetDataRootElement(lampId));
        }

        [Fact]
        public void GroupAddress_IsFormattedLikeJava()
        {
            Assert.Equal("0/257", EibAddress.FormatGroupAddress(1));
            Assert.Equal("0/300", EibAddress.FormatGroupAddress(44));
            string normalized;
            Assert.True(EibAddress.TryParsePhysicalAddress("1.1.2", out normalized));
            Assert.False(EibAddress.TryParsePhysicalAddress("16.1.2", out normalized));
        }

        [Fact]
        public void EibDevices_JavaFile_CanBeLoadedAndSaved()
        {
            string source = Path.Combine(AppContext.BaseDirectory, "TestData", "eib-devices.xml");
            EibDevicesDataModel model = new EibDevicesDataModel(XmlFile.Load(source));

            Assert.Equal(3, model.DeviceCount);
            string deviceId = model.GetDeviceIds()[0];
            Assert.Equal("ABB", model.GetManufacturerName(deviceId));
            Assert.Equal("REG", model.GetInstallationType(deviceId));
            Assert.NotEmpty(model.GetFunctionGroupIds(deviceId));
            Assert.Equal("17", model.GetApplication(deviceId).MaskVersion);
            Assert.Equal("Ausgang A", model.GetName(model.GetFunctionIds(model.GetFunctionGroupIds(deviceId)[0])[0]));

            List<string> matches = model.FindMatchingDevices("REG", new string[] { "switching" }, "actuator");
            Assert.Contains(deviceId, matches);

            string target = Path.Combine(_tempDirectory, "eib-devices.xml");
            XmlFile.Save(model.Document, target);
            EibDevicesDataModel reloaded = new EibDevicesDataModel(XmlFile.Load(target));
            Assert.Equal(model.DeviceCount, reloaded.DeviceCount);
            Assert.Equal(model.GetName(deviceId), reloaded.GetName(deviceId));
            Assert.Equal(model.GetApplication(deviceId).EepromData, reloaded.GetApplication(deviceId).EepromData);
        }

        [Fact]
        public void EibDevices_AddDevice_UsesNextFreeId()
        {
            string source = Path.Combine(AppContext.BaseDirectory, "TestData", "eib-devices.xml");
            EibDevicesDataModel model = new EibDevicesDataModel(XmlFile.Load(source));
            int count = model.DeviceCount;

            string newId = model.AddDevice("Neu");
            string groupId = model.AddFunctionGroup(newId);
            model.AddFunction(groupId, new EibDeviceFunction { Name = "Funktion", Type = "switching", ComObject = 3, EisType = 1, FunctionName = "Schalten" });

            Assert.Equal(count + 1, model.DeviceCount);
            Assert.NotEqual(model.GetDeviceIds()[0], newId);
            Assert.Equal("switching", XmlDataModel.ReadValue(model.GetDataRootElement(model.GetFunctionIds(groupId)[0]), "type"));
        }
    }
}

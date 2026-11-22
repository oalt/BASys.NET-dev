using System.Xml.Linq;
using BASys.DataModels;
using BASys.Services;
using BASys.ViewModels.Dialogs;

namespace BASys.ViewModels
{
    /// <summary>
    /// Adds an end device (lamp, valve, ...) to a room: the user selects an actuator of the project or of the EIB device
    /// database, names it, gives it a physical address, and the device is connected to a function group of the actuator
    /// (port of the Java command AddDeviceToRoomCommand).
    /// </summary>
    public class AddEndDeviceWorkflow
    {
        private readonly ILocalizationService _localizer;

        private readonly IDialogService _dialogs;

        private readonly IEibDataService _eibDataService;

        public AddEndDeviceWorkflow(ILocalizationService localizer, IDialogService dialogs, IEibDataService eibDataService)
        {
            _localizer = localizer;
            _dialogs = dialogs;
            _eibDataService = eibDataService;
        }

        /// <summary>
        /// Returns true if the device was added.
        /// </summary>
        public async Task<bool> ExecuteAsync(Project project, string roomId, EndDeviceType deviceType)
        {
            bool result = false;
            BusSystem preferredBusSystem = XmlCodes.ParseBusSystem(project.Architectural.PreferredBusSystem);
            NewEndDeviceDialogViewModel dialog = new NewEndDeviceDialogViewModel(deviceType, preferredBusSystem == BusSystem.None, preferredBusSystem, _localizer);

            if (await _dialogs.ShowAsync(dialog))
            {
                if (dialog.BusSystem == BusSystem.Eib)
                {
                    result = await AddEibDeviceAsync(project, roomId, deviceType, dialog);
                }
                else
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.busNotSupported"]);
                }
            }

            return result;
        }

        private async Task<bool> AddEibDeviceAsync(Project project, string roomId, EndDeviceType deviceType, NewEndDeviceDialogViewModel dialog)
        {
            bool result = false;
            string[] requiredFunctions = GetRequiredFunctions(deviceType);
            string locationCode = XmlCodes.ToCode(dialog.Location);
            string? installationLocationId = roomId;

            if (dialog.Location == InstallationLocation.DinRail)
            {
                installationLocationId = await SelectJunctionBoxAsync(project);
            }

            if (installationLocationId != null)
            {
                string? actuatorId = await SelectActuatorAsync(project, installationLocationId, locationCode, requiredFunctions);
                string? functionGroupId = actuatorId != null ? await SelectFunctionGroupAsync(project, actuatorId, requiredFunctions) : null;

                if (actuatorId != null && functionGroupId != null)
                {
                    InstallationModel installation = project.Installation;
                    string deviceId = installation.AddPassiveComponent(dialog.Name.Trim());
                    installation.SetProperty(deviceId, "bussystem", XmlCodes.ToCode(dialog.BusSystem));
                    installation.SetProperty(deviceId, "actor-installation-location", locationCode);
                    installation.SetProperty(deviceId, "type", ((int)deviceType).ToString());
                    installation.SetName(functionGroupId, ReadFirstFunctionName(installation, functionGroupId));
                    installation.SetGroupAddresses(requiredFunctions, functionGroupId);
                    installation.AddConnection(deviceId, functionGroupId);
                    project.Architectural.AddEndDevice(roomId, deviceId, (int)deviceType);

                    if (installation.GetProperty(actuatorId, "device-state") == "ready")
                    {
                        installation.SetProperty(actuatorId, "device-state", "addressed");
                    }

                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Selects the junction box in which a DIN rail device is installed. Returns null if there is none or the selection was cancelled.
        /// </summary>
        private async Task<string?> SelectJunctionBoxAsync(Project project)
        {
            string? result = null;
            List<KeyValuePair<string, string>> boxes = project.Architectural.GetJunctionBoxList();

            if (boxes.Count == 0)
            {
                await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noJunctionBoxesFonund"]);
            }
            else if (boxes.Count == 1)
            {
                result = boxes[0].Key;
            }
            else
            {
                List<KeyValuePair<string, string>> entries = boxes.Select(currentBox => new KeyValuePair<string, string>(currentBox.Value, currentBox.Key)).ToList();
                result = await _dialogs.ShowSelectionAsync(_localizer["tit.selection"], entries);
            }

            return result;
        }

        /// <summary>
        /// Selects an actuator that is already installed at the location or adds a new one from the device database.
        /// Returns the id of the actuator or null if nothing was selected.
        /// </summary>
        private async Task<string?> SelectActuatorAsync(Project project, string installationLocationId, string locationCode, string[] requiredFunctions)
        {
            string? result = null;
            List<string> existingActuators = project.Installation.FindMatchingDevices(locationCode, requiredFunctions, "actuator",
                project.Architectural.GetBusDeviceIds(installationLocationId, true));
            bool takeNew = true;

            if (existingActuators.Count > 0)
            {
                takeNew = !await _dialogs.ShowQuestionAsync(_localizer["tit.question"], _localizer["mess.devInProjectFound"]);
            }

            if (takeNew)
            {
                result = await AddNewActuatorAsync(project, installationLocationId, locationCode, requiredFunctions);
            }
            else
            {
                List<KeyValuePair<string, string>> entries = existingActuators
                    .Select(currentId => new KeyValuePair<string, string>(project.Installation.GetName(currentId) + " - " + project.Installation.GetProperty(currentId, "device-name"), currentId))
                    .ToList();
                result = entries.Count == 1 ? entries[0].Value : await _dialogs.ShowSelectionAsync(_localizer["tit.selDevice"], entries);
            }

            return result;
        }

        private async Task<string?> AddNewActuatorAsync(Project project, string installationLocationId, string locationCode, string[] requiredFunctions)
        {
            string? result = null;
            EibDevicesDataModel deviceModel = _eibDataService.Devices;
            List<string> deviceIds = deviceModel.FindMatchingDevices(locationCode, requiredFunctions, "actuator");

            if (deviceIds.Count == 0)
            {
                await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noDeviceFound"]);
            }
            else
            {
                List<KeyValuePair<string, string>> entries = deviceIds
                    .Select(currentId => new KeyValuePair<string, string>(deviceModel.GetManufacturerName(currentId) + " - " + deviceModel.GetName(currentId), currentId))
                    .ToList();
                string? deviceId = await _dialogs.ShowSelectionAsync(_localizer["tit.selDevice"], entries);
                string? name = deviceId != null ? await _dialogs.ShowPromptAsync(_localizer["tit.input"], _localizer["mess.inputname"], _localizer["noname"]) : null;
                string? physicalAddress = name != null ? await AskPhysicalAddressAsync(project) : null;

                if (deviceId != null && name != null && physicalAddress != null)
                {
                    result = CreateActuator(project, deviceModel, deviceId, name.Length > 0 ? name : _localizer["noname"], physicalAddress, locationCode);
                    project.Architectural.AddBusDevice(installationLocationId, result);
                }
            }

            return result;
        }

        private async Task<string?> AskPhysicalAddressAsync(Project project)
        {
            string? result = null;
            bool done = false;

            while (!done)
            {
                string? text = await _dialogs.ShowPromptAsync(_localizer["tit.input"], _localizer["mess.inputPhAddr"], string.Empty);
                string normalized = string.Empty;

                if (text == null)
                {
                    done = true;
                }
                else if (!EibAddress.TryParsePhysicalAddress(text, out normalized))
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.wrongPhAddrFormat"]);
                }
                else if (project.Installation.IsEibPhysicalAddressInUse(normalized))
                {
                    await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.phAddrInUse"]);
                }
                else
                {
                    result = normalized;
                    done = true;
                }
            }

            return result;
        }

        private static string CreateActuator(Project project, EibDevicesDataModel deviceModel, string deviceId, string name, string physicalAddress, string locationCode)
        {
            InstallationModel installation = project.Installation;
            string result = installation.AddActuator(name);
            installation.SetProperty(result, "device-name", deviceModel.GetName(deviceId));
            installation.SetProperty(result, "device-id", deviceId);
            installation.SetProperty(result, "device-state", "unprogrammed");
            installation.SetProperty(result, "manufacturer", deviceModel.GetManufacturerName(deviceId));
            installation.SetProperty(result, "bussystem", "EIB");
            installation.SetProperty(result, "installation-location", locationCode);
            installation.SetProperty(result, "eib-physical-address", physicalAddress);

            foreach (string currentDeviceGroupId in deviceModel.GetFunctionGroupIds(deviceId))
            {
                string groupId = installation.AddFunctionGroup(result);

                foreach (string currentDeviceFunctionId in deviceModel.GetFunctionIds(currentDeviceGroupId))
                {
                    string functionId = installation.AddFunction(groupId);
                    XElement source = deviceModel.GetDataRootElement(currentDeviceFunctionId)!;
                    XElement destination = installation.GetDataRootElement(functionId)!;
                    installation.SetName(functionId, deviceModel.GetName(currentDeviceFunctionId));
                    installation.WriteValue(destination, "type", XmlDataModel.ReadValue(source, "type"));
                    installation.WriteValue(destination, "eis-type", XmlDataModel.ReadValue(source, "eis-type"));
                    installation.WriteValue(destination, "com-object", XmlDataModel.ReadValue(source, "com-object"));
                    installation.WriteValue(destination, "devmodelid", currentDeviceFunctionId);
                    installation.WriteValue(destination, "state", "unused");
                }
            }

            return result;
        }

        /// <summary>
        /// Selects the function group of the actuator that is used for the device.
        /// </summary>
        private async Task<string?> SelectFunctionGroupAsync(Project project, string actuatorId, string[] requiredFunctions)
        {
            string? result = null;
            List<string> groupIds = project.Installation.GetMatchingFunctionGroups(actuatorId, requiredFunctions);

            if (groupIds.Count == 1)
            {
                result = groupIds[0];
            }
            else if (groupIds.Count > 1)
            {
                List<KeyValuePair<string, string>> entries = groupIds
                    .Select(currentId => new KeyValuePair<string, string>(ReadFirstFunctionName(project.Installation, currentId), currentId))
                    .ToList();
                result = await _dialogs.ShowSelectionAsync(_localizer["tit.selDevice"], entries);
            }
            else
            {
                await _dialogs.ShowMessageAsync(_localizer["tit.info"], _localizer["mess.noDeviceFound"]);
            }

            return result;
        }

        private static string ReadFirstFunctionName(InstallationModel installation, string functionGroupId)
        {
            string result = XmlDataModel.ReadValue(installation.GetDataRootElement(functionGroupId), "function/name");

            return result;
        }

        /// <summary>
        /// Function types an actuator needs for the given end device type.
        /// </summary>
        private static string[] GetRequiredFunctions(EndDeviceType deviceType)
        {
            string[] result = new string[] { string.Empty };

            switch (deviceType)
            {
                case EndDeviceType.Lamp:
                    result = new string[] { "switching" };
                    break;
                case EndDeviceType.DimmableLamp:
                    result = new string[] { "switching", "relative dimming", "dimming value" };
                    break;
                case EndDeviceType.Valve:
                    result = new string[] { "value" };
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}

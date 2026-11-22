using System.Collections.ObjectModel;
using BASys.DataModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels
{
    /// <summary>
    /// View model of a node of the architectural tree. For rooms it also exposes the room content
    /// (installed devices, junction boxes and bus devices) as read from the project.
    /// </summary>
    public class ArchitecturalNodeViewModel : ObservableObject
    {
        private readonly Project _project;

        private readonly string _id;

        private readonly ArchitecturalNodeViewModel? _parent;

        private readonly ObservableCollection<ArchitecturalNodeViewModel> _children = new ObservableCollection<ArchitecturalNodeViewModel>();

        private readonly ObservableCollection<InstallationDeviceRow> _installationDevices = new ObservableCollection<InstallationDeviceRow>();

        private readonly ObservableCollection<JunctionBoxRow> _junctionBoxes = new ObservableCollection<JunctionBoxRow>();

        private readonly ObservableCollection<BusDeviceRow> _busDevices = new ObservableCollection<BusDeviceRow>();

        public ArchitecturalNodeViewModel(Project project, string id, ArchitecturalNodeViewModel? parent)
        {
            _project = project;
            _id = id;
            _parent = parent;

            if (NodeType != ArchitecturalNodeType.Room)
            {
                foreach (string currentChildId in project.Architectural.GetChildIds(id))
                {
                    _children.Add(new ArchitecturalNodeViewModel(project, currentChildId, this));
                }
            }
            else
            {
                RefreshContent();
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public ArchitecturalNodeViewModel? Parent
        {
            get
            {
                return _parent;
            }
        }

        public string Name
        {
            get
            {
                return _project.Architectural.GetName(_id);
            }
            set
            {
                if (_project.Architectural.GetName(_id) != value)
                {
                    _project.Architectural.SetName(_id, value);
                    OnPropertyChanged();
                }
            }
        }

        public ArchitecturalNodeType NodeType
        {
            get
            {
                ArchitecturalNodeType result = ArchitecturalNodeType.Room;

                if (_id.StartsWith(ArchitecturalDataModel.ProjectPrefix, StringComparison.Ordinal))
                {
                    result = ArchitecturalNodeType.Project;
                }
                else if (_id.StartsWith(ArchitecturalDataModel.BuildingPrefix, StringComparison.Ordinal))
                {
                    result = ArchitecturalNodeType.Building;
                }
                else if (_id.StartsWith(ArchitecturalDataModel.FloorPrefix, StringComparison.Ordinal))
                {
                    result = ArchitecturalNodeType.Floor;
                }

                return result;
            }
        }

        public string IconName
        {
            get
            {
                string result = "folder2-open";

                switch (NodeType)
                {
                    case ArchitecturalNodeType.Building:
                        result = "building";
                        break;
                    case ArchitecturalNodeType.Floor:
                        result = "layers";
                        break;
                    case ArchitecturalNodeType.Room:
                        result = "door-open";
                        break;
                    default:
                        break;
                }

                return result;
            }
        }

        public ObservableCollection<ArchitecturalNodeViewModel> Children
        {
            get
            {
                return _children;
            }
        }

        public ObservableCollection<InstallationDeviceRow> InstallationDevices
        {
            get
            {
                return _installationDevices;
            }
        }

        public ObservableCollection<JunctionBoxRow> JunctionBoxes
        {
            get
            {
                return _junctionBoxes;
            }
        }

        public ObservableCollection<BusDeviceRow> BusDevices
        {
            get
            {
                return _busDevices;
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                SetProperty(ref _isExpanded, value);
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        /// <summary>
        /// Adds a view model for a child that was added to the project model.
        /// </summary>
        public ArchitecturalNodeViewModel AddChildNode(string childId)
        {
            ArchitecturalNodeViewModel result = new ArchitecturalNodeViewModel(_project, childId, this);
            _children.Add(result);

            return result;
        }

        public void RemoveChildNode(ArchitecturalNodeViewModel child)
        {
            _children.Remove(child);
        }

        /// <summary>
        /// Reads the room content again from the project (only for rooms).
        /// </summary>
        public void RefreshContent()
        {
            if (NodeType == ArchitecturalNodeType.Room)
            {
                _installationDevices.Clear();
                _junctionBoxes.Clear();
                _busDevices.Clear();

                foreach (KeyValuePair<string, int> currentDevice in _project.Architectural.GetEndDevices(_id))
                {
                    _installationDevices.Add(CreateInstallationDeviceRow(currentDevice.Key, currentDevice.Value));
                }

                foreach (string currentBoxId in _project.Architectural.GetChildIds(_id))
                {
                    _junctionBoxes.Add(new JunctionBoxRow { Id = currentBoxId, Name = _project.Architectural.GetName(currentBoxId) });
                }

                foreach (string currentBusDeviceId in _project.Architectural.GetBusDeviceIds(_id, true))
                {
                    _busDevices.Add(CreateBusDeviceRow(currentBusDeviceId));
                }
            }
        }

        private InstallationDeviceRow CreateInstallationDeviceRow(string deviceId, int deviceTypeCode)
        {
            InstallationModel installation = _project.Installation;
            InstallationDeviceRow result = new InstallationDeviceRow
            {
                Id = deviceId,
                Name = installation.GetName(deviceId),
                DeviceType = XmlCodes.ParseEndDeviceType(deviceTypeCode),
                BusSystem = XmlCodes.ParseBusSystem(installation.GetProperty(deviceId, "bussystem"))
            };

            if (result.BusSystem == BusSystem.Eib)
            {
                foreach (string currentConnection in installation.GetConnections(deviceId))
                {
                    if (currentConnection.StartsWith("functiongroup", StringComparison.Ordinal))
                    {
                        string actuatorId = installation.GetEibDeviceId(currentConnection);
                        result.AssignedActuator = installation.GetName(actuatorId);
                        result.InstallationLocation = _project.Architectural.GetInstallationLocationName(actuatorId);
                        result.Manufacturer = installation.GetProperty(actuatorId, "manufacturer");
                        result.ActuatorType = installation.GetProperty(actuatorId, "device-name");
                        result.FunctionGroupName = installation.GetName(currentConnection);
                        result.Addresses = string.Join(" ", installation.GetGroupAddresses(currentConnection));
                        break;
                    }
                }
            }

            return result;
        }

        private BusDeviceRow CreateBusDeviceRow(string busDeviceId)
        {
            InstallationModel installation = _project.Installation;
            BusSystem busSystem = XmlCodes.ParseBusSystem(installation.GetProperty(busDeviceId, "bussystem"));
            BusDeviceRow result = new BusDeviceRow
            {
                Id = busDeviceId,
                Name = installation.GetName(busDeviceId),
                Manufacturer = installation.GetProperty(busDeviceId, "manufacturer"),
                DeviceName = installation.GetProperty(busDeviceId, "device-name"),
                BusSystem = busSystem,
                PhysicalAddress = busSystem == BusSystem.Eib ? installation.GetProperty(busDeviceId, "eib-physical-address") : string.Empty
            };

            return result;
        }
    }
}

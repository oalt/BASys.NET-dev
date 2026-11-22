namespace BASys.DataModels
{
    /// <summary>
    /// A BASys project: architectural structure and installation components. It is stored as the two
    /// files structure.xml and installation.xml in a project directory.
    /// </summary>
    public class Project
    {
        private readonly ArchitecturalDataModel _architecturalModel;

        private readonly InstallationModel _installationModel;

        public Project(string projectName, string preferredBusSystem)
        {
            _architecturalModel = new ArchitecturalDataModel(projectName);
            _architecturalModel.PreferredBusSystem = preferredBusSystem;
            _installationModel = new InstallationModel();
            Attach();
        }

        public Project(ArchitecturalDataModel architecturalModel, InstallationModel installationModel, string projectPath)
        {
            _architecturalModel = architecturalModel;
            _installationModel = installationModel;
            _projectPath = projectPath;
            Attach();
        }

        /// <summary>
        /// Raised when the project data were changed.
        /// </summary>
        public event EventHandler? Modified;

        public ArchitecturalDataModel Architectural
        {
            get
            {
                return _architecturalModel;
            }
        }

        public InstallationModel Installation
        {
            get
            {
                return _installationModel;
            }
        }

        public string Name
        {
            get
            {
                return _architecturalModel.ProjectName;
            }
        }

        private string? _projectPath;

        /// <summary>
        /// Directory the project was loaded from or saved to (null for a project that was never saved).
        /// </summary>
        public string? ProjectPath
        {
            get
            {
                return _projectPath;
            }
            set
            {
                _projectPath = value;
            }
        }

        private bool _isModified;

        public bool IsModified
        {
            get
            {
                return _isModified;
            }
            set
            {
                _isModified = value;
            }
        }

        /// <summary>
        /// Removes a structural element with all end devices and bus devices located below it.
        /// </summary>
        public void RemoveStructuralElement(string id)
        {
            List<string> endDeviceIds = new List<string>();
            List<string> busDeviceIds = new List<string>();
            _architecturalModel.CollectReferencedDevices(id, endDeviceIds, busDeviceIds);
            _architecturalModel.RemoveData(id);

            foreach (string currentId in endDeviceIds.Concat(busDeviceIds))
            {
                _installationModel.RemoveComponent(currentId);
            }
        }

        private void Attach()
        {
            _architecturalModel.Changed += OnModelChanged;
            _installationModel.Changed += OnModelChanged;
        }

        private void OnModelChanged(object? sender, EventArgs e)
        {
            _isModified = true;
            Modified?.Invoke(this, EventArgs.Empty);
        }
    }
}

using BASys.DataAccess;
using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Project service that stores projects as structure.xml and installation.xml (format of the Java application).
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly LocalConfigurator _configurator;

        private readonly ProjectStorage _storage;

        public ProjectService(LocalConfigurator configurator)
        {
            _configurator = configurator;
            _storage = new ProjectStorage(configurator);
        }

        public string ProjectsDirectory
        {
            get
            {
                return _configurator.ProjectsDirectory;
            }
        }

        public Project CreateProject(string name, BusSystem preferredBusSystem)
        {
            Project result = new Project(name, XmlCodes.ToCode(preferredBusSystem));
            result.IsModified = true;

            return result;
        }

        public bool IsProjectPath(string directory)
        {
            bool result = _storage.IsProjectPath(directory);

            return result;
        }

        public Project OpenProject(string directory)
        {
            Project result = _storage.Load(directory);

            return result;
        }

        public void SaveProject(Project project, string? targetDirectory)
        {
            _storage.Save(project, targetDirectory);
        }
    }
}

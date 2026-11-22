using BASys.DataModels;

namespace BASys.DataAccess
{
    /// <summary>
    /// Loads and saves projects. A project is a directory with the files structure.xml and installation.xml.
    /// </summary>
    public class ProjectStorage
    {
        public const string StructureFileName = "structure.xml";

        public const string InstallationFileName = "installation.xml";

        private readonly LocalConfigurator _configurator;

        public ProjectStorage(LocalConfigurator configurator)
        {
            _configurator = configurator;
        }

        public bool IsProjectPath(string directory)
        {
            bool result = File.Exists(Path.Combine(directory, StructureFileName)) && File.Exists(Path.Combine(directory, InstallationFileName));

            return result;
        }

        public Project Load(string directory)
        {
            ArchitecturalDataModel architecturalModel = new ArchitecturalDataModel(XmlFile.Load(Path.Combine(directory, StructureFileName)));
            InstallationModel installationModel = new InstallationModel(XmlFile.Load(Path.Combine(directory, InstallationFileName)));
            Project result = new Project(architecturalModel, installationModel, directory);

            return result;
        }

        /// <summary>
        /// Saves the project. Without a target directory the project directory is used or, for a new project,
        /// a directory below the projects directory is created.
        /// </summary>
        public void Save(Project project, string? targetDirectory)
        {
            string directory = targetDirectory ?? project.ProjectPath ?? Path.Combine(_configurator.ProjectsDirectory, project.Architectural.ProjectDirectoryName);
            Directory.CreateDirectory(directory);
            XmlFile.Save(project.Architectural.Document, Path.Combine(directory, StructureFileName));
            XmlFile.Save(project.Installation.Document, Path.Combine(directory, InstallationFileName));
            project.ProjectPath = directory;
            project.IsModified = false;
        }
    }
}

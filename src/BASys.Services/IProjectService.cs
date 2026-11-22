using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Creates, loads and saves projects.
    /// </summary>
    public interface IProjectService
    {
        string ProjectsDirectory { get; }

        Project CreateProject(string name, BusSystem preferredBusSystem);

        bool IsProjectPath(string directory);

        Project OpenProject(string directory);

        /// <summary>
        /// Saves the project to its directory or, if a target directory is given, to that directory.
        /// </summary>
        void SaveProject(Project project, string? targetDirectory);
    }
}

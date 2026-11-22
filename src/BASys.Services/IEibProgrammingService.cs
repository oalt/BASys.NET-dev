using BASys.DataModels;

namespace BASys.Services
{
    /// <summary>
    /// Programs EIB devices over the bus.
    /// </summary>
    public interface IEibProgrammingService
    {
        /// <summary>
        /// Programs the physical address and optionally the application of a device and updates its "device-state".
        /// </summary>
        Task ProgramDeviceAsync(Project project, string deviceId, bool addressOnly, IProgress<int> progress, CancellationToken cancellationToken);
    }
}

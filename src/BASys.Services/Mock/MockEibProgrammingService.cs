using BASys.DataModels;

namespace BASys.Services.Mock
{
    /// <summary>
    /// Simulates the programming of EIB devices (the bus connection is still a mockup).
    /// </summary>
    public class MockEibProgrammingService : IEibProgrammingService
    {
        private const int StepCount = 10;

        public async Task ProgramDeviceAsync(Project project, string deviceId, bool addressOnly, IProgress<int> progress, CancellationToken cancellationToken)
        {
            for (int step = 1; step <= StepCount; step++)
            {
                await Task.Delay(150, cancellationToken);
                progress.Report(step * 100 / StepCount);
            }

            project.Installation.SetProperty(deviceId, "device-state", addressOnly ? "addressed" : "ready");
        }
    }
}

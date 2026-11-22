using BASys.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BASys.Views
{
    /// <summary>
    /// Error boundary that writes unhandled exceptions of the user interface to the error log.
    /// </summary>
    public class LoggingErrorBoundary : ErrorBoundary
    {
        [Inject]
        private IErrorLogService ErrorLog { get; set; } = default!;

        protected override Task OnErrorAsync(Exception exception)
        {
            ErrorLog.Write("UI", exception);

            return Task.CompletedTask;
        }
    }
}

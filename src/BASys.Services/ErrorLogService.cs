using System.Diagnostics;
using System.Text;
using BASys.DataAccess;

namespace BASys.Services
{
    /// <summary>
    /// Error log that appends to basys-errors.log in the local data directory and to the debugger output.
    /// </summary>
    public class ErrorLogService : IErrorLogService
    {
        private readonly object _lock = new object();

        private readonly string _logFileName;

        public ErrorLogService(LocalConfigurator configurator)
        {
            _logFileName = Path.Combine(configurator.BaseDirectory, "basys-errors.log");
        }

        public string LogFileName
        {
            get
            {
                return _logFileName;
            }
        }

        public void Write(string source, Exception exception)
        {
            string text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + source + "] " + exception + Environment.NewLine;
            Debug.WriteLine(text);

            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFileName)!);
                    File.AppendAllText(_logFileName, text, Encoding.UTF8);
                }
            }
            catch (Exception logException) when (logException is IOException || logException is UnauthorizedAccessException)
            {
                Debug.WriteLine(logException);
            }
        }
    }
}

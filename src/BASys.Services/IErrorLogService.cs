namespace BASys.Services
{
    /// <summary>
    /// Writes unexpected errors to the log file basys-errors.log in the local data directory.
    /// </summary>
    public interface IErrorLogService
    {
        string LogFileName { get; }

        void Write(string source, Exception exception);
    }
}

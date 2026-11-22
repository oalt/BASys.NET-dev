using BASys.ViewModels;

namespace BASys.Views
{
    /// <summary>
    /// Describes where and for which node a context menu is requested.
    /// </summary>
    public class ContextMenuRequest
    {
        public ContextMenuRequest(double x, double y, ArchitecturalNodeViewModel? target)
        {
            X = x;
            Y = y;
            Target = target;
        }

        public double X { get; }

        public double Y { get; }

        public ArchitecturalNodeViewModel? Target { get; }
    }
}

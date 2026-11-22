using BASys.DataAccess;
using BASys.Services;
using BASys.Services.Mock;
using BASys.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BASys.Views
{
    /// <summary>
    /// Registers all services, view models and helpers needed by the components.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBASys(this IServiceCollection services)
        {
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<LocalConfigurator>(CreateConfigurator);
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IErrorLogService, ErrorLogService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IEibDataService, EibDataService>();
            services.AddSingleton<IEibProgrammingService, MockEibProgrammingService>();
            services.AddSingleton<IEibBusService, EibBusService>();
            services.AddSingleton<DialogHostViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DragDropState>();

            return services;
        }

        /// <summary>
        /// Creates the configurator. The environment variable BASYS_DATA_DIR overrides the data directory (user profile/basys).
        /// </summary>
        private static LocalConfigurator CreateConfigurator(IServiceProvider serviceProvider)
        {
            string? dataDirectory = Environment.GetEnvironmentVariable("BASYS_DATA_DIR");
            LocalConfigurator result = string.IsNullOrWhiteSpace(dataDirectory) ? new LocalConfigurator() : new LocalConfigurator(dataDirectory);

            return result;
        }
    }
}

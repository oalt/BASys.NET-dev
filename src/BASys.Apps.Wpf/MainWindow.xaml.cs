using System.ComponentModel;
using System.Windows;
using BASys.Views;
using BASys.Services;
using BASys.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BASys.Apps.Wpf
{
    /// <summary>
    /// Main window. It only hosts the Blazor user interface.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;

        private readonly IErrorLogService _errorLog;

        private readonly ServiceProvider _serviceProvider;

        public MainWindow()
        {
            InitializeComponent();

            ServiceCollection services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            services.AddBASys();
            services.AddSingleton<IFileDialogService, WpfFileDialogService>();
            services.AddSingleton<WpfToolWindowService>();
            services.AddSingleton<IToolWindowService>(provider => provider.GetRequiredService<WpfToolWindowService>());

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _serviceProvider = serviceProvider;
            BlazorWebView.Services = serviceProvider;

            _mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
            _mainViewModel.ExitRequested += OnExitRequested;
            _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            Title = _mainViewModel.Title;
            Activated += OnWindowActivated;
            Closed += OnWindowClosed;

            _errorLog = serviceProvider.GetRequiredService<IErrorLogService>();
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            if (Environment.GetEnvironmentVariable("BASYS_LOG_FIRST_CHANCE") == "1")
            {
                AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            }
        }

        private void OnFirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            _errorLog.Write("FirstChance", e.Exception);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _errorLog.Write("WPF", e.Exception);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _errorLog.Write("Task", e.Exception);
        }

        /// <summary>
        /// Gives the keyboard focus to the web view whenever the window is activated so that the first click is handled by the page.
        /// </summary>
        private void OnWindowActivated(object? sender, EventArgs e)
        {
            BlazorWebView.WebView?.Focus();
        }

        /// <summary>
        /// Disposes the services on exit so that an open EIB tunnel is closed and the gateway frees its connection slot.
        /// </summary>
        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _serviceProvider.GetRequiredService<WpfToolWindowService>().CloseAll();
            _serviceProvider.GetRequiredService<IEibBusService>().Disconnect();
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Title))
            {
                Title = _mainViewModel.Title;
            }
        }
    }
}

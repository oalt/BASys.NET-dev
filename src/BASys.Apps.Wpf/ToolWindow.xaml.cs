using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using BASys.Services;
using BASys.ViewModels.Dialogs;
using BASys.Views;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace BASys.Apps.Wpf
{
    /// <summary>
    /// A window that hosts one dialog (bus monitor, telegram dialog) as an independent, non-modal window.
    /// </summary>
    public partial class ToolWindow : Window
    {
        private readonly DialogViewModelBase _dialog;

        private const int StartupCheckMilliseconds = 8000;

        private readonly IErrorLogService _errorLog;

        private int _startupChecks;

        private bool _isClosing;

        public ToolWindow(IServiceProvider services, DialogViewModelBase dialog)
        {
            InitializeComponent();

            _dialog = dialog;
            Title = dialog.Title;

            if (dialog.PreferredWidth > 0)
            {
                Width = dialog.PreferredWidth;
            }

            if (dialog.PreferredHeight > 0)
            {
                Height = dialog.PreferredHeight;
            }

            BlazorWebView.Services = services;
            RootComponent root = new RootComponent();
            root.Selector = "#app";
            root.ComponentType = typeof(ToolWindowRoot);
            root.Parameters = new Dictionary<string, object?> { { nameof(ToolWindowRoot.Dialog), dialog } };
            BlazorWebView.RootComponents.Add(root);

            _errorLog = services.GetRequiredService<IErrorLogService>();
            Loaded += OnWindowLoaded;
            dialog.PropertyChanged += OnDialogPropertyChanged;
            dialog.CloseRequested += OnDialogCloseRequested;
            Activated += OnWindowActivated;
            Closing += OnWindowClosing;
        }

        /// <summary>
        /// Starts the startup check of the page.
        /// </summary>
        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            await CheckStartedAsync();
        }

        /// <summary>
        /// Safety net: if the Blazor page did not render its content within a few seconds (the web view stays at "Loading..."),
        /// the problem is logged and the page is reloaded once.
        /// </summary>
        private async Task CheckStartedAsync()
        {
            try
            {
                await Task.Delay(StartupCheckMilliseconds);

                if (!_isClosing)
                {
                    Microsoft.Web.WebView2.Core.CoreWebView2? core = BlazorWebView.WebView?.CoreWebView2;

                    if (core == null)
                    {
                        _errorLog.Write("ToolWindow", new InvalidOperationException("The web view of '" + _dialog.Title + "' was not initialized."));
                    }
                    else if (await core.ExecuteScriptAsync("document.querySelector('.tool-window') !== null") != "true")
                    {
                        _errorLog.Write("ToolWindow", new InvalidOperationException("The page of '" + _dialog.Title + "' did not start (check " + (_startupChecks + 1) + ")."));

                        if (_startupChecks == 0)
                        {
                            _startupChecks++;
                            core.Reload();
                            await CheckStartedAsync();
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is System.Runtime.InteropServices.COMException || exception is ObjectDisposedException)
            {
                // the window was closed in the meantime
                _errorLog.Write("ToolWindow", exception);
            }
        }

        private void OnWindowActivated(object? sender, EventArgs e)
        {
            BlazorWebView.WebView?.Focus();
        }

        private void OnDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DialogViewModelBase.Title))
            {
                Dispatcher.Invoke(() => Title = _dialog.Title);
            }
        }

        private void OnDialogCloseRequested(object? sender, bool dialogResult)
        {
            Dispatcher.InvokeAsync(CloseAfterEventAsync);
        }

        /// <summary>
        /// The dialog usually asks for closing from within a click handler of its own page. Disposing the web view at that
        /// point makes Blazor fail when it sends the result of the event to the page. So the window is hidden at once and
        /// closed a moment later, when the event has been completed.
        /// </summary>
        private async Task CloseAfterEventAsync()
        {
            Hide();
            await Task.Delay(300);
            if (!_isClosing)
            {
                Close();
            }
        }

        /// <summary>
        /// The dialog must stop its work (bus monitoring) before the web view is disposed, otherwise late updates
        /// are rendered into a disposed web view and raise exceptions.
        /// </summary>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _isClosing = true;
            _dialog.PropertyChanged -= OnDialogPropertyChanged;
            _dialog.CloseRequested -= OnDialogCloseRequested;
            _dialog.CloseFromView();
        }
    }
}

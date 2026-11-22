using System.ComponentModel;
using System.Windows;
using BASys.ViewModels.Dialogs;
using BASys.Views;
using Microsoft.AspNetCore.Components.WebView.Wpf;

namespace BASys.Apps.Wpf
{
    /// <summary>
    /// A window that hosts one dialog (bus monitor, telegram dialog) as an independent, non-modal window.
    /// </summary>
    public partial class ToolWindow : Window
    {
        private readonly DialogViewModelBase _dialog;

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

            dialog.PropertyChanged += OnDialogPropertyChanged;
            dialog.CloseRequested += OnDialogCloseRequested;
            Activated += OnWindowActivated;
            Closing += OnWindowClosing;
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
            Dispatcher.Invoke(Close);
        }

        /// <summary>
        /// The dialog must stop its work (bus monitoring) before the web view is disposed, otherwise late updates
        /// are rendered into a disposed web view and raise exceptions.
        /// </summary>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _dialog.PropertyChanged -= OnDialogPropertyChanged;
            _dialog.CloseRequested -= OnDialogCloseRequested;
            _dialog.CloseFromView();
        }
    }
}

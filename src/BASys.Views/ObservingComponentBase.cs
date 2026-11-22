using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using BASys.Services;
using Microsoft.AspNetCore.Components;

namespace BASys.Views
{
    /// <summary>
    /// Base class for components that re-render when observed view models, collections or commands change.
    /// </summary>
    public abstract class ObservingComponentBase : ComponentBase, IDisposable
    {
        private readonly List<object> _observedSources = new List<object>();

        [Inject]
        protected ILocalizationService Localizer { get; set; } = default!;

        protected override void OnInitialized()
        {
            Localizer.CultureChanged += OnSourceChanged;
            base.OnInitialized();
        }

        /// <summary>
        /// Returns the localized text for the given key.
        /// </summary>
        protected string T(string key)
        {
            string result = Localizer[key];

            return result;
        }

        /// <summary>
        /// Starts observing the given object (view model, observable collection or command).
        /// </summary>
        protected void Observe(object? source)
        {
            if (source != null && !_observedSources.Contains(source))
            {
                _observedSources.Add(source);

                INotifyPropertyChanged? propertySource = source as INotifyPropertyChanged;
                INotifyCollectionChanged? collectionSource = source as INotifyCollectionChanged;
                ICommand? commandSource = source as ICommand;

                if (propertySource != null)
                {
                    propertySource.PropertyChanged += OnPropertyChanged;
                }

                if (collectionSource != null)
                {
                    collectionSource.CollectionChanged += OnCollectionChanged;
                }

                if (commandSource != null)
                {
                    commandSource.CanExecuteChanged += OnSourceChanged;
                }
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        private void OnSourceChanged(object? sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            InvokeAsync(StateHasChanged);
        }

        public virtual void Dispose()
        {
            Localizer.CultureChanged -= OnSourceChanged;

            foreach (object currentSource in _observedSources)
            {
                INotifyPropertyChanged? propertySource = currentSource as INotifyPropertyChanged;
                INotifyCollectionChanged? collectionSource = currentSource as INotifyCollectionChanged;
                ICommand? commandSource = currentSource as ICommand;

                if (propertySource != null)
                {
                    propertySource.PropertyChanged -= OnPropertyChanged;
                }

                if (collectionSource != null)
                {
                    collectionSource.CollectionChanged -= OnCollectionChanged;
                }

                if (commandSource != null)
                {
                    commandSource.CanExecuteChanged -= OnSourceChanged;
                }
            }

            _observedSources.Clear();
            GC.SuppressFinalize(this);
        }
    }
}

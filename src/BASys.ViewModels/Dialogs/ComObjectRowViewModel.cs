using BASys.DataModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels.Dialogs
{
    /// <summary>
    /// Row of the table of communication objects of an application: the object with the function type chosen by the user.
    /// </summary>
    public class ComObjectRowViewModel : ObservableObject
    {
        public ComObjectRowViewModel(EibCommunicationObject communicationObject)
        {
            CommunicationObject = communicationObject;
        }

        public EibCommunicationObject CommunicationObject { get; }

        private string _functionType = "unused";

        public string FunctionType
        {
            get
            {
                return _functionType;
            }
            set
            {
                SetProperty(ref _functionType, value);
            }
        }

        private List<string> _relatedNumbers = new List<string>();

        /// <summary>
        /// Numbers of the communication objects that form a function group together with this one.
        /// </summary>
        public List<string> RelatedNumbers
        {
            get
            {
                return _relatedNumbers;
            }
            set
            {
                if (SetProperty(ref _relatedNumbers, value))
                {
                    OnPropertyChanged(nameof(RelatedFunctions));
                }
            }
        }

        public string RelatedFunctions
        {
            get
            {
                string result = string.Join(", ", _relatedNumbers);

                return result;
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }
    }
}

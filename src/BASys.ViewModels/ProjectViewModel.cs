using BASys.DataModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BASys.ViewModels
{
    /// <summary>
    /// View model of an opened project including the architectural tree.
    /// </summary>
    public class ProjectViewModel : ObservableObject
    {
        private readonly Project _model;

        private readonly ArchitecturalNodeViewModel _rootNode;

        public ProjectViewModel(Project model)
        {
            _model = model;
            _rootNode = new ArchitecturalNodeViewModel(model, ArchitecturalDataModel.RootId, null);
            ExpandAll(_rootNode);
            _model.Modified += OnModelModified;
        }

        /// <summary>
        /// Raised when the content of the project was changed.
        /// </summary>
        public event EventHandler? Modified;

        public Project Model
        {
            get
            {
                return _model;
            }
        }

        public string Name
        {
            get
            {
                return _model.Name;
            }
        }

        public ArchitecturalNodeViewModel RootNode
        {
            get
            {
                return _rootNode;
            }
        }

        private ArchitecturalNodeViewModel? _selectedNode;

        public ArchitecturalNodeViewModel? SelectedNode
        {
            get
            {
                return _selectedNode;
            }
        }

        /// <summary>
        /// The selected node if it is a room, otherwise null.
        /// </summary>
        public ArchitecturalNodeViewModel? SelectedRoom
        {
            get
            {
                ArchitecturalNodeViewModel? result = null;

                if (_selectedNode != null && _selectedNode.NodeType == ArchitecturalNodeType.Room)
                {
                    result = _selectedNode;
                }

                return result;
            }
        }

        public void Select(ArchitecturalNodeViewModel? node)
        {
            if (_selectedNode != null)
            {
                _selectedNode.IsSelected = false;
            }

            _selectedNode = node;

            if (_selectedNode != null)
            {
                _selectedNode.IsSelected = true;
            }

            OnPropertyChanged(nameof(SelectedNode));
            OnPropertyChanged(nameof(SelectedRoom));
        }

        /// <summary>
        /// Finds the node that has to contain a new element. The search starts at the target
        /// (or the selected node) and walks up the tree.
        /// </summary>
        public ArchitecturalNodeViewModel? FindHost(PaletteItemType itemType, ArchitecturalNodeViewModel? target)
        {
            ArchitecturalNodeType requiredType = itemType.GetRequiredParentType();
            ArchitecturalNodeViewModel? result = requiredType == ArchitecturalNodeType.Project ? _rootNode : target ?? _selectedNode;

            while (result != null && result.NodeType != requiredType)
            {
                result = result.Parent;
            }

            return result;
        }

        public void AddStructuralElement(ArchitecturalNodeViewModel host, PaletteItemType itemType, string name)
        {
            string? childId = _model.Architectural.AddChild(host.Id, name);

            if (childId != null)
            {
                if (itemType == PaletteItemType.JunctionBox)
                {
                    host.RefreshContent();
                }
                else
                {
                    ArchitecturalNodeViewModel newNode = host.AddChildNode(childId);
                    host.IsExpanded = true;
                    Select(newNode);
                }
            }
        }

        /// <summary>
        /// Deletes the selected node (the project root cannot be deleted).
        /// </summary>
        public void DeleteSelectedNode()
        {
            ArchitecturalNodeViewModel? node = _selectedNode;

            if (node != null && node.Parent != null)
            {
                ArchitecturalNodeViewModel parent = node.Parent;
                _model.RemoveStructuralElement(node.Id);
                parent.RemoveChildNode(node);
                Select(parent);
            }
        }

        /// <summary>
        /// Reads the content of the selected room again.
        /// </summary>
        public void RefreshSelectedRoom()
        {
            _selectedNode?.RefreshContent();
        }

        private void OnModelModified(object? sender, EventArgs e)
        {
            RefreshSelectedRoom();
            Modified?.Invoke(this, EventArgs.Empty);
        }

        private static void ExpandAll(ArchitecturalNodeViewModel node)
        {
            node.IsExpanded = true;

            foreach (ArchitecturalNodeViewModel currentChild in node.Children)
            {
                ExpandAll(currentChild);
            }
        }
    }
}

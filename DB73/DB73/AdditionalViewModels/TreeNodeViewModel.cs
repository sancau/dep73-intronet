namespace DB73.AdditionalViewModels
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using DB73.Models;
    using DB73.ViewModels;

    public class TreeNodeViewModel : ViewModelBase
    {
        private readonly ObservableCollection<TreeNodeViewModel> children;
        private readonly string name;
        public static DocumentsViewModel VIEWMODEL_REF;

        private readonly int id;
        private readonly string type;
        private readonly string doctype;

        private bool expanded;
        private bool match = true;

        public TreeNodeViewModel(int id, string name, IEnumerable<TreeNodeViewModel> children, string type)
        {
            this.name = name;
            this.type = type;
            this.id = id;
            this.children = new ObservableCollection<TreeNodeViewModel>(children);
        }

        public TreeNodeViewModel(int id, string name, string type, string doctype)
            : this(id, name, Enumerable.Empty<TreeNodeViewModel>(), type)
        {
            this.doctype = doctype;
        }

        public TreeNodeViewModel(int id, string name, string type)
            : this(id, name, Enumerable.Empty<TreeNodeViewModel>(), type)
        {
        }

        public bool IsFolder
        {
            get { return this.IsLeaf ? false : true; }
        }

        public override string ToString()
        {
            return name;
        }

        private bool IsCriteriaMatched(string criteria)
        {
            return String.IsNullOrEmpty(criteria.ToLower()) || name.ToLower().Contains(criteria);
        }

        public void ApplyCriteria(string criteria, Stack<TreeNodeViewModel> ancestors)
        {

            if (IsCriteriaMatched(criteria))
            {
                IsMatch = true;
                foreach (var ancestor in ancestors)
                {
                    ancestor.IsMatch = true;
                    ancestor.IsExpanded = !String.IsNullOrEmpty(criteria);
                }
            }
            else
                IsMatch = false;

            ancestors.Push(this);
            foreach (var child in Children)
                child.ApplyCriteria(criteria, ancestors);

            ancestors.Pop();
        }

        public IEnumerable<TreeNodeViewModel> Children
        {
            get { return children; }
        }

        public int ID { get { return id; } }

        public string Name
        {
            get { return name; }
        }

        public string Type
        {
            get { return type; }
        }

        public string DocType
        {
            get { return doctype; }
        }

        public bool IsExpanded
        {
            get { return expanded; }
            set
            {
                if (value == expanded)
                    return;

                expanded = value;
                if (expanded)
                {
                    foreach (var child in Children)
                        child.IsMatch = true;
                }
                OnPropertyChanged("IsExpanded");
            }
        }

        public bool IsMatch
        {
            get { return match; }
            set
            {
                if (value == match)
                    return;

                match = value;
                OnPropertyChanged("IsMatch");
            }
        }

        public bool IsLeaf
        {
            get { return !Children.Any(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                    if (_isSelected)
                    {
                        SelectedItem = this;
                    }
                }
            }
        }

        private TreeNodeViewModel _selectedItem = null;
        public TreeNodeViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnSelectedItemChanged();
            }
        }

        public void OnSelectedItemChanged() // что делаем при выделении
        {
            if (SelectedItem.Type == "Document")
            {
                VIEWMODEL_REF.SelectedDocument = Document.List.Find(doc => doc.ID == SelectedItem.ID);
            }
            if (SelectedItem.Type == "Folder")
            {
                VIEWMODEL_REF.SelectedFolder = Folder.List.Find(folder => folder.ID == SelectedItem.ID);
            }
        }

        #region Tree Generation

        public static ObservableCollection<TreeNodeViewModel> DrawTree(Folder rootfolder,
            List<Folder> folderlist, TreeNodeViewModel selectedItem)
        {
            // ищем рут
            var root = rootfolder;

            // список детей рута
            var rootFolders = GetChildren(root.ID, folderlist);

            // список файлов в руте
            var rootDocuments = GetChildren(root);

            // массив представлений элеметов дерева
            var viewsArray = new ObservableCollection<TreeNodeViewModel>();

            // рисуем файлы рута       
            foreach (var document in rootDocuments)
            {
                viewsArray.Add(new TreeNodeViewModel(document.ID, document.Name, "Document"));
            }

            // обсчитываем и рисуем потомков рута
            foreach (Folder fol in rootFolders)
            {
                viewsArray.Add(DrawNode(fol, folderlist));
            }

            // handle selected item applying
            if (selectedItem != null)
            {
                var item = TryFindItem(viewsArray, selectedItem);

                if (item != null)
                {
                    item.IsSelected = true;
                }
            }

            return viewsArray;
        }

        // gets a certain item in the current tree view nodes collection
        private static TreeNodeViewModel TryFindItem(IEnumerable<TreeNodeViewModel> nodes, TreeNodeViewModel item)
        {
            if (item == null) return null;

            TreeNodeViewModel output;
            output = nodes.Where(n => n.ID == item.ID && n.Type == item.Type).FirstOrDefault();

            if (output != null) return output;

            else
            {
                foreach (var node in nodes)
                {
                    output = TryFindItem(node.Children, item);

                    if (output != null) return output;
                }
            }
            return null;
        }

        public static List<Folder> GetChildren(int parentID, List<Folder> folderList)
        {
            return folderList.FindAll(f => f.ParentFolderID == parentID);
        }


        public static List<Document> GetChildren(Folder folder)
        {
            return Folder.Pull(folder.ID).DocumentList;
        }

        public static TreeNodeViewModel DrawNode(Folder folder, List<Folder> folderlist)
        {
            var folderContains = new List<TreeNodeViewModel>();

            var documentList = GetChildren(folder);

            foreach (var document in documentList)
            {
                folderContains.Add(new TreeNodeViewModel(document.ID, document.Name, "Document", document.Type));
            }

            var subFolderList = GetChildren(folder.ID, folderlist);

            foreach (Folder subFolder in subFolderList)
            {
                folderContains.Add(DrawNode(subFolder, folderlist));
            }

            return new TreeNodeViewModel(folder.ID, folder.Name, folderContains, "Folder");
        }

        #endregion
    }
}
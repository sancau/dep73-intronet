namespace DB73.ViewModels
{
    using DB73.Models;
    using DB73.Helpers;
    using DB73.AdditionalViewModels;
    using DB73.BL;

    using System.Linq;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using System.Timers;

    public class DocumentsViewModel : WorkspaceViewModel
    {
        // constructor
        public DocumentsViewModel()
        {
            TreeNodeViewModel.VIEWMODEL_REF = this;

            PackageLinks = new ObservableCollection<Link>();

            TreeNodes = new ObservableCollection<TreeNodeViewModel>();

            RefreshCycleCompleted = true;
            var autoRefreshTimer = new Timer();
            autoRefreshTimer.Elapsed += new ElapsedEventHandler(OnAutoRefreshTimer);
            autoRefreshTimer.Interval = 1000;
            autoRefreshTimer.Start();

            WorkspaceTitle = "Документы и папки";
        }

        public DocumentsViewModel(Link link)
            : this()
        {
            SatteliteWindow.OnProcessStart("Идет построение дерева документов...");

                ShowLink(link);

            SatteliteWindow.OnProcessComplete();
        }

        public void ShowLink(Link link)
        {
            switch (link.LinkedType)
            {
                case "Document":
                    SelectedDocument = link.GetObject() as Document;
                    ExpandToSelected(SelectedDocument, TreeNodes);
                    break;
                case "Folder":
                    SelectedFolder = link.GetObject() as Folder;
                    ExpandToSelected(SelectedFolder, TreeNodes);
                    break;
            }
        }

        #region Fields

        private Document _selectedDocument;
        private Folder _selectedFolder;
        private string _searchString;

        #endregion

        // access rights properties
        public bool IsAdmin { get { return Session.ActiveUser.IsAdmin; } }
        public bool IsSuperUser { get { return Session.ActiveUser.IsSuperUser; } }
        public bool IsAdminOrCreator
        {
            get
            {
                if (SelectedFolder != null)
                    return SelectedFolder.AdderID == Session.ActiveUser.ID || IsAdmin;
                if (SelectedDocument != null)
                    return SelectedDocument.AdderID == Session.ActiveUser.ID || IsAdmin;

                else return false;
            }
        }

        // on request close override
        public override void OnRequestClose()
        {
            if (EditProcess != null)
            {
                UIMessager.ShowMessage("Обнаружена активная сессия редактирования! \nЗакройте редактируемый файл!", "Документы и папки");
                return;
            }

            base.OnRequestClose();
        }

        // workspace title
        public string WorkspaceTitle { get; set; }

        // refresh view logic
        private bool RefreshCycleCompleted { get; set; }
        private void OnAutoRefreshTimer(object sender, ElapsedEventArgs e)
        {
            // select proper folder or document
            try
            {
                if (SelectedDocument != null)
                {
                    var refreshed = Document.Pull(SelectedDocument.ID);

                    if ((SelectedDocument.Name != refreshed.Name) ||
                        (SelectedDocument.Annotation != refreshed.Annotation))
                        return;
                    SelectedDocument = refreshed;
                }

                if (SelectedFolder != null)
                {
                    var refreshed = Folder.Pull(SelectedFolder.ID);

                    if ((SelectedFolder.Name != refreshed.Name) ||
                        (SelectedFolder.Annotation != refreshed.Annotation))
                        return;
                    SelectedFolder = refreshed;
                }
            }
            catch
            {
                return;
            }
        }

        // how we refresh the treeview
        public override void RefreshView()
        {
            RefreshCycleCompleted = false;

            // initialize state var
            _state = new Dictionary<int, bool>();
            // store tree state in _state
            GetTreeState(TreeNodes);

            // get new updated collection
            TreeNodes = MainWindowViewModel.DocumentTree;

            // restore tree state
            ApplyTreeState(TreeNodes, _state);

            // update view
            OnPropertyChanged("TreeNodes");

            /* select proper folder or document
            if (SelectedDocument != null) // && DocumentTextBoxes are not in focus
                SelectedDocument = Document.Pull(SelectedDocument.ID);
               

            if (SelectedFolder != null) // && FolderTextBoxes are not in focus
                SelectedFolder = Folder.Pull(SelectedFolder.ID);*/

            RefreshCycleCompleted = true;
        }

        // gets selected item from TreeNodes collection
        private TreeNodeViewModel GetSelectedItem(IEnumerable<TreeNodeViewModel> nodes)
        {
            var selected = nodes.Where(n => n.IsSelected == true).FirstOrDefault();

            if (selected != null)
                return selected;

            else
            {
                foreach (var node in nodes)
                {
                    selected = GetSelectedItem(node.Children);

                    if (selected != null) return selected;
                }
            }

            return null;
        }

        // package links list
        public ObservableCollection<Link> PackageLinks { get; set; }

        // tree view source
        public ObservableCollection<TreeNodeViewModel> TreeNodes { get; set; }

        // tree view search and state logic
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (value == _searchString)
                    return;

                _searchString = value;
                OnPropertyChanged("SearchString");

                if (_searchString.Length > 2 || _searchString.Length == 0)
                    ApplyFilter();
            }
        }
        private void ApplyFilter()
        {
            foreach (var node in TreeNodes)
                node.ApplyCriteria(SearchString.ToLower(), new Stack<TreeNodeViewModel>());
        }

        // selected items
        public Document SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                if (value != null)
                {
                    _selectedDocument = Document.Pull(value.ID);
                    SelectedFolder = null;
                    OnPropertyChanged("SelectedDocument");
                    OnPropertyChanged("IsAdminOrCreator");
                    OnPropertyChanged("DocumentDetailsVisible");
                    OnPropertyChanged("StatusInfoColor");
                    RefreshVisibility();
                }
                else
                {
                    _selectedDocument = null;
                }
            }
        }
        public Folder SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                if (value != null)
                {
                    _selectedFolder = Folder.Pull(value.ID);
                    SelectedDocument = null;
                    OnPropertyChanged("SelectedFolder");
                    OnPropertyChanged("IsAdminOrCreator");
                    OnPropertyChanged("FolderDetailsVisible");
                    RefreshVisibility();
                }
                else
                {
                    _selectedFolder = null;
                }
            }
        }
        public Link SelectedLink { get; set; }

        // doc status color
        public string StatusInfoColor
        {
            get
            {
                if (SelectedDocument == null) return "Green";
                return SelectedDocument.IsBusy ? "Red" : "Green";
            }
        }

        // panels visibility
        public bool DocumentDetailsVisible
        {
            get { return SelectedDocument == null ? false : true; }
        }
        public bool FolderDetailsVisible
        {
            get { return SelectedFolder == null ? false : true; }
        }
        public bool PackageVisible
        {
            get { return DocumentDetailsVisible || FolderDetailsVisible ? true : false; }
        }

        // visibility refresher
        private void RefreshVisibility()
        {
            OnPropertyChanged("DocumentDetailsVisible");
            OnPropertyChanged("FolderDetailsVisible");
            OnPropertyChanged("PackageVisible");
        }

        #region Commands

        #region Folder Commands

        // add folder view
        public ICommand ShowAddFolder
        {
            get
            {
                return new RelayCommand(p =>
                    SatteliteWindow.
                        ShowSatteliteWindow(new DB73.Views.NewFolderView(),
                                            new NewFolderViewModel(this)));
            }
        }

        // add subfolder view
        public ICommand ShowAddSubfolder
        {
            get
            {
                return new RelayCommand(p =>
                    OnShowAddSubFolder());
            }
        }
        private void OnShowAddSubFolder()
        {
            if (SelectedFolder != null)
            {
                if ((SelectedFolder.ID == 2) ||
                    (SelectedFolder.ID == 3) ||
                    (SelectedFolder.ID == 4) ||
                    (SelectedFolder.ID == 5) ||
                    (SelectedFolder.ID == 6))
                {
                    UIMessager.ShowMessage("Операции с системными папками запрещены");
                    return;
                }
                else
                {
                    SatteliteWindow.
                        ShowSatteliteWindow(new DB73.Views.NewFolderView(),
                                            new NewFolderViewModel(this, SelectedFolder));
                }
            }

        }

        // doc import view
        public ICommand ShowImportDocument
        {
            get
            {
                return new RelayCommand(p =>
                    OnShowImportDocument());
            }
        }
        private void OnShowImportDocument()
        {
            if (SelectedFolder != null)
            {
                if ((SelectedFolder.ID == 2) ||
                (SelectedFolder.ID == 3) ||
                (SelectedFolder.ID == 4) ||
                (SelectedFolder.ID == 5))
                {
                    UIMessager.ShowMessage("Операции с системными папками запрещены");
                    return;
                }
                else
                {
                    SatteliteWindow.
                        ShowSatteliteWindow(new DB73.Views.ImportDocumentView(),
                                            new ImportDocumentViewModel(SelectedFolder, this));
                }
            }
        }

        // import tree
        public ICommand ImportTreeToFolder
        {
            get
            {
                return new RelayCommand(parameter => this.OnImportTreeToFolder());
            }
        }
        private void OnImportTreeToFolder()
        {
            if (SelectedFolder == null)
            {
                UIMessager.ShowMessage("Выберите папку в которую будет импортировано дерево!");
                return;
            }

            if ((SelectedFolder.ID == 2) ||
                (SelectedFolder.ID == 3) ||
                (SelectedFolder.ID == 4) ||
                (SelectedFolder.ID == 5))
            {
                UIMessager.ShowMessage("Операции с системными папками запрещены");
                return;
            }

            SatteliteWindow.ShowSatteliteWindow(new DB73.Views.TreeImportView(),
                new TreeImportViewModel(SelectedFolder, this));
        }

        // manage access params
        public ICommand ShowAccessConfig
        {
            get
            {
                return new RelayCommand(p =>
                    SatteliteWindow.
                        ShowSatteliteWindow(new DB73.Views.FolderConfigView(),
                                            new FolderConfigViewModel(SelectedFolder, this)));
            }
        }

        // save folder changes
        public ICommand SaveFolderChanges
        {
            get
            {
                return new RelayCommand(p => OnSaveFolderChanges());
            }
        }
        private void OnSaveFolderChanges()
        {
            if (SelectedFolder == null) return;

            if ((SelectedFolder.ID == 2) ||
                 (SelectedFolder.ID == 3) ||
                 (SelectedFolder.ID == 4) ||
                 (SelectedFolder.ID == 5) ||
                 (SelectedFolder.ParentFolderID == 3) ||
                 (SelectedFolder.ParentFolderID == 4) ||
                 (SelectedFolder.ParentFolderID == 5))
            {
                UIMessager.ShowMessage("Операции с системными папками запрещены");
                return;
            }

            if (SelectedFolder.IsValid)
            {
                if (SelectedFolder.Push())
                {
                    UIMessager.ShowMessage(new LogicResponse(true, "data_saved").Message);
                    RefreshView();
                }
                else
                {
                    UIMessager.ShowMessage(new LogicResponse(false).Message);
                }
            }
        }

        // delete folder
        public ICommand DeleteFolder
        {
            get
            {
                return new RelayCommand(p => OnDeleteFolder(SelectedFolder));
            }
        }
        private void OnDeleteFolder(Folder selectedFolder)
        {
            if (selectedFolder == null) return;

            if ((SelectedFolder.ID == 2) ||
                (SelectedFolder.ID == 3) ||
                (SelectedFolder.ID == 4) ||
                (SelectedFolder.ID == 5) ||
                (SelectedFolder.ParentFolderID == 3) ||
                (SelectedFolder.ParentFolderID == 4) ||
                (SelectedFolder.ParentFolderID == 5))
            {
                UIMessager.ShowMessage("Операции с системными папками запрещены");
                return;
            }

            if (!UIMessager.DeleteConfirmationDialog()) return;

            var response = DocumentManager.Delete(selectedFolder);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone) RefreshView();
        }

        #endregion

        #region Document Commands

        // open read only
        public ICommand OpenReadOnly
        {
            get { return new RelayCommand(p => OnOpenReadOnly()); }
        }
        private void OnOpenReadOnly()
        {
            if (SelectedDocument == null) return;

            var response = DocumentManager.OpenReadOnly(SelectedDocument);

            if (!response.IsDone)
            {
                UIMessager.ShowMessage(response.Message);
            }
        }

        #region Edit session handlers and methods

        private Timer EditSessionMonitorTimer;
        public static Process EditProcess;
        private Document DocumentOnEdit;

        public ICommand OpenForEdit
        {
            get { return new RelayCommand(p => OnOpenForEdit()); }
        }
        private void OnOpenForEdit()
        {
            if (SelectedDocument == null) return;

            if (EditProcess != null)
            {
                if (SelectedDocument.Name == DocumentOnEdit.Name)
                {
                    UIMessager.ShowMessage("Вы уже редактируете этот документ!");
                    return;
                }

                UIMessager.ShowMessage("Обнаружена активная сессия редактирования! \nЕдиновременное редактирование нескольких документов запрещено!");
                return;
            }

            var response = DocumentManager.OpenForEdit(SelectedDocument);

            if (response.IsDone)
            {
                DocumentOnEdit = SelectedDocument;
                EditProcess = response.Output;

                //initilize data refreshing
                EditSessionMonitorTimer = new Timer();
                EditSessionMonitorTimer.Elapsed += new ElapsedEventHandler(EditSessionMonitor);
                EditSessionMonitorTimer.Interval = 1000;
                EditSessionMonitorTimer.Start();
            }
            else
                UIMessager.ShowMessage(response.Message);
        }

        private void EditSessionMonitor(object sender, ElapsedEventArgs e)
        {
            if (EditProcess == null)
            {
                UIMessager.ShowMessage("Система не смогла идентифицировать процесс редактирования! Обратитесь к администратору!");
                return;
            }

            bool editSessionEnded = false;

            try
            {
                editSessionEnded = EditProcess == null ? true : EditProcess.HasExited;
            }
            catch
            {
                editSessionEnded = true;
            }


            if (editSessionEnded)
            {
                EditSessionMonitorTimer.Stop();
                OnEditSessionEnd(DocumentOnEdit, EditProcess.StartInfo.FileName);
            }
        }
        private void OnEditSessionEnd(Document document, string editedFilePath)
        {
            if (document == null) return;
            var response = DocumentManager.EndEditSession(document, editedFilePath);
            if (!response.IsDone)
                UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
            {
                EditProcess = null;
                EditSessionMonitorTimer = null;
                DocumentOnEdit = null;
            }
        }

        #endregion

        // save document changes
        public ICommand SaveDocumentChanges
        {
            get
            {
                return new RelayCommand(p => OnSaveDocumentChanges());
            }
        }
        private void OnSaveDocumentChanges()
        {
            if (SelectedDocument == null) return;

            if (SelectedDocument.IsValid)
            {
                if (SelectedDocument.Push())
                {
                    UIMessager.ShowMessage(new LogicResponse(true, "data_saved").Message);
                    RefreshView();
                }
                else
                {
                    UIMessager.ShowMessage(new LogicResponse(false).Message);
                }
            }
        }

        // delete document
        public ICommand DeleteDocument
        {
            get
            {
                return new RelayCommand(p => OnDeleteDocument());
            }
        }
        private void OnDeleteDocument()
        {
            if (SelectedDocument == null) return;

            if (!UIMessager.DeleteConfirmationDialog()) return;

            var response = DocumentManager.Delete(SelectedDocument);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone) RefreshView();
        }

        // replace file
        public ICommand ReplaceFile
        {
            get { return new RelayCommand(p => OnReplaceFile()); }
        }
        private void OnReplaceFile()
        {
            if (SelectedDocument != null)
            {
                string chosenPath = DB73.ViewModels.
                    ImportDocumentViewModel.HandleOpenFileDialogSingleSelect();

                if (chosenPath == null) return;

                var response = DocumentManager.ReplaceFile(chosenPath, SelectedDocument);
                UIMessager.ShowMessage(response.Message);
            }
            else return;
        }

        // copy to private 
        public ICommand CopyDocumentToPrivateStorage
        {
            get
            {
                return new RelayCommand(p => OnCopyDocumentToPrivateStorage());
            }
        }
        private void OnCopyDocumentToPrivateStorage()
        {
            var storagePath = Session.ActiveUser.PrivateStoragePath;

            if (!Directory.Exists(storagePath))
            {
                UIMessager.
                    ShowMessage("Папка личного хранилища не выбрана! Выберите папку в меню ПРОГРАММА -> Моя учётная запись.");
                return;
            }

            if (SelectedDocument == null) return;

            if (DocumentManager.ExistsInPrivateStorage(SelectedDocument, storagePath))
            {
                if (!ReplaceConfirmationDialog(Link.GetLink(SelectedDocument))) return;
            }
            var response = DocumentManager.CopyToPrivateStorage(SelectedDocument, storagePath);

            UIMessager.ShowMessage(response.Message);
        }

        #endregion

        #region Helping methods

        private bool ReplaceConfirmationDialog(Link link)
        {
            string linkType = "объект ";

            if (link.LinkedType == "Document") linkType = "документ ";
            if (link.LinkedType == "Folder") linkType = "папка ";

            System.Windows.MessageBoxResult result =
                System.Windows.MessageBox.
                            Show("Копия данного объекта (" + linkType + link.LinkedName + ") уже имеется в Вашем личном хранилище. Перезаписать?",
                                "Необратимая операция!",
                                System.Windows.MessageBoxButton.YesNo);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Treeview Commands

        // on tree view item double click 
        public void OnDocumentDoubleClick()
        {
            // default action logic
            OnOpenReadOnly();
        }

        #endregion

        #region TreeView State

        // TODO - WOULD BE NICE TO HIGHLIGHT THE SELECTED ITEM ON TREEVIEW AND PACKAGE LIST

        // tree state holder
        private Dictionary<int, bool> _state;

        // tree state getter
        private void GetTreeState(ObservableCollection<TreeNodeViewModel> treeNodes)
        {
            foreach (var node in treeNodes)
            {
                if (node.Type == "Folder")
                {
                    if ((SelectedFolder != null) && (node.ID == SelectedFolder.ID))
                    {
                        _state.Add(node.ID, true);
                    }

                    else
                    {
                        _state.Add(node.ID, node.IsExpanded);
                    }
                }

                var nodeChildren = new ObservableCollection<TreeNodeViewModel>(node.Children);

                if (nodeChildren.Count != 0)
                    GetTreeState(nodeChildren);
            }
        }
        // tree state applier
        private void ApplyTreeState(IEnumerable<TreeNodeViewModel> treeNodes, Dictionary<int, bool> state)
        {
            foreach (var node in treeNodes)
            {
                if (node.Type == "Folder")
                {
                    var stateKey = state.Keys.Where(k => k.Equals(node.ID)).FirstOrDefault();

                    if (stateKey != 0)
                        node.IsExpanded = state[stateKey];
                }

                if (node.Children.Count() != 0)
                    ApplyTreeState(node.Children, _state);
            }
        }

        // expand to selected item 
        private void ExpandToSelected(Document document, IEnumerable<TreeNodeViewModel> nodes)
        {
            ExpandToSelected(Folder.Pull(document.ParentFolderID), nodes);
        }
        private void ExpandToSelected(Folder folder, IEnumerable<TreeNodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Type == "Folder")
                {
                    if (folder.IsSubfolderFor(Folder.Pull(node.ID)) || node.ID == folder.ID)
                    {
                        node.IsExpanded = true;
                    }
                    else
                    {
                        node.IsExpanded = false;
                    }
                }

                if (node.Children.Count() != 0)
                    ExpandToSelected(folder, node.Children);
            }
        }

        #endregion

        #region Package

        // on link item double click
        public void OnLinkDoubleClick()
        {
            // link show logic
            if (SelectedLink == null) return;
            LinkViewCaster.ShowLink(SelectedLink);
        }

        // on add to package
        public ICommand AddLink
        {
            get
            {
                return new RelayCommand(p => OnAddLink());
            }
        }
        private void OnAddLink()
        {
            Link link;
            if (SelectedDocument != null)
            {
                link = Link.GetLink(SelectedDocument);
            }
            else if (SelectedFolder != null)
            {
                link = Link.GetLink(SelectedFolder);
            }
            else return;

            if (PackageLinks.Where(l => l.ID == link.ID).Count() != 0)
            {
                UIMessager.ShowMessage("Ссылка на данный объект уже содержится в текущем наборе");
                return;
            }
            PackageLinks.Add(link);
            OnPropertyChanged("PackageLinks");
        }

        // on remove from package
        public ICommand RemoveLink
        {
            get
            {
                return new RelayCommand(p => OnRemoveLink());
            }
        }
        private void OnRemoveLink()
        {
            if (SelectedLink != null)
            {
                PackageLinks.Remove(SelectedLink);
                OnPropertyChanged("PackageLinks");
            }
        }

        // send package
        public ICommand SendPackage
        {
            get { return new RelayCommand(p => OnSendPackage()); }
        }
        private void OnSendPackage()
        {
            if (PackageLinks.Count == 0)
            {
                UIMessager.ShowMessage("Набор пуст");
                return;
            }

            var window = new DB73.Views.NewMessageView();
            var vm = new NewMessageViewModel(window, PackageLinks);
            window.DataContext = vm;
            MainWindowViewModel.OpenedWindows.Add(window);
            window.Show();
        }

        // paste package
        public ICommand PastePackage
        {
            get { return new RelayCommand(p => OnPastePackage()); }
        }
        private void OnPastePackage()
        {
            if (PackageLinks.Count == 0)
            {
                UIMessager.ShowMessage("Набор пуст");
                return;
            }

            if ((SelectedFolder.ID == 2) ||
                (SelectedFolder.ID == 3) ||
                (SelectedFolder.ID == 4) ||
                (SelectedFolder.ID == 5))
            {
                UIMessager.ShowMessage("Операции с системными папками запрещены");
                return;
            }


            if (SelectedFolder == null)
            {
                UIMessager.ShowMessage("Целевая папка не выбрана");
                return;
            }

            // pop paste dialog
            SatteliteWindow.ShowSatteliteWindow(new DB73.Views.PasteDialogView(),
                                                new PasteDialogViewModel(PackageLinks, this));
        }

        // request of package copy
        public ICommand RequestPackageCopy
        {
            get { return new RelayCommand(p => OnRequestPackageCopy()); }
        }
        private void OnRequestPackageCopy()
        {
            if (PackageLinks.Count == 0)
            {
                UIMessager.ShowMessage("Набор пуст!");
                return;
            }

            LogicResponse response = new LogicResponse(false);

            foreach (var link in PackageLinks)
            {
                var storagePath = Session.ActiveUser.PrivateStoragePath;

                if (!Directory.Exists(storagePath))
                {
                    UIMessager.
                        ShowMessage("Папка личного хранилища не выбрана! Выберите папку в меню ПРОГРАММА -> Моя учётная запись.");
                    return;
                }

                dynamic obj;

                if (link.LinkedType == "Folder")
                {
                    obj = link.GetObject() as Folder;
                }
                else if (link.LinkedType == "Document")
                {
                    obj = link.GetObject() as Document;
                }
                else return;

                if (DocumentManager.ExistsInPrivateStorage(obj, storagePath))
                {
                    if (!ReplaceConfirmationDialog(Link.GetLink(obj))) return;
                }
                response = DocumentManager.CopyToPrivateStorage(obj, storagePath);

                if (!response.IsDone)
                    UIMessager.ShowMessage(response.Message);
            }

            if (response.IsDone)
                UIMessager.ShowMessage("Копирование успешно завершено");
        }

        #endregion

        #endregion
    }
}
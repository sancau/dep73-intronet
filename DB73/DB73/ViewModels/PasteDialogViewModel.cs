namespace DB73.ViewModels
{
    using DB73.Models;
    using DB73.BL;
    using DB73.Helpers;

    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public class PasteDialogViewModel : WorkspaceViewModel
    {
        public PasteDialogViewModel()
        {

        }

        public PasteDialogViewModel(ObservableCollection<Link> links, DocumentsViewModel requester)
        {
            this.Requester = requester;
            this.Links = links;
        }

        public override void OnRequestClose()
        {
            Requester.RefreshView();
            SatteliteWindow.CloseSatteliteWindow();
        }

        public DocumentsViewModel Requester { get; set; }
        public ObservableCollection<Link> Links { get; set; }
        public Link SelectedLink { get; set; }
        public Folder TargetFolder
        {
            get { return Requester.SelectedFolder; }
        }

        // on move click
        public ICommand Move
        {
            get { return new RelayCommand(p => OnMove()); }
        }
        private void OnMove()
        {
            var response = new LogicResponse(false);

            foreach (var link in Links)
            {
                if (link.LinkedType == "Folder")
                {
                    if (link.LinkedID == TargetFolder.ID)
                    {
                        UIMessager.ShowMessage("Перемещаемая папка совпадает с целевой папкой. Объект будет пропущен!");
                    }
                    else
                    {
                        response = DocumentManager.Move(link.GetObject() as Folder, TargetFolder);

                        if (!response.IsDone)
                        {
                            UIMessager.ShowMessage(response.Message);
                            return;
                        }
                    }
                }

                if (link.LinkedType == "Document")
                {
                    var obj = link.GetObject() as Document;

                    if (obj.ParentFolderID == TargetFolder.ID)
                    {
                        UIMessager.ShowMessage("Перемещаемый документ уже находится в целевой папке. Объект будет пропущен!");
                    }
                    else
                    {
                        response = DocumentManager.Move(obj, TargetFolder);

                        if (!response.IsDone)
                        {
                            UIMessager.ShowMessage(response.Message);
                            return;
                        }
                    }                 
                }
            }

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
            {
                OnRequestClose();
            }
        }

        // on copy click
        public ICommand Copy
        {
            get { return new RelayCommand(p => OnCopy()); }
        }
        private void OnCopy()
        {
            var response = new LogicResponse(false);

            foreach (var link in Links)
            {
                if (link.LinkedType == "Folder")
                {
                    response = DocumentManager.Copy(link.GetObject() as Folder, TargetFolder);

                    if (!response.IsDone)
                    {
                        UIMessager.ShowMessage(response.Message);
                        return;
                    }
                }

                if (link.LinkedType == "Document")
                {
                    response = DocumentManager.Copy(link.GetObject() as Document, TargetFolder);

                    if (!response.IsDone)
                    {
                        UIMessager.ShowMessage(response.Message);
                        return;
                    }
                }
            }

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
            {
                OnRequestClose();
            }
        }

        // on cancel click
        public ICommand Cancel
        {
            get { return new RelayCommand(p => OnRequestClose()); }
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
                if (Links.Count == 1)
                {
                    UIMessager.ShowMessage("Набор не может быть пустым!");
                    return;
                }

                Links.Remove(SelectedLink);
                OnPropertyChanged("PackageLinks");
            }
        }

        // on link double click
        public void OnLinkDoubleClick()
        {
            if (SelectedLink == null) return;
            LinkViewCaster.ShowLink(SelectedLink);
        }
    }
}

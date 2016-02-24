namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class Folder : IDataErrorInfo
    {
        #region Fields

        private List<User> _userList;
        private List<Document> _documentList;

        #endregion

        #region Constructors

        public Folder()
        {
        }

        public Folder(string param)
        {
            if (param == "generate_root")
            {
                this.Name = "ROOT";
                this.ParentFolderID = 0;
                this.Annotation = "ROOT_FOLDER";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }

            if (param == "generate_inventory_folder")
            {
                this.Name = "Инвентарь";
                this.ParentFolderID = 1;
                this.Annotation = "Содержит папки документов прикрепленные к определённым инвентарным позициям";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }

            if (param == "generate_misc_item_folder")
            {
                this.Name = "Прочие позиции";
                this.ParentFolderID = AppConfig.InventoryRootFolderID;
                this.Annotation = "Содержит папки документов прикрепленные к определённым инвентарным позициям";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }

            if (param == "generate_tool_folder")
            {
                this.Name = "Средства измерений и приборы";
                this.ParentFolderID = AppConfig.InventoryRootFolderID;
                this.Annotation = "Содержит папки документов прикрепленные к определённым инвентарным позициям";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }

            if (param == "generate_test_system_folder")
            {
                this.Name = "Испытательные установки";
                this.ParentFolderID = AppConfig.InventoryRootFolderID;
                this.Annotation = "Содержит папки документов прикрепленные к определённым инвентарным позициям";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }

            if (param == "generate_projects_folder")
            {
                this.Name = "Проекты";
                this.ParentFolderID = 1;
                this.Annotation = "Содержит папки документов прикрепленные к определённым проектам";
                this.AdderID = 0;
                this.AddDate = Server.CurrentTime;
                this.EditDate = this.AddDate;
            }
        }

        public Folder(InventoryItem item)
        {
            this.Name = item.Name;
            this.ParentFolderID = AppConfig.GetInventoryFolderID(item.ItemType);
            this.Annotation = "Папка сопутствующей документации для " + item.Name;
            this.AdderID = 0;
            this.AddDate = Server.CurrentTime;
            this.EditDate = this.AddDate;
        }

        #endregion

        #region Properties

        public int ID { get; set; }

        public int ParentFolderID { get; set; }

        public string Name { get; set; }

        public DateTime AddDate { get; set; }
        public DateTime EditDate { get; set; }

        public int AdderID { get; set; }
        public int EditorID { get; set; }

        public string Annotation { get; set; }

        public virtual List<Document> DocumentList
        {
            get
            {
                if (_documentList == null)
                    _documentList = new List<Document>();
                return _documentList;
            }
            set
            {
                if (_documentList == null)
                    _documentList = new List<Document>();
                if (_documentList != value)
                    _documentList = value;
            }
        }

        public virtual List<User> UserList
        {
            get
            {
                if (_userList == null)
                    _userList = new List<User>();
                return _userList;
            }
            set 
			{
                if (_userList == null)
                    _userList = new List<User>();
				if (_userList != value)
					_userList = value;
			}				
        }

        #endregion

        #region DataAccess members

        public static Folder Pull(int id)
        {
            try { var entity = DataInterface<Folder>.Pull(id); return entity; }
            catch(Exception) { return null; }
        }

        public static List<Folder> FilteredList(User user) 
        {
            var userEntity = User.Pull(user.ID);
            var allFolders = DataInterface<Folder>.List;

            if (user.IsSystemAdmin)
                return allFolders;
            else
                return user.FolderList;                         
        }

        // gets all folders need to be rendered to view last edited list of documents 
        // based on user folder access
        public static List<Folder> GetRangedByEditDateFilteredList(User user, int numberOfDocs)
        {
            var output = new List<Folder>();
            var userFolders = user.FolderList;

            // go through last modified docs
            foreach (var d in Document.GetLastEditedList(numberOfDocs))
            {
                var fol = Folder.Pull(d.ParentFolderID);
                // if user has access to doc
                if (userFolders.FindAll(f => f.ID == fol.ID).Count != 0)
                {
                    // add all tree from doc till root
                    int folderID = d.ParentFolderID;
                    while (folderID != 1)
                    { 
                        if (output.FindAll(f => f.ID == folderID).Count == 0)
                        {
                            output.Add(Folder.Pull(folderID));
                        }
                        folderID = Folder.Pull(folderID).ParentFolderID;
                    }
                }
            }
            return output;
        }

        public static List<Folder> List
        {
            get
            {
                try { return DataInterface<Folder>.List; }
                catch { return null; }
            }
        }

        public bool Push()
        {
            try
            {
                if (this.ID == 0)
                {                    
                    DataInterface<Folder>.Push(this);
                                        
                    DataInterface<Link>.
                        Push(new Link(this.ID, this.Name, "Folder"));
                }
                else if (this.Name != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.Name;
                    link.Push();
                    
                    DataInterface<Folder>.Push(this);
                }
                else
                {
                    DataInterface<Folder>.Push(this);
                }
                return true;
            }
            catch (Exception ex) { return false; }       
        }

        public bool Delete()
        {
            try
            {
                var link = Link.GetLink(this);

                DataInterface<Folder>.Delete(this.ID);

                link.Delete();

                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }
        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

        #endregion

        #region Validation

        static readonly string[] ValidatedProperties =
        {
            "Name"
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)

                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }
        private string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "Name":
                    error = ValidateName();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateName()
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return "Имя папки не выбрано";
            }

            if (Name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return "Недопустимые симболы в имени папки";
            }

            if (List.FindAll(d => d.Name.ToLower() == Name.ToLower() && d.ID != this.ID).Count != 0)
            {
                return "Папка с таким именем уже существует в базе. Измените имя папки";
            }

            return null;
        }

        #endregion

        #region Helping methods

        public bool IsSubfolderFor(Folder folder)
        {
            int i = GetParentID(this);
            while (i != 0)
            {
                if (i == folder.ID)
                {
                    return true;
                }

                i = GetParentID(Folder.Pull(i));
            }
            return false;
        }

        private int GetParentID(Folder folder)
        {
            return folder.ParentFolderID;
        }

        #endregion
    }
}

namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Document : IDataErrorInfo
    {
        #region Properties

        public int ID { get; set; }

        public int ParentFolderID { get; set; }

        private List<Folder> _folderList;
        public virtual List<Folder> FolderList
        {
            get
            {
                if (_folderList == null)
                    _folderList = new List<Folder>();
                return _folderList;
            }
            set
            {
                if (_folderList == null)
                    _folderList = new List<Folder>();
                if (_folderList != value)
                    _folderList = value;
            }
        }

        public string Name { get; set; }

        public bool IsBusy { get; set; }

        public int HolderID { get; set; }

        public DateTime AddDate { get; set; }

        public DateTime EditDate { get; set; }

        public DateTime LastAccessDate { get; set; }

        public DateTime CreationDate { get; set; }

        public int AdderID { get; set; }

        public int EditorID { get; set; }

        public bool ChangesForbidden { get; set; }

        public string Type { get; set; }

        public string BusinessType { get; set; }

        public string Path { get; set; }

        public string Annotation { get; set; }

        #endregion

        #region DataAccess members

        public static Document Pull(int id)
        {
            try { var entity = DataInterface<Document>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<Document> List
        {
            get { return DataInterface<Document>.List; }
        }

        // gets list sorted by last edit date 
        public static List<Document> GetLastEditedList(int percent)
        {
            var output = from doc in List
                         orderby doc.EditDate ascending
                         select doc;

            int itemsNumber = output.Count() * percent / 100;

            return output.Skip(Math.Max(0, output.Count() - itemsNumber)).Reverse().ToList();
        }

        // gets list sorted by last edit date 
        public static List<Document> GetLastAccesedList(int percent)
        {
            var output = from doc in List
                         orderby doc.LastAccessDate ascending
                         select doc;

            int itemsNumber = output.Count() * percent / 100;

            return output.Skip(Math.Max(0, output.Count() - itemsNumber)).Reverse().ToList();
        }

        public bool Push()
        {
            try
            {
                if (this.ID == 0)
                {
                    DataInterface<Document>.Push(this);

                    DataInterface<Link>.
                        Push(new Link(this.ID, this.Name, "Document"));
                }
                else if (this.Name != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.Name;
                    link.Push();

                    DataInterface<Document>.Push(this);
                }
                else
                {
                    DataInterface<Document>.Push(this);
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

                DataInterface<Document>.Delete(this.ID);

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
            "Name",
            "BusinessType"
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
                case "BusinessType":
                    error = ValidateBusinessType();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateName()
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return "Имя документа не выбрано";
            }

            if(Name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return "Недопустимые симболы в имени документа";
            }

            if (List.FindAll(d => d.Name.ToLower() == Name.ToLower() && d.ID != this.ID).Count != 0)
            {
                return "Документ с таким именем уже существует в базе. Измените имя документа";
            }

            return null;
        }
        private string ValidateBusinessType()
        {
            if (String.IsNullOrWhiteSpace(BusinessType))
            {
                return "Тип документа не выбран";
            }

            return null;
        }
        #endregion

        #region Additional NOT MAPPED properties

        // PARENT FOLDER NAME 
        [NotMapped]
        public string FolderName
        {
            get
            {
                if (ParentFolderID != 0)
                {
                    return Folder.Pull(ParentFolderID).Name;
                }
                else return String.Empty;
            }
        }
        // EDIT INFO
        [NotMapped]
        public string EditInfo
        {
            get
            {
                string editorInfo;
                if (this.EditorID != 0)
                {
                    var editor = User.Pull(EditorID);

                    editorInfo = editor.FirstName + " " + editor.LastName;
                }
                else return "Изменений не было";

                return EditDate.ToString("dd-MM-yyyy в HH-mm") + " (" + editorInfo + ")";
            }
        }
        // ADD INFO
        [NotMapped]
        public string AddInfo
        {
            get
            {
                string adderInfo;
                if (this.AdderID != 0)
                {
                    var adder = User.Pull(AdderID);

                    adderInfo = adder.FirstName + " " + adder.LastName; 
                }
                else adderInfo = String.Empty;

                return AddDate.ToString("dd-MM-yyyy в HH-mm") + " (" + adderInfo + ")";
            }
        }
        // STATUS INFO
        public string StatusInfo
        {
            get
            {
                return this.IsBusy == true ? "Занят (" + User.Pull(HolderID).LastName + ")" : "Свободен";
            }
        }

        #endregion

        #region Helping methods

        // LOG DOCUMENT REQUEST EVENT
        public bool DeclareRequest(User user, Document document)
        {
            var doc = Document.Pull(document.ID);

            doc.LastAccessDate = Server.CurrentTime;

            if (doc.Push())
                return true;
            else
                return false;
        }

        // HOLDS THE DOCUMENT
        public bool Hold(User user, Document document)
        {
            var doc = Document.Pull(document.ID);

            if (doc.IsBusy) return false;

            doc.IsBusy = true;
            doc.HolderID = user.ID;

            if (doc.Push())
                return true;
            else
                return false;
        }

        #endregion
    }
}

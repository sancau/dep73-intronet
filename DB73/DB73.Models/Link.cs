namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Link : IDataErrorInfo
    {
        #region Constructors

        public Link()
        {

        }

        public Link(int linkedID, string linkedName, string linkedType)
        {
            this.LinkedID = linkedID;
            this.LinkedName = linkedName;
            this.LinkedType = linkedType;
        }

        #endregion

        #region Fields

        private List<Message> _messageList;

        #endregion

        #region Properties

        public int ID { get; set; }

        public int LinkedID { get; set; }

        public string LinkedType { get; set; }
        
        public string LinkedName { get; set; }

        public virtual List<Message> MessageList
        {
            get
            {
                if (_messageList == null)
                    _messageList = new List<Message>();
                return _messageList;
            }
            set
            {
                if (_messageList == null)
                    _messageList = new List<Message>();
                if (_messageList != value)
                    _messageList = value;
            }
        }

        [NotMapped]
        public string LocalizedLinkedType
        {
            get
            {
                switch(LinkedType)
                {
                    case "Document": return "Документ";
                    case "BugTicket": return "Заявка разработчику";
                    case "User": return "Пользователь";
                    case "Message": return "Сообщение";
                    case "Folder": return "Папка";
                    case "MiscItem": return "Инвентарная позиция";
                    case "Tool": return "Прибор";
                    case "TestSystem": return "Испытательная установка";
                    default: return "Не определено локализованное имя";
                }
            }
        }

        #endregion

        // get linked object
        public object GetObject()
        {
            switch (LinkedType)
            {
                case "Document": return Document.Pull(LinkedID) as object;
                case "BugTicket": return BugTicket.Pull(LinkedID) as object;
                case "User": return User.Pull(LinkedID) as object;
                case "Message": return Message.Pull(LinkedID) as object;
                case "Folder": return Folder.Pull(LinkedID) as object;
                case "TestSystem": return InventoryItem.Pull(LinkedID) as object;
                case "MiscItem": return InventoryItem.Pull(LinkedID) as object;
                case "Tool": return InventoryItem.Pull(LinkedID) as object;

                default: throw new NotImplementedException();
            }
        }

        // get link for Document
        public static Link GetLink(Document obj)
        {
            return Link.List.Find(l => l.LinkedType == "Document" && l.LinkedID == obj.ID);
        }
        // get link for BugTicket
        public static Link GetLink(BugTicket obj)
        {
            return Link.List.Find(l => l.LinkedType == "BugTicket" && l.LinkedID == obj.ID);
        }
        // get link for User
        public static Link GetLink(User obj)
        {
            return Link.List.Find(l => l.LinkedType == "User" && l.LinkedID == obj.ID);
        }
        // get link for Message
        public static Link GetLink(Message obj)
        {
            return Link.List.Find(l => l.LinkedType == "Message" && l.LinkedID == obj.ID);
        }
        // get link for Folder
        public static Link GetLink(Folder obj)
        {
            return Link.List.Find(l => l.LinkedType == "Folder" && l.LinkedID == obj.ID);
        }
        // get link for InventoryItem
        public static Link GetLink(InventoryItem obj)
        {
            return Link.List.Find(l => l.LinkedType == obj.ItemType && l.LinkedID == obj.ID);
        }

        #region DataAccess members

        public static Link Pull(int id)
        {
            try { var entity = DataInterface<Link>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<Link> List
        {
            get { return DataInterface<Link>.List;
            }
        }

        public bool Push()
        {
            try { DataInterface<Link>.Push(this); return true; }
            catch (Exception) { return false; }
        }

        public bool Delete()
        {
            try { DataInterface<Link>.Delete(this.ID); return true; }
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
            "LinkedName",
            "LinkedType",
            "LinkedID"
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
                case "LinkedID":
                    error = ValidateLinkedID();
                    break;

                case "LinkedType":
                    error = ValidateLinkedType();
                    break;

                case "LinkedName":
                    error = ValidateLinkedName();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateLinkedID()
        {
            if (LinkedID == 0)
            {
                return "Не указан LinkedID";
            }

            return null;
        }
        private string ValidateLinkedType()
        {
            if (String.IsNullOrWhiteSpace(LinkedType))
            {
                return "Не указан LinkedType";
            }

            return null;
        }
        private string ValidateLinkedName()
        {
            if(String.IsNullOrWhiteSpace(LinkedName))
            {
                return "Не указан LinkedName";
            }
            return null;
        }

        #endregion
    }
}

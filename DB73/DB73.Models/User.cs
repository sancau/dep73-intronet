namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User : IDataErrorInfo
    {
        #region Fields

        private List<Folder> _folderList;
        private List<Message> _messageList;
        private bool _isSystemAdmin;

        #endregion

        #region Properties

        public int ID { get; set; }

        public string Username { get; set; }

        public int Password { get; set; }

        public DateTime RegDate { get; set; }

        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }

        public string Position { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsSuperUser { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSystemAdmin
        {
            get { return _isSystemAdmin; }
            set
            {
                this._isSystemAdmin = value;

                if (value == true)
                {
                    this.IsAdmin = true;
                    this.IsSuperUser = true;
                }
            }
        }
        public string PrivateStoragePath { get; set; }

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

        #endregion

        #region DataAccess members

        public static User Pull(int id)
        {
            try { var entity = DataInterface<User>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<User> List
        {
            get
            {
                try { return DataInterface<User>.List; }
                catch { return null; }
            }
        }

        public bool Push()
        {
            try
            {
                if (this.ID == 0)
                {
                    DataInterface<User>.Push(this);

                    DataInterface<Link>.
                        Push(new Link(this.ID, this.Username, "User"));
                }
                else if (this.Username != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.Username;
                    link.Push();

                    DataInterface<User>.Push(this);
                }
                else
                {
                    DataInterface<User>.Push(this);
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool Delete()
        {
            try
            {
                var link = Link.GetLink(this);

                DataInterface<User>.Delete(this.ID);

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
            "Username",
            "FirstName",
            "SecondName",
            "LastName",
            "Position",
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
                case "Username":
                    error = ValidateUsername();
                    break;

                case "FirstName":
                    error = ValidateFirstName();
                    break;

                case "SecondName":
                    error = ValidateSecondName();
                    break;

                case "LastName":
                    error = ValidateLastName();
                    break;

                case "Position":
                    error = ValidatePosition();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateUsername()
        {
            if (String.IsNullOrWhiteSpace(Username))
            {
                return "Укажите имя пользователя";
            }

            return null;
        }
        private string ValidateFirstName()
        {
            if (String.IsNullOrWhiteSpace(FirstName))
            {
                return "Укажите имя сотрудника";
            }

            return null;
        }
        private string ValidateSecondName()
        {
            if (String.IsNullOrWhiteSpace(SecondName))
            {
                return "Укажите отчество сотрудника";
            }

            return null;
        }
        private string ValidateLastName()
        {
            if (String.IsNullOrWhiteSpace(LastName))
            {
                return "Укажите фамилию сотрудника";
            }

            return null;
        }
        private string ValidatePosition()
        {
            if (String.IsNullOrWhiteSpace(Position))
            {
                return "Укажите должность сотрудника";
            }

            return null;
        } 

        #endregion

        #region Helping properties (NOT MAPPED)

        [NotMapped]
        public string LocalizedStringRegDate
        {
            get { return RegDate.ToString("dd-MM-yyyy"); }
        }
            
        public string VinPadegPersonName
        {
            get
            {
                string output = string.Empty;
                var ending = LastName.Substring(LastName.Length - 1);

                switch (ending)
                {
                    case "а":
                        output = LastName.Substring(0, LastName.Length - 1) + @"ой " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                    case "о":
                        output = LastName + " " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                    default:
                        output = LastName + @"а " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                }
                return output;
            }
        }

        public string DatPadegPersonName
        {
            get
            {
                string output = string.Empty;
                var ending = LastName.Substring(LastName.Length - 1);

                switch (ending)
                {
                    case "а":
                        output = LastName.Substring(0, LastName.Length - 1) + @"ой " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                    case "о":
                        output = LastName + " " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                    default:
                        output = LastName + @"у " + FirstName.Substring(0, 1) + ". " + SecondName.Substring(0, 1) + ".";
                        break;
                }
                return output;
            }
        }

        #endregion
    }
}

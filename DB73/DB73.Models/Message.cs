namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class Message : IDataErrorInfo
    {
        #region Fields

        private List<User> _userList;
        private List<Link> _linkList;

        #endregion

        #region Properties

        public int ID { get; set; }

        public int SenderID { get; set; }

        public virtual List<Link> LinkList
        {
            get
            {
                if (_linkList == null)
                    _linkList = new List<Link>();
                return _linkList;
            }
            set
            {
                if (_linkList == null)
                    _linkList = new List<Link>();
                if (_linkList != value)
                    _linkList = value;
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

        public string MessageTitle { get; set; }
        public string MessageBody { get; set; }

        public DateTime SendDate { get; set; }
        public DateTime RecivedDate { get; set; }

        public string ReadersString { get; set; }

        #endregion

        #region DataAccess members

        public static Message Pull(int id)
        {
            try { var entity = DataInterface<Message>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<Message> List
        {
            get { return DataInterface<Message>.List; }
        }

        public bool Push()
        {
            try
            {
                if (this.ID == 0)
                {
                    DataInterface<Message>.Push(this);

                    DataInterface<Link>.
                        Push(new Link(this.ID, this.MessageTitle, "Message"));
                }
                else if (this.MessageTitle != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.MessageTitle;
                    link.Push();

                    DataInterface<Message>.Push(this);
                }
                else
                {
                    DataInterface<Message>.Push(this);
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

                DataInterface<Message>.Delete(this.ID);

                link.Delete();

                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion

        #region Helping properties NOT MAPPED 

        public bool IsRead
        {
            get { return SendDate == RecivedDate ? false : true; }
        }

        public string LocalizedSendDate
        {
            get
            {
                return SendDate.ToString("dd-MM-yyyy в HH-mm");
            }          
        }
        public string LocalizedRecivedDate
        {
            get
            {
                return RecivedDate.ToString("dd-MM-yyyy в HH-mm");
            }
        }
        
        public string Sender
        {
            get
            {
                if (SenderID == 0) return null;

                var sender = User.Pull(SenderID);
                return sender.FirstName + " " + sender.LastName;
            }
        }
        public string Recipient
        {
            get
            {
                if (this.ID == 0) return null;

                var entity = Message.Pull(this.ID);

                return entity.UserList.Count == 1 ?
                    entity.UserList.First().FirstName + " " + entity.UserList.First().LastName :
                    "Несколько получателей";
            }
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
            "MessageTitle",
            "MessageBody",
            "UserList"
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
                case "MessageTitle":
                    error = ValidateMessageTitle();
                    break;

                case "MessageBody":
                    error = ValidateMessageBody();
                    break;

                case "UserList":
                    error = ValidateMessageUserList();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateMessageTitle()
        {
            if (String.IsNullOrWhiteSpace(MessageTitle))
            {
                return "Не указана тема сообщения";
            }

            return null;
        }
        private string ValidateMessageBody()
        {
            if (String.IsNullOrWhiteSpace(MessageBody))
            {
                return "Основное поле сообщения не должно быть пустым";
            }

            return null;
        }
        private string ValidateMessageUserList()
        {
            if (UserList.Count == 0)
            {
                return "У сообщения должен быть хотя бы один адресат";
            }

            return null;
        }

        #endregion

        #region Helping methods

        public static List<Message> GetInboxList(User user)
        {
            var inbox = from item in List
                        where Message.Pull(item.ID).UserList.FindAll(u => u.ID == user.ID).Count != 0
                        select item;

            var output = inbox.ToList();

            output.Reverse();

            return output;
        }

        public static List<Message> GetOutboxList(User user)
        {
            var output = List.FindAll(l => l.SenderID == user.ID);

            output.Reverse();

            return output;
        }

        public bool IsDeliveryRead(User user)
        {
            string data = ReadersString.Split(' ').ToList().Find(s => s.Contains(user.Username));           

            if (data != null)
                return false;

            return true;
        }

        #endregion
    }
}

namespace DB73.ViewModels
{
    using DB73.Helpers;
    using DB73.Models;
    using DB73.BL;

    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class NewMessageViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        #region Constructors

        // for new message
        public NewMessageViewModel(WorkspaceViewModel requester, Window windowInstance)
        {
            Requester = requester;
            WindowInstance = windowInstance;
            NewMessage = new Message();
        }

        // for send links with message function
        public NewMessageViewModel(Window windowInstance, ObservableCollection<Link> links)
        {
            MessageLinks = new ObservableCollection<Link>();
            NewMessage = new Message();

            foreach (var l in links)
            {
                MessageLinks.Add(l);
            }
            WindowInstance = windowInstance;

            OnPropertyChanged("MessageLinks");
        }

        // for resend function
        public NewMessageViewModel(WorkspaceViewModel requester, Window windowInstance, Message message, string param)
            : this(requester, windowInstance)
        {
            switch (param)
            {
                case "resend":
                    NewMessage.MessageTitle = message.MessageTitle;
                    NewMessage.MessageBody = message.MessageBody;

                    if (MessageLinks == null) MessageLinks = new ObservableCollection<Link>();
                    foreach (var link in message.LinkList)
                    {
                        var item = Link.Pull(link.ID);
                        MessageLinks.Add(item);
                    }
                    break;

                case "reply":

                    if (MessageUserList == null) MessageUserList = new ObservableCollection<User>();

                    MessageUserList.Add(User.Pull(message.SenderID));

                    NewMessage.MessageTitle = "re: " + message.MessageTitle; 
                    NewMessage.MessageBody = "re: " + message.MessageTitle +
                        "\n-------от " + message.LocalizedSendDate + "-------\n";

                    break;
            }

            RefreshView();     
        }

        #endregion

        public override void OnRequestClose()
        {
            WindowInstance.Close();
            
            if (Requester != null)
            Requester.RefreshView();
        }

        #region Properties

        public Message NewMessage { get; set; }

        // ref to the window instance
        private Window WindowInstance { get; set; }

        // ref to the requesting workspace 
        public WorkspaceViewModel Requester { get; private set; }

        // links to send with the message
        public ObservableCollection<Link> MessageLinks { get; set; }

        // list of all the available links in the system and search stuff
        private string _systemLinksSearchString;
        public string SystemLinksSearchString
        {
            get { return _systemLinksSearchString; }
            set
            {
                if (_systemLinksSearchString != value)
                {
                    _systemLinksSearchString = value;
                    OnPropertyChanged("FilteredLinks");
                }
                return;
            }
        }

        public ObservableCollection<Link> FilteredLinks
        {
            get
            {
                if (SystemLinksSearchString == string.Empty || SystemLinksSearchString == null)
                    return new ObservableCollection<Link>(AllLinks);

                var output = from item in AllLinks
                             where item.LinkedName.ToLower().Contains(SystemLinksSearchString.ToLower()) ||
                                   item.LinkedID.ToString().Contains(SystemLinksSearchString)
                             select item;
                return new ObservableCollection<Link>(output);
            }
        }
        public List<Link> AllLinks
        {
            get
            {
                var links = from link in Link.List
                            where link.LinkedType != "User" &&
                                  link.LinkedType != "Message" &&
                                  !(link.LinkedType == "Folder" || link.LinkedType == "Document")
                            select link;

                return links.ToList();
            }
        }

        // selected link property
        public Link SelectedLink { get; set; }

        // selected user property
        public User SelectedUser { get; set; }

        // list of users to recieve to message
        public ObservableCollection<User> MessageUserList { get; set; }

        // list of all available recipients
        public ObservableCollection<User> AllUserList
        {
            get { return new ObservableCollection<User>(User.List); }
        }

        #endregion

        #region Commads

        // send message command
        public ICommand SendMessage { get { return new RelayCommand(p => OnSendMessage()); } }
        private void OnSendMessage()
        {
            if (MessageLinks == null) MessageLinks = new ObservableCollection<Link>();

            NewMessage.UserList = MessageUserList.ToList();
            NewMessage.LinkList = MessageLinks.ToList();

            var response = MessagesLogic.SendMessage(NewMessage);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
                this.OnRequestClose();
        }

        // add link to message package
        public ICommand AddLink { get { return new RelayCommand(p => OnAddLink()); } }
        private void OnAddLink()
        {
            if (SelectedLink == null) return;
            if (MessageLinks == null) MessageLinks = new ObservableCollection<Link>();

            if (MessageLinks.Where(l => l.ID == SelectedLink.ID).Count() != 0)
            {
                UIMessager.ShowMessage("Ссылка на данный объект уже прикреплена к сообщению");
                return;
            }

            MessageLinks.Add(SelectedLink);
            OnPropertyChanged("MessageLinks");
        }

        // remove link from message package
        public ICommand RemoveLink { get { return new RelayCommand(p => OnRemoveLink()); } }
        private void OnRemoveLink()
        {
            if (SelectedLink == null) return;

            MessageLinks.Remove(SelectedLink);
            OnPropertyChanged("MessageLinks");
        }

        // add user to message user list
        public ICommand AddUser { get { return new RelayCommand(p => OnAddUser()); } }
        private void OnAddUser()
        {
            if (SelectedUser == null) return;
            if (MessageUserList == null) MessageUserList = new ObservableCollection<User>();

            if (MessageUserList.Where(l => l.ID == SelectedUser.ID).Count() != 0)
            {
                UIMessager.ShowMessage("Данный пользователь уже находится в списке адресатов");
                return;
            }

            MessageUserList.Add(SelectedUser);
            OnPropertyChanged("MessageUserList");
        }

        // remove user from message user list
        public ICommand RemoveUser { get { return new RelayCommand(p => OnRemoveUser()); } }
        private void OnRemoveUser()
        {
            if (SelectedUser == null) return;

            MessageUserList.Remove(SelectedUser);
            OnPropertyChanged("MessageUserList");
        }

        // on link double click
        public void OnLinkDoubleClick()
        {
            if (SelectedLink == null) return;
            LinkViewCaster.ShowLink(SelectedLink);
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
            "MessageUserList"
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
                case "MessageUserList":
                    error = ValidateMessageUserList();
                    break;

                default: break;
            }

            return error;
        }

        private string ValidateMessageUserList()
        {
            if (MessageUserList == null) MessageUserList = new ObservableCollection<User>();
            if (MessageUserList.Count == 0)
            {
                return "У сообщения должен быть хотя бы один адресат";
            }

            return null;
        }

        #endregion
    }
}

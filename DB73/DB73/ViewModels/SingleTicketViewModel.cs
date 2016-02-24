namespace DB73.ViewModels
{
    using DB73.BL;
    using DB73.Models;
    using DB73.Helpers;

    using System.Windows.Input;

    public class SingleTicketViewModel : WorkspaceViewModel
    {
        public SingleTicketViewModel(BugTicket ticket)
        {
            Ticket = ticket;
        }

        public SingleTicketViewModel(BugTicket ticket, BugTicketsViewModel requester)
        {
            Ticket = ticket;
            Requester = requester;
        }

        public override void OnRequestClose()
        {
            if (Requester != null)
                Requester.RefreshView();
            SatteliteWindow.CloseSatteliteWindow();
        }

        public string WorkspaceTitle
        {
            get { return "Заявка №" + Ticket.ID.ToString(); }
        }

        private BugTicketsViewModel Requester { get; set; }
        public BugTicket Ticket { get; set; }

        public string Sender
        {
            get
            {
                var user = User.Pull(Ticket.AdderID);
                return user.FirstName + " " + user.LastName;
            }
        }
        public string Closer
        {
            get
            {
                if (Ticket.IsClosed)
                {
                    var user = User.Pull(Ticket.EditorID);

                    return user.FirstName + " " + user.LastName;
                }

                return "Заявка еще не обработана.";
            }
        }

        public bool IsDeleteVisible
        {
            get
            {
                if (Ticket.IsClosed) return false;
                return Ticket.AdderID == Session.ActiveUser.ID ? true : false;
            }
        }
        public bool CanCloseTicket
        {
            get
            {
                if (Ticket.IsClosed) return false;
                return Session.ActiveUser.IsAdmin == true ? true : false;
            }
        }

        public ICommand DeleteTicket
        {
            get
            {   
                return new RelayCommand(parameter => OnDeleteTicket(parameter));
            }
        } 
        private void OnDeleteTicket(object parameter)
        {
            if (!UIMessager.DeleteConfirmationDialog()) return;

            var response = BugTicketsLogic.DeleteTicket(Ticket);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
            {
                this.OnRequestClose();             
            }
        }

        public ICommand CloseTicket
        {
            get
            {
                return new RelayCommand(parameter => OnCloseTicket(parameter));
            }
        }
        private void OnCloseTicket(object parameter)
        {
            var response = BugTicketsLogic.CloseTicket(Ticket);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone)
            {
                this.OnRequestClose();
            }
        }
    }
}

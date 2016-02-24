namespace DB73.ViewModels
{
    using DB73.Models;
    using DB73.BL;
    using DB73.Helpers;

    using System.Windows.Input;

    public class NewUserViewModel : WorkspaceViewModel
    {
        public NewUserViewModel(WorkspaceViewModel requester)
        {
            this.Requester = requester;
            this.NewUser = new User();
        }

        public override void OnRequestClose()
        {
            Requester.RefreshView();
            SatteliteWindow.CloseSatteliteWindow();
        }

        public string WorkspaceTitle
        {
            get { return "Новый пользователь"; }
        }

        public WorkspaceViewModel Requester { get; set; }

        public User NewUser { get; set; }

        public ICommand AddUser
        {
            get
            {
                return new RelayCommand(parameter => OnAddUser(parameter));
            }
        }
        private void OnAddUser(object parameter)
        {
            NewUser.Username = NewUser.Username.Trim();

            if (User.List.FindAll(u => u.Username.ToLower() == NewUser.Username.ToLower()).Count != 0)
            {
                UIMessager.ShowMessage("Пользователь с тамим именем уже существует в системе. Выберите другое имя пользователя");
                return;
            }

            var response = AdminTools.AddUser(NewUser);

            UIMessager.ShowMessage(response.Message);

            if (response.IsDone) this.OnRequestClose();
        }
    }
}
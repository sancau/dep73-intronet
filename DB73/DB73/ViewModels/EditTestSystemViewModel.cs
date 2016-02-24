namespace DB73.ViewModels
{
    using DB73.Models;
    using DB73.BL;
    using DB73.Helpers;

    public class EditTestSystemViewModel : WorkspaceViewModel
    {
        public EditTestSystemViewModel(WorkspaceViewModel requester)
        {
            Requester = requester;
        }

        public WorkspaceViewModel Requester { get; set; }

        public override void OnRequestClose()
        {
            Requester.RefreshView();
            SatteliteWindow.CloseSatteliteWindow();
        }
    }
}


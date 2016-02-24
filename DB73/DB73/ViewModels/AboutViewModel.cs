namespace DB73.ViewModels
{
    using DB73.Models;

    public class AboutViewModel : WorkspaceViewModel
    {
        public string WorkspaceTitle
        {
            get { return "О программе"; }
        }

        public string About { get { return AppConfig.ApplicationCredits; } }
    }
}

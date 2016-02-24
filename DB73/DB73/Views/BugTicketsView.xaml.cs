namespace DB73.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for BugTicketsView.xaml
    /// </summary>
    public partial class BugTicketsView : UserControl
    {
        public BugTicketsView()
        {
            InitializeComponent();
        }

        private void BugTicketsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).OnShowTicket();
            }
        }
    }
}

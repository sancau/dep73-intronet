

namespace DB73.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;
    /// <summary>
    /// Логика взаимодействия для AdminToolsView.xaml
    /// </summary>
    public partial class AdminToolsView : UserControl
    {
        public AdminToolsView()
        {
            InitializeComponent();
        }

        private void UserListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).OnShowUser();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DB73.Views
{
    /// <summary>
    /// Логика взаимодействия для PasteDialogView.xaml
    /// </summary>
    public partial class PasteDialogView : Window
    {
        public PasteDialogView()
        {
            InitializeComponent();
        }

        private void Link_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (((dynamic)this.DataContext).SelectedLink == null)
                    return;
                else
                    ((dynamic)this.DataContext).OnLinkDoubleClick();
            }
        }
    }
}

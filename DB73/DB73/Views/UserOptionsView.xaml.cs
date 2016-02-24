namespace DB73.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Логика взаимодействия для UserOptionsView.xaml
    /// </summary>
    public partial class UserOptionsView : UserControl
    {
        public UserOptionsView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password.GetHashCode(); }
	    
            if (((PasswordBox)sender).Password.Length > 0)
            {
                this.PasswordValidatorDisplay.BorderBrush = Brushes.Transparent;
            }
            else
            {
                this.PasswordValidatorDisplay.BorderBrush = Brushes.Red;
            }	
        }

        private void PasswordBoxNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).NewPassword = ((PasswordBox)sender).Password.GetHashCode(); }

            if (((PasswordBox)sender).Password.Length > 0)
            {
                this.PasswordValidatorDisplayNew.BorderBrush = Brushes.Transparent;
            }
            else
            {
                this.PasswordValidatorDisplayNew.BorderBrush = Brushes.Red;
            }
        }

        private void PasswordBoxNewCopy_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).NewPasswordCopy = ((PasswordBox)sender).Password.GetHashCode(); }

            if (((PasswordBox)sender).Password.Length > 0)
            {
                this.PasswordValidatorDisplayCopy.BorderBrush = Brushes.Transparent;
            }
            else
            {
                this.PasswordValidatorDisplayCopy.BorderBrush = Brushes.Red;
            }
        }
    }
}

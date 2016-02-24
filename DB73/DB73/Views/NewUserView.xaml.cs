﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DB73.Views
{
    /// <summary>
    /// Логика взаимодействия для NewUserView.xaml
    /// </summary>
    public partial class NewUserView : Window
    {
        public NewUserView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).NewUser.Password = ((PasswordBox)sender).Password.GetHashCode();
            }

            if (((PasswordBox)sender).Password.Length > 0)
            {
                this.PasswordValidatorDisplay.BorderBrush = Brushes.Transparent;
            }
            else
            {
                this.PasswordValidatorDisplay.BorderBrush = Brushes.Red;
            }
        }
    }
}

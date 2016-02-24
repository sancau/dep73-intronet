namespace DB73.Views
{
    using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите завершить текущую сессию?", "Выход из программы", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }

            if (DB73.ViewModels.DocumentsViewModel.EditProcess != null && e.Cancel == false)
            {
                System.Windows.Forms.MessageBox.Show("Обнаружена активная сессия редактирования! \nЗакройте редактируемый файл!", "Сессия не может быть завершена!");
                e.Cancel = true;
            }

            if (DB73.ViewModels.MainWindowViewModel.OpenedWindows.Count != 0)
            {
                System.Windows.Forms.MessageBox.Show("Обнаружены открытые дополнительные окна программы. Завершите работу с этими окнами");
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }
}

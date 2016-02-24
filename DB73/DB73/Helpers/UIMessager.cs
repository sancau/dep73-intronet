namespace DB73.Helpers
{
    public static class UIMessager
    {
        public static void ShowMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        public static void ShowMessage(string message, string caption)
        {
            System.Windows.MessageBox.Show(message, caption);
        }

        public static bool DeleteConfirmationDialog()
        {
            System.Windows.MessageBoxResult result =
                System.Windows.MessageBox.
                            Show("Внимание! Подтвердите удаление!",
                                "Необратимая операция!",
                                System.Windows.MessageBoxButton.YesNo);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PrivateStorageReplaceConfirmationDialog()
        {
            System.Windows.MessageBoxResult result =
                System.Windows.MessageBox.
                            Show("Копия данного документа уже имеется в Вашем личном хранилище. Перезаписать файл?",
                                "Необратимая операция!",
                                System.Windows.MessageBoxButton.YesNo);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void UnderDevMessage()
        {
            string underdev = "Функционал в разработке! Спасибо за понимание!";

            UIMessager.ShowMessage(underdev);
        }
    }
}

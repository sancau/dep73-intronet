namespace DB73.BL
{
    using System.Collections.Generic;

    public static class UIMessageBuilder
    {
        //In our case it's customized to product russian language messages
        public static string Locale { get { return "rus"; } }

        //Returns a certain dictionary for chosen locale
        private static Dictionary<string, string> GetDictionary(string locale)
        {
            switch (locale)
            {
                case "rus": return RussianMessageDictionary;
                case "eng": return EnglishMessageDictionary;
                default: return null;
            }
        }

        //Returns a message by a caseKey
        public static string GetMessage(string caseKey)
        {
            var dict = GetDictionary(Locale);

            return dict.ContainsKey(caseKey)
                ? dict[caseKey] : "message not found, check UI dictionary";
        }

        //Russian language UI message dictionary
        public static Dictionary<string, string> RussianMessageDictionary
        {
            get
            {
                var dict = new Dictionary<string, string>();

                dict.Add(true.ToString(), "Успешно");
                dict.Add(false.ToString(), "Команда не была выполнена");

                dict.Add("error_on_db_preparations", "Ошибка первичной инициализации БД");

                dict.Add("invalid_data", "Данные некорректны или неполны");
                dict.Add("data_saved", "Данные были успешно обновлены");

                dict.Add("exception", "Ошибка программы! Исключение.");
                dict.Add("config_file_error", "Ошибка конфигурационного файла");

                dict.Add("ticket_added", "Ваша заявка отправлена и будет рассмотрена в ближайшее время");
                dict.Add("ticket_deleted", "Заявка была успешно удалена");
                dict.Add("ticket_closed", "Статус заявки изменен");

                dict.Add("no_db_connection", "Не удалось установить соединение с базой данных Обратитесь к администратору");
                dict.Add("no_such_user", "Пользователь с таким именем не найден");
                dict.Add("incorrect_password", "Неверный пароль");
                dict.Add("user_is_online", "Пользователь уже находится в системе. Завершите все активные сессии и попробуйте снова");
                dict.Add("error_on_session_start", "Ошибка БД при старте сессии");
                dict.Add("error_on_session_end", "Ошибка БД при завершении сессии");
                dict.Add("login_init_error", "Ошибка инициализации при попытке начала сессии");
                dict.Add("error_on_login_proccessing", "Ошибка метода обработчика сессий!");

                dict.Add("on_delete_active_user", "Вы не можете удалить собственную учетную запись");
                dict.Add("user_added", "Пользователь добавлен");
                dict.Add("user_deleted", "Пользователь был удален");

                dict.Add("password_changed", "Пароль был успешно изменен");
                dict.Add("private_storage_set", "Папка личного хранилища установлена");
                dict.Add("no_such_directory", "Указанная папка не найдена");

                #region Documents 

                dict.Add("file_not_found", "Указанный файл не найден!");
                dict.Add("file_already_exists", "Документ с таким именем уже существует в базе данных!");
                dict.Add("copy_error", "Ошибка при копировании файла!");
                dict.Add("document_added", "Документ успешно добавлен!");
                dict.Add("folder_added", "Папка успешно добавлена!");
                dict.Add("document_deleted", "Документ был удален!");
                dict.Add("error_on_document_delete", "Ошибка при попытке удалить документ!");
                dict.Add("error_on_folder_delete", "Ошибка при попытке удалить папку!");
                dict.Add("folder_deleted", "Папка была удалена!");
                dict.Add("document_copied", "Документ был успешно скопирован!");
                dict.Add("folder_copied", "Папка была успешна скопирована!");
                dict.Add("document_moved", "Документ перемещен!");
                dict.Add("folder_moved", "Папка перемещена!");
                dict.Add("tree_imported", "Дерево документов успешно импортировано!");
                dict.Add("tree_exported", "Дерево документов успешно экспортировано!");
                dict.Add("invalid_file_type", "Тип указанного файла не совпадает с типом документа!");
                dict.Add("file_replaced", "Содержимое файла было успешно заменено!");
                dict.Add("copied_to_private", "Копия помещена в Ваше личное хранилище!");
                dict.Add("cant_copy_to_own_child", "Невозможно скопировать папку в её же подпапку!");
                dict.Add("cant_move_to_own_child", "Невозможно переместить папку в её же подпапку!");
                dict.Add("document_is_not_free", "Документ занят другим пользователем!");
                dict.Add("edit_session_closed", "Сессия редактирования успешно завершена!");
                dict.Add("edit_session_failed", "Ошибка при работе с сессией редактирования!");
                dict.Add("edit_session_started", "Сессия редактирования успешна запущена!");
                dict.Add("error_on_edit_backup", "Ошибка при резервном копировании файла!");

                #endregion

                #region Messages

                dict.Add("message_sended", "Сообщение отправлено");
                dict.Add("error_on_message_send", "Ошибка при отправке сообщения");

                #endregion

                #region Inventory

                dict.Add("error_on_itemfolder_validation", "Некорректные данные вызвали ошибку при создании папки сопуствующей документации");
                dict.Add("error_on_itemfolder_push", "Ошибка при добавлении папки сопуствующей документации в БД");
                dict.Add("error_on_item_push", "Ошибка при записи позиции в БД");
                dict.Add("item_added", "Позиция добавлена");

                #endregion
                
                dict.Add("error_on_backup", "Ошибка при совершении резервного копирования");
                dict.Add("backup_done", "Резервное копирование данных успешно завершено");

                return dict;
            }
        }

        //English language UI message dictionary
        public static Dictionary<string, string> EnglishMessageDictionary
        {
            get
            {
                var dict = new Dictionary<string, string>();

                return dict;
            }
        }
    }
}

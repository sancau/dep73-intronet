namespace DB73.BL
{
    using DB73.Models;
    using System;
    using System.IO;

    public static class UserTools
    { 
        private static User ActiveUser
        {
            get { return Session.ActiveUser; }
        } 

        public static LogicResponse ChangePassword(int oldPass, int newPass)
        {
            try
            {
                var user = User.Pull(ActiveUser.ID);

                if (user.Password != oldPass)
                {
                    return new LogicResponse(false, "incorrect_password");
                }

                else user.Password = newPass;

                user.Push();

                return new LogicResponse(true, "password_changed");
                
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        public static LogicResponse SetPrivateStoragePath(string path)
        {
            try
            {
                var user = User.Pull(ActiveUser.ID);
                
                if (!Directory.Exists(path))
                {
                    return new LogicResponse(false, "no_such_directory");
                }

                user.PrivateStoragePath = path;
                user.Push();
                return new LogicResponse(true, "private_storage_set");
            }
            catch(Exception ex)
            {
                return new LogicResponse(false, "exception", ex.Message);
            }
        }
    }
}

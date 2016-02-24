namespace DB73.BL
{
    using DB73.Models;

    using System;

    //Logic for administration functions
    public static class AdminTools
    {
        public static LogicResponse AddUser(User user)
        {
            if (!user.IsValid) return new LogicResponse(false, "invalid_data");

            try
            {
                user.RegDate = Server.CurrentTime;
                user.Push();

                return new LogicResponse(true, "user_added");
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }
        public static LogicResponse SaveUserChanges(User user)
        {
            if (!user.IsValid) return new LogicResponse(false, "invalid_data");

            try
            {
                var userEntity = User.Pull(user.ID);

                userEntity.Username = user.Username;
                userEntity.Password = user.Password;
                userEntity.FirstName = user.FirstName;
                userEntity.SecondName = user.SecondName;
                userEntity.LastName = user.LastName;
                userEntity.Position = user.Position;
                userEntity.IsAdmin = user.IsAdmin;
                userEntity.IsSuperUser = user.IsSuperUser;
          
                userEntity.Push();

                return new LogicResponse(true, "data_saved");
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }
        public static LogicResponse DeleteUser(User user)
        {
            try
            {
                if (user.ID == Session.ActiveUser.ID)
                {
                    return new LogicResponse(false, "on_delete_active_user");
                }

                var userEntity = User.Pull(user.ID);

                userEntity.Delete();

                return new LogicResponse(true, "user_deleted");
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }
    }
}

namespace DB73.BL
{
    //Handles user login/logout procedures, provides information on active user, etc
    using DB73.Models;
    using System.IO;
    using System.Timers;

    public static class Session
    {
        //Global property
        public static User ActiveUser { get; private set; }

        //refreshes user entity 
        private static void RefreshUser(object sender, ElapsedEventArgs e)
        {
            ActiveUser = User.Pull(ActiveUser.ID);
        }

        //Database connection check
        public static LogicResponse InitializeDBConnection()
        {
            try
            {
                if (User.List == null)
                {
                    return new LogicResponse(false, "no_db_connection");
                }
                return new LogicResponse(true);
            }
            catch
            {
                return new LogicResponse(false, "login_init_error");
            }
        }

        //Login logic
        public static LogicResponse OnLoginTry(string username, int password)
        {
            try
            {
                if (Folder.List.Count == 0)
                {
                    if (!AppConfig.OnFirstRun())
                        return new LogicResponse(false, "error_on_db_preparations");
                }
               
                if (User.List.Count == 0)
                {
                    var firstUser = new User();
                    firstUser.Username = username;
                    firstUser.Password = password;
                    firstUser.RegDate = Server.CurrentTime;
                    firstUser.IsSystemAdmin = true;

                    firstUser.Push();
                    return StartSession(User.Pull(1));
                }

                var user = User.List.Find(u => u.Username == username);

                if (user == null)
                {
                    return new LogicResponse(false, "no_such_user");
                }

                else if (user.Password == password)
                {
                    return StartSession(user);
                }

                else
                {
                    return new LogicResponse(false, "incorrect_password");
                }
            }
            catch
            {
                return new LogicResponse(false, "error_on_login_proccessing");
            }
       
        }

        private static LogicResponse StartSession(User user)
        {
            try
            {
                user = User.Pull(user.ID);
                if (user.IsOnline)
                    //return new LogicResponse("user_is_online", false);

                user.IsOnline = true;
                Session.ActiveUser = user;
                user.Push();

                //initilize data refreshing
                Timer timer = new Timer();
                timer.Elapsed += new ElapsedEventHandler(RefreshUser);
                timer.Interval = 1000;
                timer.Start();

                return new LogicResponse(true);
            }
            catch
            {
                return new LogicResponse(false, "error_on_session_start");
            }
        }

        public static LogicResponse EndSession()
        {
            try
            {
                var flow = new DirectoryInfo(DocumentManager.FlowPath);

                if (flow.Exists)
                {
                    flow.Delete(true);
                }

                var user = User.Pull(ActiveUser.ID);
                user.IsOnline = false;
                user.Push();
                return new LogicResponse(true);
            }
            catch
            {
                return new LogicResponse(false, "error_on_session_end");
            }
        }
    }
}
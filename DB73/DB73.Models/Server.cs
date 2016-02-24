namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Linq;

    public static class Server
    {
        public static string ConnectionString
        {
            get { return ServerAccess.GetCurrentConnectionString(); }
        }

        public static DateTime CurrentTime
        {
            get { return ServerAccess.CurrentTime; }
        }

        public static string DatabaseName
        {
            get
            {
                var tokens = ConnectionString.Split(' ');

                var nameContainer = tokens.Where(str => str.Contains("Catalog=")).First();
                nameContainer = nameContainer.Replace("Catalog=", string.Empty);

                var lastIndexOfName = nameContainer.IndexOf(';');

                var name = nameContainer.Remove(lastIndexOfName);

                return name;
            }
        }
    }
}

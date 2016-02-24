namespace DB73.Models.DataAccess
{
    using System.Data.Entity;

    //This is the context class to use with Entity Framework
    public class Context : DbContext
    {
        public Context()
            // define our connection strings
            //: base("name=DB73_PROTOTYPE") 
            : base("name=DB73_ONDUTY_IMPORTANT_DATA")
            // if none of them is active we're using Local DB
        {
        }

        public DbSet<Link> Links { get; set; }

        public DbSet<BugTicket> BugTickets { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<Folder> Folders { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<InventoryItem> InventoryItems { get; set; }
    }
}

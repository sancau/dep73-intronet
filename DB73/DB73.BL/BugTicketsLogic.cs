namespace DB73.BL
{
    using DB73.Models;

    using System;

    public static class BugTicketsLogic
    {
        public static LogicResponse AddTicket(BugTicket ticket)
        {
            if (!ticket.IsValid) return new LogicResponse(false, "invalid_data"); 

            try
            {
                ticket.AddDate = Server.CurrentTime;
                ticket.EditDate = ticket.AddDate;

                ticket.Push();

                return new LogicResponse(true, "ticket_added");
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        public static LogicResponse DeleteTicket(BugTicket ticket)
        {
            try
            {
                var ticketEntity = BugTicket.Pull(ticket.ID);

                ticketEntity.Delete();

                return new LogicResponse(true, "ticket_deleted");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        public static LogicResponse CloseTicket(BugTicket ticket)
        {
            try
            {
                var ticketEntity = BugTicket.Pull(ticket.ID);

                ticketEntity.EditDate = Server.CurrentTime;
                ticketEntity.EditorID = Session.ActiveUser.ID;
                ticketEntity.Resolution = ticket.Resolution;
                ticketEntity.IsClosed = true;

                ticketEntity.Push();

                return new LogicResponse(true, "ticket_closed");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }
    }
}

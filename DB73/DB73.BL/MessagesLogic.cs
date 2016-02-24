namespace DB73.BL
{
    using DB73.Models;

    public static class MessagesLogic
    {
        public static LogicResponse SendMessage(Message message)
        {
            if (!message.IsValid)
            {
                return new LogicResponse(false, "invalid_data");
            }

            try
            {
                if (message.ReadersString == null) message.ReadersString = string.Empty;
                foreach (var user in message.UserList)
                {
                    message.ReadersString += user.Username + " ";
                }

                message.SenderID = Session.ActiveUser.ID;
                message.SendDate = Server.CurrentTime;
                message.RecivedDate = message.SendDate;

                if (message.Push())
                    return new LogicResponse(true, "message_sended");
                else
                    return new LogicResponse(false, "error_on_message_send");
            }
            catch
            {
                return new LogicResponse(false, "exception");
            }
        }
    }
}

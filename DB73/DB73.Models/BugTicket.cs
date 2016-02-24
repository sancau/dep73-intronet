namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class BugTicket : IDataErrorInfo
    {
        #region Properties

        public int ID { get; set; }

        public DateTime AddDate { get; set; }
        public DateTime EditDate { get; set; }

        public int AdderID { get; set; }
        public int EditorID { get; set; }

        public string TicketTitle { get; set; }
        public string TicketBody { get; set; }

        public string Resolution { get; set; }

        public bool IsClosed { get; set; }

        #endregion

        #region DataAccess members

        public static BugTicket Pull(int id)
        {
            try { var entity = DataInterface<BugTicket>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<BugTicket> List
        {
            get { return DataInterface<BugTicket>.List; }
        }

        public bool Push()
        {                     
            try
            {
                if (this.ID == 0)
                {
                    DataInterface<BugTicket>.Push(this);

                    DataInterface<Link>.
                        Push(new Link(this.ID, this.TicketTitle, "BugTicket"));
                }
                else if (this.TicketTitle != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.TicketTitle;
                    link.Push();
                    
                    DataInterface<BugTicket>.Push(this);
                }
                else
                {
                    DataInterface<BugTicket>.Push(this);
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool Delete()
        {
            try
            {
                var link = Link.GetLink(this);
                
                DataInterface<BugTicket>.Delete(this.ID);

                link.Delete();                

                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }
        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

        #endregion

        #region Validation

        static readonly string[] ValidatedProperties =
        {
            "TicketTitle",
            "TicketBody"
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)

                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }
        private string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "TicketTitle":
                    error = ValidateTicketTitle();
                    break;

                case "TicketBody":
                    error = ValidateTicketBody();
                    break;

                default: break;
            }

            return error;
        }
        private string ValidateTicketTitle()
        {
            if (String.IsNullOrWhiteSpace(TicketTitle))
            {
                return "Не указана тема заявки";
            }

            return null;
        }
        private string ValidateTicketBody()
        {
            if (String.IsNullOrWhiteSpace(TicketBody))
            {
                return "Основное поле заявки не должно быть пустым";
            }

            return null;
        }

        #endregion

        #region Helping properties (NOT MAPPED)

        [NotMapped]
        public string LocalizedStringAddDate
        {
            get { return AddDate.ToString("dd-MM-yyyy"); }
        }

        [NotMapped]
        public string LocalizedStringEditDate
        {
            get
            {
                if (AddDate == EditDate)
                {
                    return null;
                }
                return EditDate.ToString("dd-MM-yyyy");
            }
        }

        #endregion
    }
}

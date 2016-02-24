namespace DB73.Models
{
    using DB73.Models.DataAccess;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class InventoryItem : IDataErrorInfo, INotifyPropertyChanged
    {
        #region Basic Inventory Item Properties

        public int ID { get; set; }

        public string ItemType { get; set; }

        public string Name { get; set; }

        public DateTime AddDate { get; set; }

        public DateTime EditDate { get; set; }

        public int AdderID { get; set; }

        public int EditorID { get; set; }

        public string Code { get; set; }

        public string InventoryNumber { get; set; }

        public string Unit { get; set; }

        private string _price;
        public string Price
        {
            get { return _price; }
            set
            {
                value = value.Trim().Replace('.', ',');

                if (_price == value) return;
                _price = value;

                if (_price != null && Quantity != 0 && _price != "")
                    try
                    {
                        TotalPrice = (double.Parse(_price) * (double)Quantity).ToString("N7");
                    }
                    catch { }
                else
                    TotalPrice = null;
                OnPropertyChanged("TotalPrice");
            }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity == value) return;
                _quantity = value;

                if (_price != null && Quantity != 0 && _price != "")
                    try
                    {
                        TotalPrice = (double.Parse(_price) * (double)Quantity).ToString("N2");
                    }
                    catch
                    {

                    }
                else
                    TotalPrice = null;
                OnPropertyChanged("TotalPrice");
            }
        }

        public string TotalPrice { get; set; }

        public string ActualPlacement { get; set; }

        public int ItemFolderID { get; set; }

        #endregion

        #region Equipment Properties

        public string Purpose { get; set; }

        private DateTime _lastTestDate;
        public DateTime LastTestDate
        {
            get { return _lastTestDate; }
            set
            {
                _lastTestDate = value;
                _nextTestDate = _lastTestDate.AddYears(1);
                OnPropertyChanged("NextTestDate");
                OnPropertyChanged("LocalizedStringNextTestDate");
                OnPropertyChanged("LastTestDate");
                OnPropertyChanged("LocalizedStringLastTestDate");
            }
        }

        private DateTime _nextTestDate;
        public DateTime NextTestDate
        {
            get { return _nextTestDate; }
            set
            {
                _nextTestDate = value;
                OnPropertyChanged("NextTestDate");
                OnPropertyChanged("LocalizedStringNextTestDate");
            }
        }

        public string Manufacturer { get; set; }

        public int YearOfProduction { get; set; }

        public string Specification { get; set; }

        #endregion

        #region Validation

        static readonly string[] ValidatedProperties =
        {
            "Name",
            "AddDate",
            "EditDate",
            "AdderID",
            "EditorID",
            "Code",
            "InventoryNumber",
            "Unit",
            "Price",
            "Quantity",
            "TotalPrice",
            "ActualPlacement",
            "Purpose",
            "LastTestDate",
            "NextTestDate",
            "Manufacturer",
            "YearOfProduction",
            "Specification"
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    // adder id validation FAILED todo
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }
        public string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "Name":
                    error = ValidateName();
                    break;
                case "Code":
                    error = ValidateCode();
                    break;
                case "InventoryNumber":
                    error = ValidateInventoryNumber();
                    break;
                case "Unit":
                    error = ValidateUnit();
                    break;
                case "Price":
                    error = ValidatePrice();
                    break;
                case "Quantity":
                    error = ValidateQuantity();
                    break;
                case "TotalPrice":
                    error = ValidateTotalPrice();
                    break;
                case "ActualPlacement":
                    error = ValidateActualPlacement();
                    break;
                case "AddDate":
                    error = ValidateAddDate();
                    break;
                case "EditDate":
                    error = ValidateEditDate();
                    break;
                case "AdderID":
                    error = ValidateAdderID();
                    break;
                case "ItemFolderID":
                    error = ValidateItemFolderID();
                    break;
                case "Purpose":
                    error = ValidatePurpose();
                    break;
                case "LastTestDate":
                    error = ValidateLastTestDate();
                    break;
                case "NextTestDate":
                    error = ValidateNextTestDate();
                    break;
                case "Manufacturer":
                    error = ValidateManufacturer();
                    break;
                case "YearOfProduction":
                    error = ValidateYearOfProduction();
                    break;
                case "Specification":
                    error = ValidateSpecification();
                    break;
                default: break;
            }

            return error;
        }

        // basic inventory properties validation

        private string ValidateActualPlacement()
        {
            if (String.IsNullOrWhiteSpace(ActualPlacement))
            {
                return "Укажите местоположение";
            }

            return null;
        }

        private string ValidateTotalPrice()
        {
            if (TotalPrice == null || TotalPrice == "") 
            {
                return "Стоймость не задана";
            }                    

            if (float.Parse(TotalPrice) == 0)
            {
                return "Стоймость не может быть равна нулю";
            }

            return null;
        }

        private string ValidateQuantity()
        {
            if (Quantity == 0)
            {
                return "Количество не может быть равно нулю";
            }

            return null;
        }

        private string ValidatePrice()
        {
            if (Price == null)
            {
                return "Стоймость не задана";
            }

            try
            {
                if (float.Parse(Price) == 0)
                {
                    return "Стоймость не может быть равна нулю";
                }
                return null;
            }
            catch
            {
                return "Некорректный ввод для стоймости";
            }
        }

        private string ValidateUnit()
        {
            if (String.IsNullOrWhiteSpace(Unit))
            {
                return "Укажите единицы измерения";
            }

            return null;
        }

        private string ValidateInventoryNumber()
        {
            if (String.IsNullOrWhiteSpace(InventoryNumber))
            {
                return "Укажите инвентарный номер";
            }

            return null;
        }

        private string ValidateCode()
        {
            if (Code == null) return "Код позиции не задан";

            if (Code.ToString().Length != 10)
            {
                return "Код должен содержать ровно 10 цифр";
            }

            return null;
        }

        private string ValidateName()
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return "Укажите наименование";
            }

            if (List.FindAll(d => d.Name.ToLower() == Name.ToLower() && d.ID != this.ID).Count != 0)
            {
                return "Инвентарная позиция с таким именем уже существует в базе. Измените имя позиции";
            }

            return null;
        }

        private string ValidateAddDate()
        {
            if (AddDate.ToString("dd-MM-yyyy") == "01-01-0001")
            {
                return "Дата добавления не указана";
            }

            return null;
        }

        private string ValidateEditDate()
        {
            if (EditDate.ToString("dd-MM-yyyy") == "01-01-0001")
            {
                return "Дата редактирования не указана";
            }

            return null;
        }

        private string ValidateAdderID()
        {
            if (AdderID == 0)
            {
                return "Пользователь добавивший запись не указан";
            }
            return null;
        }

        private string ValidateItemFolderID()
        {
            if (ItemFolderID == 0)
            {
                return "Папка сопутсвующей документации не указана";
            }
            return null;
        }

        // equipment properties validation

        private string ValidatePurpose()
        {
            if (ItemType == "TestSystem" && string.IsNullOrWhiteSpace(Purpose))
            {
                return "Область применения не указана";
            }
            return null;
        }

        private string ValidateLastTestDate()
        {
            if ((ItemType == "TestSystem" || ItemType == "Tool") && LastTestDate.ToString("dd-MM-yyyy") == "01-01-0001")
            {
                return "Дата последней поверки не указана";
            }
            return null;
        }

        private string ValidateNextTestDate()
        {
            if ((ItemType == "TestSystem" || ItemType == "Tool") && NextTestDate.ToString("dd-MM-yyyy") == "01-01-0001")
            {
                return "Срок поверки не указан";
            }
            return null;
        }

        private string ValidateManufacturer()
        {
            if ((ItemType == "TestSystem" || ItemType == "Tool") && string.IsNullOrWhiteSpace(Manufacturer))
            {
                return "Производитель не указан";
            }
            return null;
        }

        private string ValidateYearOfProduction()
        {
            if (ItemType == "TestSystem" || ItemType == "Tool")
            {
                if (YearOfProduction < 2020 && YearOfProduction > 1950)
                {
                    return null;
                }
                else
                {
                    return "Некорректная дата изготовления";
                }
            }
            return null;
        }

        private string ValidateSpecification()
        {
            if ((ItemType == "TestSystem" || ItemType == "Tool") && string.IsNullOrWhiteSpace(Specification))
            {
                return "Производитель не указан";
            }
            return null;
        }

        #endregion

        #region DataAccess Members

        public static InventoryItem Pull(int id)
        {
            try { var entity = DataInterface<InventoryItem>.Pull(id); return entity; }
            catch (Exception) { return null; }
        }

        public static List<InventoryItem> List
        {
            get { return DataInterface<InventoryItem>.List; }
        }

        public bool Push()
        {
            try
            {
                if (this.ID == 0)
                {                                     
                    DataInterface<InventoryItem>.Push(this);
                    DataInterface<Link>.
                        Push(new Link(this.ID, this.Name, this.ItemType));
                }
                else if (this.Name != Link.GetLink(this).LinkedName)
                {
                    var link = Link.GetLink(this);
                    link.LinkedName = this.Name;
                    link.Push();

                    DataInterface<InventoryItem>.Push(this);
                }
                else
                {
                    DataInterface<InventoryItem>.Push(this);
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

                DataInterface<InventoryItem>.Delete(this.ID);

                link.Delete();

                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion

        #region Additional NOT MAPPED Properties

        // EDIT INFO
        [NotMapped]
        public string EditInfo
        {
            get
            {
                string editorInfo;
                if (this.EditorID != 0)
                {
                    var editor = User.Pull(EditorID);

                    editorInfo = editor.FirstName + " " + editor.LastName;
                }
                else return "Изменений не было";

                return EditDate.ToString("dd-MM-yyyy в HH-mm") + " (" + editorInfo + ")";
            }
        }
        // ADD INFO
        [NotMapped]
        public string AddInfo
        {
            get
            {
                string adderInfo;
                if (this.AdderID != 0)
                {
                    var adder = User.Pull(AdderID);

                    adderInfo = adder.FirstName + " " + adder.LastName;
                }
                else adderInfo = String.Empty;

                return AddDate.ToString("dd-MM-yyyy в HH-mm") + " (" + adderInfo + ")";
            }
        }
        // LOCALIZED LAST TEST DATE
        [NotMapped]
        public string LocalizedStringLastTestDate
        {
            get
            {
                string output = LastTestDate.ToString("dd-MM-yyyy");

                return output == "01-01-0001" ? null : output;
            }
        }
        // LOCALIZED NEXT TEST DATE
        [NotMapped]
        public string LocalizedStringNextTestDate
        {
            get
            {
                string output = NextTestDate.ToString("dd-MM-yyyy");

                return output == "01-01-0001" ? null : output;
            }
        }
        // LOCALIZED ITEM TYPE
        [NotMapped]
        public string LocalizedItemType
        {
            get
            {
                switch(ItemType)
                {
                    case "MiscItem": return "Инвентарная позиция";
                    case "Tool": return "Средство измерений";
                    case "TestSystem": return "Испытательная установка";
                    default: return "Тип неопределен";
                }
            }
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
            }
        }

        #endregion
    }
}

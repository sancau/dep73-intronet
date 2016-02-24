namespace DB73.BL
{
    using DB73.Models;

    public static class InventoryLogic
    {
        public static LogicResponse AddInventoryItem(InventoryItem item)
        {
            // perpare item object for push
            item.AddDate = Server.CurrentTime;
            item.EditDate = item.AddDate;
            item.AdderID = Session.ActiveUser.ID;

            if (item.ItemType == "MiscItem")
            {
                item.LastTestDate = item.AddDate;
                item.NextTestDate = item.AddDate;
            }

            // validate input data
            if (!item.IsValid)
                return new LogicResponse(false, "invalid_data");
            try
            {
                // add item folder
                var itemFolder = new Folder(item);

                // fix if name of item contained bad characters
                if (item.Name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    foreach (var ch in System.IO.Path.GetInvalidFileNameChars())
                    {
                        itemFolder.Name = itemFolder.Name.Replace(ch.ToString(), "");
                    }
                }

                if (!itemFolder.IsValid)
                {
                    return new LogicResponse(false, "error_on_itemfolder_validation");
                }
                if (!itemFolder.Push()) return new LogicResponse(false, "error_on_itemfolder_push");

                item.ItemFolderID = itemFolder.ID;

                // push item
                if (item.Push())
                {
                    return new LogicResponse(true, "item_added");
                }
                else
                {
                    var itemFolderEntity = Folder.Pull(item.ItemFolderID);
                    itemFolderEntity.Delete();
                    return new LogicResponse(false, "error_on_item_push");
                }              
            }
            catch
            {
                return new LogicResponse(false, "exception");
            }
        }

        public static LogicResponse ModifyInventoryItem(InventoryItem item)
        {
            // validate input data
            if (!item.IsValid)
                return new LogicResponse(false, "invalid_data");
            try
            {
                // check if we should change item folder name
                var entity = InventoryItem.Pull(item.ID);
                if (item.Name != entity.Name)
                {
                    var itemFolder = Folder.Pull(item.ItemFolderID);
                    itemFolder.Name = item.Name;
                    if (!itemFolder.IsValid) return new LogicResponse(false, "error_on_itemfolder_validation");
                    if (!itemFolder.Push()) return new LogicResponse(false, "error_on_itemfolder_push");
                }

                // perpare item object for push
                item.EditDate = Server.CurrentTime;
                item.EditorID = Session.ActiveUser.ID;

                // push item
                if (item.Push())
                {
                    return new LogicResponse(true, "data_saved");
                }
                else
                    return new LogicResponse(false, "error_on_item_push");
            }
            catch
            {
                return new LogicResponse(false, "exception");
            }
        }
    }
}

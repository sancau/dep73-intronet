namespace DB73.Models
{
    using System;
    using System.IO;
    using System.Text;

    //This class provides application config data
    public static class AppConfig
    {
        //defines where the config data is stored 
        private const string VERSION_FILE = "config/buildInfo.conf";
        private const string DOCUMENT_FOLDER_FILE = "config/docFolder.conf";
        private const string APPLICATION_CREDITS_FILE = "config/applicationCredits.conf";
        private const string DATABASE_FILES_PATH = "config/databasePath.conf";

        //methods to retrive config data stored in config files
        public static string BuildVersion
        {
            get
            {
                string version = string.Empty;

                try
                {
                    version = File.ReadAllText(VERSION_FILE, Encoding.GetEncoding("utf-8"));

                    return version;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public static string DocumentFolderPath
        {
            get
            {
                string docPath = string.Empty;

                try
                {
                    docPath = File.ReadAllText(DOCUMENT_FOLDER_FILE, Encoding.GetEncoding("utf-8"));

                    if (!Directory.Exists(docPath))
                        Directory.CreateDirectory(docPath);

                    return docPath;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public static string DatabasePath
        {
            get
            {
                string databasePath = string.Empty;

                try
                {
                    databasePath = File.ReadAllText(DATABASE_FILES_PATH, Encoding.GetEncoding("utf-8"));

                    return databasePath;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        public static string ApplicationCredits
        {
            get
            {
                string creditsPath = string.Empty;

                try
                {
                    creditsPath = File.ReadAllText(APPLICATION_CREDITS_FILE, Encoding.GetEncoding("utf-8"));

                    return creditsPath;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        #region ON DB CREATION

        // on db creation 
        public static bool OnFirstRun()
        {
            try
            {
                var root = new Folder("generate_root");
                if (!root.Push()) return false; 

                var inventoryFolder = new Folder("generate_inventory_folder");
                if (!inventoryFolder.Push()) return false;

                var testSystemFolder = new Folder("generate_test_system_folder");
                if (!testSystemFolder.Push()) return false;

                var toolFolder = new Folder("generate_tool_folder");
                if (!toolFolder.Push()) return false;

                var miscItemFolder = new Folder("generate_misc_item_folder");
                if (!miscItemFolder.Push()) return false;

                var projectsFolder = new Folder("generate_projects_folder");
                if (!projectsFolder.Push()) return false;

                var rootEntity = Folder.Pull(root.ID);

                if (!rootEntity.Push()) return false;

                var inventoryFolderEntity = Folder.Pull(inventoryFolder.ID);

                if (!inventoryFolderEntity.Push()) return false;

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        // returns DocumentsRootFolderID
        public static int DocumentsRootFolderID
        {
            get { return 1; }
        }

        // returns InventoryRootFolderID
        public static int InventoryRootFolderID
        {
            get { return 2; }
        }

        // returns TestSystemFolderID
        public static int TestSystemFolderID
        {
            get { return 3; }
        }

        // returns ToolItemFolderID
        public static int ToolFolderID
        {
            get { return 4; }
        }

        // returns MiscItemFolderID
        public static int MiscItemFolderID
        {
            get { return 5; }
        }

        // returns ProjectsRootFolderID
        public static int ProjectsRootFolderID
        {
            get { return 6; }
        }

        // returns folderID for inventory item by its type
        public static int GetInventoryFolderID(string type)
        {
            switch (type)
            {
                case "MiscItem":
                    return MiscItemFolderID;
                case "Tool":
                    return ToolFolderID;
                case "TestSystem":
                    return TestSystemFolderID;
                default:
                    throw new Exception();
            }
        }

        #endregion
    }
}


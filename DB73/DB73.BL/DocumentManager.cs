namespace DB73.BL
{
    using DB73.Models;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Diagnostics;
    using System.Timers;

    /// <summary>
    /// This class provides BL methods to manage document and folder entities in the system
    /// This class is static
    /// </summary>
    public static class DocumentManager
    {
        #region Config properties

        // main document folder path
        private static string CurrentFolder
        {
            get
            {
                string path = AppConfig.DocumentFolderPath + "Current\\";

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        // recycle bin folder path
        private static string DeletedFolder
        {
            get
            {
                string path = AppConfig.DocumentFolderPath + "Deleted\\";

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }  

        // version history folder path
        private static string VersionHistoryFolder
        { 
            get
            {
                string path = AppConfig.DocumentFolderPath + "VersionHistory\\";

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        // read-only Flow
        public static string FlowPath
        {
            get
            {
                string path = "flow\\";

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        #endregion

        // returns "current" directory size
        public static long GetCurrentSize()
        {
            string[] allFiles = Directory.GetFiles(CurrentFolder, "*.*");

            long size = 0;
            foreach (string file in allFiles)
            {
                FileInfo info = new FileInfo(file);
                size += info.Length;
            }
            return size;
        }

        #region Folder and document management methods 

        // open file read only
        public static LogicResponse OpenReadOnly(Document document)
        {
            try
            {
                if (!document.DeclareRequest(Session.ActiveUser, document))
                    return new LogicResponse(false, "exception");

                var fullPath = GetFullPath(document.Path);

                var readOnlyRelativePath = GetStoragePath(document.Name + " ( ТОЛЬКО ДЛЯ ЧТЕНИЯ ) ", GetExtension(fullPath));

                var readOnlyFullPath = FlowPath + readOnlyRelativePath;

                if (CopyFile(fullPath, readOnlyFullPath))
                {
                    Process.Start(readOnlyFullPath);
                    return new LogicResponse(true);
                }
                else
                {
                    return new LogicResponse(false);
                }
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // start edit session
        public static LogicResponse<Process> OpenForEdit(Document document)
        {
            try
            {
                if (!document.DeclareRequest(Session.ActiveUser, document))
                    return new LogicResponse<Process>(false, "exception");

                if (!document.Hold(Session.ActiveUser, document))
                   return new LogicResponse<Process>(false, "document_is_not_free");

                var docFullPath = GetFullPath(document.Path);
                var editFullPath = FlowPath + document.Name 
                    + " ( АКТИВНАЯ СЕССИЯ РЕДАКТИРОВАНИЯ ) " + GetExtension(document.Path);

                if (CopyFile(docFullPath, editFullPath))
                {
                    var editProcess = new Process();

                    ProcessStartInfo psi = new ProcessStartInfo(editFullPath);
                    editProcess.StartInfo = psi;

                    editProcess.Start();
                                    
                    return new LogicResponse<Process>(true, "edit_session_started", editProcess);
                }
                else
                {
                    return new LogicResponse<Process>(false);
                }
            }
            catch(Exception)
            {
                return new LogicResponse<Process>(false);
            }
        }

        // end edit session 
        public static LogicResponse EndEditSession(Document document, string editFullPath)
        {
            if(MakeEditBackup(document))
            {
                try
                {
                    var docFullPath = GetFullPath(document.Path);
                    if (CopyFile(editFullPath, docFullPath))
                    {
                        // what to do on session close
                        var docEntity = Document.Pull(document.ID);
                        docEntity.IsBusy = false;
                        docEntity.HolderID = 0;
                        docEntity.EditDate = Server.CurrentTime;
                        docEntity.EditorID = Session.ActiveUser.ID;

                        docEntity.Push();

                        return new LogicResponse(true, "edit_session_closed");
                    }
                    else
                    {
                        return new LogicResponse(true, "edit_session_failed");
                    }
                }
                catch
                {
                    // what to do on error to prevent data loses  should be HERE

                    return new LogicResponse(true, "edit_session_failed");
                }
            }
            else
            {
                return new LogicResponse(false, "error_on_edit_backup");
            }           
        }

        // adds a document entity / document file 
        public static LogicResponse Add(string filePath, Document document, Folder parentFolder)
        {
            try
            {
                if (!File.Exists(filePath)) return new LogicResponse(false, "file_not_found");

                // building document 
                if (String.IsNullOrWhiteSpace(document.Name))
                {
                    document.Name = GetDocumentName(filePath);
                }

                document.Name = HandlePossibleDocumentNameCollision(document.Name);

                document.AdderID = Session.ActiveUser.ID;
                document.Type = GetDocumentType(filePath);
                document.ParentFolderID = parentFolder.ID;
                document.FolderList.Add(Folder.Pull(parentFolder.ID));
                document.AddDate = Server.CurrentTime;
                document.CreationDate = File.GetCreationTime(filePath);
                document.EditDate = File.GetLastWriteTime(filePath);
                document.LastAccessDate = File.GetLastAccessTime(filePath);
                document.Path = GetStoragePath(document.Name, GetExtension(filePath));

                if (!document.IsValid) 
                    return new LogicResponse(false, "invalid_data");

                // generate full path
                string fullPath = GetFullPath(document.Path);
                if (File.Exists(fullPath))
                    return new LogicResponse(false, "file_already_exists");

                // copy file to storage
                if (CopyFile(filePath, fullPath))
                {
                    // add entity
                    document.Push();
                }
                else
                {
                    return new LogicResponse(false, "copy_error");
                }

                return new LogicResponse(true, "document_added");
            }
            catch(Exception ex)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // adds a folder entity
        public static LogicResponse Add(Folder folder, Folder parentFolder)
        {
            try
            {
                folder.Name = HandlePossibleFolderNameCollision(folder.Name);

                folder.ParentFolderID = parentFolder.ID;
                folder.AdderID = Session.ActiveUser.ID;
                folder.AddDate = Server.CurrentTime;
                folder.EditDate = folder.AddDate;

                if (!folder.IsValid) 
                    return new LogicResponse(false, "invalid_data");

                if (folder.Push())
                {
                    return new LogicResponse(true, "folder_added");
                }
                else
                {
                    return new LogicResponse(false, "exception");
                }               
            }
            catch (Exception ex)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // deletes document and moves file to the basket
        public static LogicResponse Delete(Document document)
        {
            try
            {
                if (DeleteFile(document))
                {
                    document.Delete();
                    return new LogicResponse(true, "document_deleted");
                }
                return new LogicResponse(false, "error_on_document_delete");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // deletes folder and ALL IT'S SUBFOLDERS AND DOCUMENTS, move all theese docs to the basket
        public static LogicResponse Delete(Folder folder)
        {
            try
            {
                var subfolders = Folder.List.FindAll(f => f.ParentFolderID == folder.ID);
                var folderDocuments = Document.List.FindAll(d => d.ParentFolderID == folder.ID);

                folder.Delete();

                foreach(var doc in folderDocuments)
                {
                    if(!Delete(doc).IsDone) return new LogicResponse(false, "error_on_document_delete");
                } 

                foreach(var fol in subfolders)
                {
                    if(!Delete(fol).IsDone) return new LogicResponse(false, "error_on_folder_delete"); ;
                }
              
                return new LogicResponse(true, "folder_deleted");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // creates a document copy at a given location
        public static LogicResponse Copy(Document document, Folder targetFolder)
        {
            Document newDoc = new Document();

            try
            {
                var fullPath = GetFullPath(document.Path);

                newDoc.BusinessType = document.BusinessType;
                newDoc.Annotation = "Копия документа " + document.Name;
                newDoc.Name = GetCopyName(document.Name);

                Add(fullPath, newDoc, targetFolder);

                return new LogicResponse(true, "document_copied");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // Copies folder and its contains to a given location
        public static LogicResponse Copy(Folder folder, Folder targetFolder)
        {
            if (targetFolder.IsSubfolderFor(folder))
                return new LogicResponse(false, "cant_copy_to_own_child");
            try
            {
                var subfolders = Folder.List.FindAll(f => f.ParentFolderID == folder.ID);
                var folderDocuments = Document.List.FindAll(d => d.ParentFolderID == folder.ID);

                var newFolder = new Folder();
                newFolder.Name = GetCopyName(folder.Name);
                Add(newFolder, targetFolder);

                foreach(var doc in folderDocuments)
                {
                    Copy(doc, newFolder);
                }

                foreach(var subfolder in subfolders)
                {
                    Copy(subfolder, newFolder);
                }

                return new LogicResponse(true, "folder_copied");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // moves document to another folder
        public static LogicResponse Move(Document document, Folder targetFolder)
        {
            try
            {
                var documentEntity = Document.Pull(document.ID);

                documentEntity.FolderList.Remove(Folder.Pull(documentEntity.ParentFolderID));

                documentEntity.ParentFolderID = targetFolder.ID;

                documentEntity.FolderList.Add(Folder.Pull(documentEntity.ParentFolderID));

                documentEntity.Push();

                return new LogicResponse(true, "document_moved");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // moves folder to another folder
        public static LogicResponse Move(Folder folder, Folder targetFolder)
        {
            if (targetFolder.IsSubfolderFor(folder))
                return new LogicResponse(false, "cant_move_to_own_child");

            try
            {
                var folderEntity = Folder.Pull(folder.ID);              

                folderEntity.ParentFolderID = targetFolder.ID;
                folderEntity.Push();

                return new LogicResponse(true, "folder_moved");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // replaces document's file with new one by given path
        public static LogicResponse ReplaceFile(string inputFilePath, Document document)
        {
            string fullTargetPath = GetFullPath(document.Path);

            if (File.Exists(fullTargetPath))
            {
                try
                {
                    var inputType = GetExtension(inputFilePath);
                    var currentType = GetExtension(fullTargetPath);

                    if (inputType != currentType)
                    {
                        return new LogicResponse(false, "invalid_file_type");
                    }

                    File.Copy(inputFilePath, fullTargetPath, true);

                    document.EditDate = Server.CurrentTime;
                    document.EditorID = Session.ActiveUser.ID;
                    document.Push();

                    return new LogicResponse(true, "file_replaced");
                }
                catch
                {
                    return new LogicResponse(false, "exception");
                }
            }
            else
            {
                return new LogicResponse(false);
            }
        }

        #region Additional methods

        // checks if object copy exists in user private storage
        public static bool ExistsInPrivateStorage(Document document, string storagePath)
        {
            if (File.Exists(storagePath + "//" + document.Path))
                return true;
            else
                return false;
        }
        public static bool ExistsInPrivateStorage(Folder folder, string storagePath)
        {
            if (Directory.Exists(storagePath + "//" + folder.Name))
                return true;
            else
                return false;
        }
        
        // copies object to user private storage (overwrites if exists)
        public static LogicResponse CopyToPrivateStorage(Document document, string storagePath)
        {
            try
            {
                var fullPath = GetFullPath(document.Path);
                var fullStoragePath = storagePath + "//" + document.Path;
                
                if (CopyFile(fullPath, fullStoragePath))
                {
                    return new LogicResponse(true, "copied_to_private");
                }

                return new LogicResponse(false);
            }
            catch(Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }
        public static LogicResponse CopyToPrivateStorage(Folder folder, string storagePath)
        {
            return ExportTree(storagePath, folder);
        }

        #endregion

        #endregion

        #region Tree import and export 

        // imports file tree by given path, parent folder and access parameter
        public static LogicResponse ImportTree(string path, Folder parentFolder, List<User> users, string businessType)
         {
            try
            {
                List<User> accessList = new List<User>();
                
                //construct folder entity
                var folder = new Folder();
                folder.Name = GetFolderName(path);
                folder.UserList = users;
                folder.AddDate = Server.CurrentTime;
                folder.EditDate = folder.AddDate;

                Add(folder, parentFolder);

                //pick added entity from DB
                var added_folder = Folder.List.LastOrDefault();

                if (added_folder == null)
                {
                    added_folder = new Folder();
                }

                //add documents
                foreach (string filePath in Directory.GetFiles(path))
                {
                    var newDoc = new Document();
                    newDoc.BusinessType = businessType;
                    Add(filePath, newDoc, added_folder);                 
                }
                //recursively add folders
                foreach (string f in Directory.GetDirectories(path))
                {
                    ImportTree(f, added_folder, users, businessType);
                }

                return new LogicResponse(true, "tree_imported");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        // exports given folder and its subfolders and contains to a given location
        public static LogicResponse ExportTree(string path, Folder baseFolder)
        {
            try
            {
                var currentFolderPath = path + "\\" + baseFolder.Name + "\\";

                // create dir
                Directory.CreateDirectory(currentFolderPath);

                // folder documents
                var documentChildren = Document.List.FindAll(d => d.ParentFolderID == baseFolder.ID);

                foreach(var doc in documentChildren)
                {
                    CopyFile(GetFullPath(doc.Path), currentFolderPath + doc.Path);
                }

                // subfolders
                var folderChildren = Folder.List.FindAll(f => f.ParentFolderID == baseFolder.ID);

                foreach(var folder in folderChildren)
                {
                    ExportTree(currentFolderPath, folder);
                }

                return new LogicResponse(true, "tree_exported");
            }
            catch (Exception)
            {
                return new LogicResponse(false, "exception");
            }
        }

        #endregion 

        #region Service methods

        // makes edit backup to be proceed before edit opening
        private static bool MakeEditBackup(Document document)
        {
            try
            {
                var actualFullPath = GetFullPath(document.Path);

                var editName = GetEditBackupName(document.Name);
                var backupFullPath = VersionHistoryFolder +
                    GetStoragePath(editName, GetExtension(document.Path));

                if(!CopyFile(actualFullPath, backupFullPath))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }        

        // returns storage path for a document
        private static string GetStoragePath(string name, string extension)
        {
            return name + extension;
        }

        // returns file extension
        private static string GetExtension(string filePath)
        {
            try
            {
                int nameStartIndex = filePath.LastIndexOf('\\') + 1;
                int nameEndIndex = filePath.LastIndexOf('.');

                if (nameStartIndex > nameEndIndex) // so there's no extension
                {
                    return ".empty";
                }

                return filePath.Substring(filePath.LastIndexOf('.'));
            }
            catch
            {
                return ".empty";
            }
        }

        // returns file type
        private static string GetDocumentType(string filePath)
        {
            string fileExtention = GetExtension(filePath);
            string documentType;

            switch (fileExtention)
            {
                case ".txt": { documentType = "Текстовый файл"; }
                    break;
                case ".pdf": { documentType = "Документ PDF"; }
                    break;
                case ".doc": { documentType = "Документ Word"; }
                    break;
                case ".docx": { documentType = "Документ Word"; }
                    break;
                case ".xls": { documentType = "Документ Excel"; }
                    break;
                case ".xlsx": { documentType = "Документ Excel"; }
                    break;
                case ".jpg": { documentType = "Графический файл"; }
                    break;
                case ".bmp": { documentType = "Графический файл"; }
                    break;
                case ".png": { documentType = "Графический файл"; }
                    break;
                case ".gif": { documentType = "Графический файл"; }
                    break;
                case ".": { documentType = "Программа Votsch"; }
                    break;
                case ".empty": { documentType = "Без расширения"; }
                    break;
                default: { documentType = "Неопределенный тип"; }
                    break;
            }

            return documentType;
        }

        // returns full document path
        private static string GetFullPath(string relativePath)
        {
            return CurrentFolder + relativePath;
        }   

        // returns folder name by it's path
        private static string GetFolderName(string path)
        {
            string[] split;

            split = path.Split('\\');

            return split.LastOrDefault();
        }
        
        // returns document name by it's path
        public static string GetDocumentName(string inputFilePath)
        {
            int nameStartIndex = inputFilePath.LastIndexOf('\\') + 1;
            int nameEndIndex = inputFilePath.LastIndexOf('.');
            int nameLength = nameEndIndex - nameStartIndex;

            if (nameStartIndex > nameEndIndex) // so there's no extension
            {
                return inputFilePath.Substring(nameStartIndex);
            }

            if (nameLength > 0 && nameStartIndex > 0 && nameEndIndex > 0)
            {
                return inputFilePath.Substring(nameStartIndex, nameLength);
            }
            else
                return "empty";
        }

        // copies a given file to a given location
        private static bool CopyFile(string inputFilePath, string fullTargetPath)
        {
            try
            {
                File.Copy(inputFilePath, fullTargetPath, true);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // deletes given file
        private static bool DeleteFile(Document document)
        {
            try
            {
                var fullPath = GetFullPath(document.Path);

                if (File.Exists(fullPath))
                {
                    var deleteBackupPath = DeletedFolder + document.Path;

                    if (!CopyFile(fullPath, deleteBackupPath))
                        return false;

                    File.Delete(GetFullPath(document.Path));

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // modifies document name in case of the name repetiton
        private static string HandlePossibleDocumentNameCollision(string name)
        {
            int collisionsNumber = Document.List.FindAll(doc => doc.Name.ToLower() == name.ToLower()).Count;

            if (collisionsNumber != 0)
            {
                name = name + "(#)";
                name = HandlePossibleDocumentNameCollision(name);
            }
            return name;
        }

        // modifies document name in case of the name repetiton
        private static string HandlePossibleFolderNameCollision(string name)
        {
            int collisionsNumber = Folder.List.FindAll(f => f.Name.ToLower() == name.ToLower()).Count;

            if (collisionsNumber != 0)
            {
                name = name + "(#)";
                name = HandlePossibleFolderNameCollision(name);
            }
            return name;
        }

        // generates copy name for a folder or document
        private static string GetCopyName(string name)
        {
            return name +
                    "(копия от " +
                     Server.CurrentTime.ToShortDateString().Replace('.', '_').Replace(':', '_') + "__" +
                    Server.CurrentTime.ToLongTimeString().Replace('.', '_').Replace(':', '_') +
                    ")";
        }

        // generates edit backup name
        private static string GetEditBackupName(string name)
        {
            return name +
                    "(версия от " +
                    Server.CurrentTime.ToString().Replace('.', '_').Replace(':', '_') +
                    ")";
        }

        #endregion
    }
}

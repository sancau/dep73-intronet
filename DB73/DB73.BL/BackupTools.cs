namespace DB73.BL
{
    using DB73.Models;

    using System;
    using System.IO;

    public static class BackupTools
    {
        public static LogicResponse BackupSystem(string backupPath)
        {
            if (CopyFolder(AppConfig.DocumentFolderPath, backupPath) ||
                    CopyFolder(AppConfig.DatabasePath, backupPath))
            {
                return new LogicResponse(false, "error_on_backup");
            }

            return new LogicResponse(true, "backup_done");
        }

        private static bool CopyFolder(string fromPath, string toPath)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(fromPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(fromPath, toPath));
                }

                foreach (string newPath in Directory.GetFiles(fromPath, "*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(fromPath, toPath), true);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

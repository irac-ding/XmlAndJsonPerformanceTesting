/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RescueFileOperation.cs
 * Purpose:  Write and read file securely.
 * Author:   IracDing added on Oct.11th, 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.IO;
using NLog;

namespace TVU.SharedLib.GenericUtility.FileRescue
{
    public static class RescueFileOperation
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion Log

        private static string _backupFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup");
        private static string _tempFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");

        // TODO: Do we need to set an interface for the wirte & read action? And the json, xml operators to implement it.

        public static void RescueSave(object value, string fileName, Action<object, string> write, bool needRescue)
        {
            string tempFileName = DateTime.Now.ToFileTimeUtc().ToString() + "-" + Path.GetFileName(fileName);
            string tempFile = Path.Combine(_tempFilePath, tempFileName);
            string backupFile = Path.Combine(_backupFilePath, Path.GetFileName(fileName));

            FileExtensions.CheckAndCreateDirectory(_tempFilePath, _backupFilePath, Path.GetDirectoryName(fileName));
            write(value, tempFile);

            if (needRescue)
            {
                FileExtensions.BackupFile(fileName, tempFile, backupFile);
            }
            FileExtensions.SaveFile(fileName, tempFile);
        }

        public static T RescueRead<T>(string fileName, Func<string, T> read, bool needRescue)
        {
            T ret = default(T);
            FileExtensions.CheckAndCreateDirectory(_backupFilePath, Path.GetDirectoryName(fileName));
            try
            {
                ret = read(fileName);
            }
            catch (Exception exception)
            {
                if (!needRescue)
                {
                    logger.Error("RescueRead() error, {0}.", exception.Message);
                    return ret;
                }
                string filePath = FileExtensions.RestoreFile(fileName, _backupFilePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        File.Delete(fileName);
                        File.Copy(filePath, fileName);
                        ret = read(filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("RescueRead() error, {0}.", ex.Message);
                    }
                }
                else
                {
                    string message = $"Both {fileName} file and the backup{_backupFilePath} are invalid.";
                    logger.Error("RescueRead() error, {0}.", message);
                }
            }
            return ret;
        }
    }
}

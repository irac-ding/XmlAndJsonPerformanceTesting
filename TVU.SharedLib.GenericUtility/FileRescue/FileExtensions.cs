/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: FileExtensions.cs
 * Purpose:  Configuration file backup and recover.
 * Author:   IracDing added on Oct.11th, 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using NLog;

namespace TVU.SharedLib.GenericUtility.FileRescue
{
    public static class FileExtensions
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods

        public static string RestoreFile(string configFilePath, string backupFilePath)
        {
            string fileName = Path.GetFileName(configFilePath);
            string filePath = Path.Combine(backupFilePath, fileName);
            if (!CheckFileIsValid(filePath))
            {
                filePath = string.Empty;
            }
            return filePath;
        }

        public static void BackupFile(string configFile, string tempFile, string backupFile)
        {
            try
            {
                if (!CheckFileIsValid(tempFile))
                    return;

                File.Copy(tempFile, backupFile, true);
            }
            catch (Exception ex)
            {
                string message = $"BackupFile {tempFile} to {backupFile} error: {ex.Message} ";
                logger.Error(message);
                throw new Exception(message);
            }
        }

        public static void SaveFile(string configFile, string tempFile)
        {
            try
            {
                if (!CheckFileIsValid(tempFile))
                    return;

                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                    File.Move(tempFile, configFile);
                }
                else
                {
                    File.Move(tempFile, configFile);
                }
            }
            catch (Exception ex)
            {
                string message = $"Replace {tempFile} to {configFile} error: {ex.Message} ";
                logger.Error(message);
                throw new Exception(message);
            }
        }

        public static void CheckAndCreateDirectory(params string[] args)
        {
            if (args == null || args.Length == 0)
                return;

            foreach (string dir in args)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
            }
        }

        public static bool CheckFileIsValid(string fileName)
        {
            if (File.Exists(fileName))
            {
                string strExt = Path.GetExtension(fileName).ToLower();
                switch (strExt)
                {
                    case ".json":
                        return IsVaildJson(fileName);
                    case ".xml":
                        return IsValidXML(fileName);
                    default:
                        return true;
                }
            }
            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private static bool IsValidXML(string fileName)
        {
            try
            {
                XDocument.Load(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsVaildJson(string fileName)
        {
            using (TextReader reader = new StreamReader(fileName))
            {
                try
                {
                    string jsonString = reader.ReadToEnd();
                    JToken.Parse(jsonString);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion Private Methods
    }
}

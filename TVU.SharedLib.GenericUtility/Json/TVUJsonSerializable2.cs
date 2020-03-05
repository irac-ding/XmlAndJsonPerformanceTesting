/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUJsonSerializable2.cs
 * Purpose:  Simplify Json serialization to/from file.
 * Author:   MikkoXU added on July.27th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.IO;
using Newtonsoft.Json;
using NLog;
using TVU.SharedLib.GenericUtility.FileRescue;

namespace TVU.SharedLib.Json
{
    public class TVUJsonSerializable2
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region To/from file

        /// <summary>
        /// Serialize one object to file with backup.
        /// </summary>
        /// <param name="value">object instance value</param>
        /// <param name="fileName">the destination file name</param>
        /// <param name="needRescue"> true: backup the json file</param>
        /// <returns>whether the method was executed successfully or not</returns>
        public static bool SerializeObjectToFile(object value, string fileName, bool needRescue = false)
        {
            bool ret = false;
            try
            {
                RescueFileOperation.RescueSave(value, fileName, Write, needRescue);
                ret = true;
            }
            catch (Exception ex)
            {
                logger.Error($"SerializeObjectToFile: {fileName}, error message: {ex.Message}");
            }
            return ret;
        }

        public static T DeserializeObjectFromFile<T>(string fileName, bool needRescue = false)
        {
            T ret = default(T);
            string rawString = string.Empty;
            logger.Debug("DeserializeObjectFromFile() file:{0}", fileName);
            try
            {
                ret = RescueFileOperation.RescueRead(fileName, Read<T>, needRescue);
            }
            catch (Exception ex)
            {
                logger.Error("DeserializeObjectFromFile() error, {0}.", ex.Message);
            }
            return ret;
        }

        #endregion

        #region Read & Write

        private static T Read<T>(string fileName)
        {
            T ret = default(T);
            string rawString = string.Empty;
            using (TextReader reader = new StreamReader(fileName))
            {
                rawString = reader.ReadToEnd();
            }
            ret = JsonConvert.DeserializeObject<T>(rawString);
            return ret;
        }

        private static void Write(object value, string fileName)
        {
            using (TextWriter writer = new StreamWriter(fileName))
            {
                string rawString = JsonConvert.SerializeObject(value, Formatting.Indented);
                writer.Write(rawString);
            }
        }

        #endregion Read & Write
    }
}

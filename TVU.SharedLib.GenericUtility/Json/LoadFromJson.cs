using System;
using System.IO;
using Newtonsoft.Json.Linq;
using NLog;
using TVU.SharedLib.GenericUtility.FileRescue;

namespace TVU.SharedLib.Json
{
    public class LoadFromJson
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static string TryLoadFromJson(string fileFullName, bool needRescue = true)
        {
            string rawString = string.Empty;
            try
            {
                rawString = RescueFileOperation.RescueRead(fileFullName, Read, needRescue);
                return rawString;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return rawString;
            }
        }

        private static string Read(string fileFullName)
        {
            string rawString = string.Empty;
            using (FileStream fs = new FileStream(fileFullName, FileMode.Open))
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(fs);
                    rawString = sr.ReadToEnd();
                    // Validity check.
                    JToken.Parse(rawString);
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }
            }
            return rawString;
        }
    }
}

/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: TVUXmlSerializable2.cs
 * Purpose:  To make it possible that calling methods defined in the class without inheriting from it.
 * Author:   EllaLiu added on Apr.22th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NLog;
using TVU.SharedLib.GenericUtility.FileRescue;

namespace TVU.SharedLib.Xml
{
    public sealed class TVUXmlSerializable2
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private static XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        #endregion

        #region Constructors

        static TVUXmlSerializable2()
        {
            ns.Add(string.Empty, string.Empty);
        }

        #endregion

        #region To/from file

        /// <summary>
        /// Serialize one object to file with backup.
        /// </summary>
        /// <param name="value">object instance value</param>
        /// <param name="fileName">the destination file name</param>
        /// <param name="needRescue">true: backup the xml file</param>
        /// <returns>whether the method was executed successfully or not</returns>
        public static bool SerializeObjectToFile(object value, string fileName, bool needRescue = true)
        {
            bool ret = false;
            try
            {
                RescueFileOperation.RescueSave(value, fileName, Write, needRescue);
                ret = true;
            }
            catch (Exception ex)
            {
                logger.Error("SerializeObjectToFile() error:{0}", ex.Message);
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Deserialize one object base on a file and return the file name which used to do real deserializer
        /// If neither the file or backup file exists, the sourcefile would be empty and one default object instance would be returned.
        /// </summary>
        /// <typeparam name="T"> Type which would be deserialized to</typeparam>
        /// <param name="fileName">first option to be used to deserialize to an object</param>
        /// <param name="sourceFile">the file name which used to deserializer</param>
        /// <param name="sourceFile">true: try to restore from backup</param>
        /// <returns>return an sepecified object instance</returns>
        public static T DeserializeObjectFromFile<T>(string fileName, bool needRescue = true)
        {
            T ret = default(T);

            logger.Debug("DeserializeObjectFromFile() file:{0}", fileName);
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            try
            {
                ret = RescueFileOperation.RescueRead<T>(fileName, Read<T>, needRescue);
            }
            catch (Exception ex)
            {
                logger.Error("DeserializeObjectFromFile() error, {0}.", ex.Message);
            }
            return ret;
        }

        #endregion

        #region To/from string

        public static string SerializeObjectToString(object value)
        {
            XmlSerializer serializer = new XmlSerializer(value.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, value, ns);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static string SerializeObjectToString(object value, bool encoderShouldEmitUTF8Identifier)
        {
            XmlSerializer serializer = new XmlSerializer(value.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier)))
                {
                    serializer.Serialize(writer, value, ns);
                    return Encoding.UTF8.GetString((writer.BaseStream as MemoryStream).ToArray());
                }
            }
        }

        public static T DeserializeObjectFromString<T>(string rawXml)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(rawXml)))
            {
                object obj = deserializer.Deserialize(ms);
                T ret = (T)obj;
                return ret;
            }
        }

        #endregion

        #region from XmlNode

        [Obsolete]
        public static T DeserializeObjectFroXmlNode<T>(XmlNode node) where T : class
        {
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            T result = (ser.Deserialize(stm) as T);

            return result;
        }

        #endregion

        #region Read & Write

        private static T Read<T>(string fileName)
        {
            T ret = default(T);
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StreamReader(fileName))
            {
                object obj = deserializer.Deserialize(reader);
                ret = (T)obj;
            }
            return ret;
        }

        private static void Write(object value, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(value.GetType());
            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, value, ns);
            }
        }

        #endregion Read & Write
    }
}

/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: TVUXmlSerializable.cs
 * Purpose:  Base class for XML serialization/deserialization.
 * Author:   MikkoXU added on Dec.9th, 2014.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NLog;

namespace TVU.SharedLib.Xml
{
    [Obsolete]
    public abstract class TVUXmlSerializable
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private static XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        #endregion

        #region Constructors

        static TVUXmlSerializable()
        {
            ns.Add(string.Empty, string.Empty);
        }

        #endregion

        #region To/from file

        protected void SerializeObject<T>(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, this, ns);
            }
        }

        protected static T DeserializeObject<T>(string fileName)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StreamReader(fileName))
            {
                object obj = deserializer.Deserialize(reader);
                T ret = (T)obj;
                return ret;
            }
        }

        #endregion

        #region To/from string

        protected string SerializeObjectToString<T>()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, this, ns);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        protected static T DeserializeObjectFromString<T>(string rawXml)
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
    }
}
